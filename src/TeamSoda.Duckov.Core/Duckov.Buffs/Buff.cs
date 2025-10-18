using System;
using System.Collections.Generic;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.Events;

namespace Duckov.Buffs;

public class Buff : MonoBehaviour
{
	public enum BuffExclusiveTags
	{
		NotExclusive,
		Bleeding,
		Starve,
		Thirsty,
		Weight,
		Poison,
		Pain,
		Electric,
		Burning,
		Space,
		StormProtection,
		Nauseous,
		Stun
	}

	[SerializeField]
	private int id;

	[SerializeField]
	private int maxLayers = 1;

	[SerializeField]
	private BuffExclusiveTags exclusiveTag;

	[Tooltip("优先级高的代替优先级低的。同优先级，选剩余时间长的。如果一方不限制时长，选后来的")]
	[SerializeField]
	private int exclusiveTagPriority;

	[LocalizationKey("Buffs")]
	[SerializeField]
	private string displayName;

	[LocalizationKey("Buffs")]
	[SerializeField]
	private string description;

	[SerializeField]
	private Sprite icon;

	[SerializeField]
	private bool limitedLifeTime;

	[SerializeField]
	private float totalLifeTime;

	[SerializeField]
	private List<Effect> effects = new List<Effect>();

	[SerializeField]
	private bool hide;

	[SerializeField]
	private int currentLayers = 1;

	private CharacterBuffManager master;

	public UnityEvent OnSetupEvent;

	[SerializeField]
	private GameObject buffFxPfb;

	private GameObject buffFxInstance;

	[HideInInspector]
	public CharacterMainControl fromWho;

	public int fromWeaponID;

	private float timeWhenStarted;

	public BuffExclusiveTags ExclusiveTag => exclusiveTag;

	public int ExclusiveTagPriority => exclusiveTagPriority;

	public bool Hide => hide;

	public CharacterMainControl Character => master?.Master;

	private Item CharacterItem => master?.Master?.CharacterItem;

	public int ID
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public int CurrentLayers
	{
		get
		{
			return currentLayers;
		}
		set
		{
			currentLayers = value;
			this.OnLayerChangedEvent?.Invoke();
		}
	}

	public int MaxLayers => maxLayers;

	public string DisplayName => displayName.ToPlainText();

	public string DisplayNameKey => displayName;

	public string Description => description.ToPlainText();

	public Sprite Icon => icon;

	public bool LimitedLifeTime => limitedLifeTime;

	public float TotalLifeTime => totalLifeTime;

	public float CurrentLifeTime => Time.time - timeWhenStarted;

	public float RemainingTime
	{
		get
		{
			if (!limitedLifeTime)
			{
				return float.PositiveInfinity;
			}
			return totalLifeTime - CurrentLifeTime;
		}
	}

	public bool IsOutOfTime
	{
		get
		{
			if (!limitedLifeTime)
			{
				return false;
			}
			return CurrentLifeTime >= totalLifeTime;
		}
	}

	public event Action OnLayerChangedEvent;

	internal void Setup(CharacterBuffManager manager)
	{
		master = manager;
		timeWhenStarted = Time.time;
		base.transform.SetParent(CharacterItem.transform, worldPositionStays: false);
		if ((bool)buffFxInstance)
		{
			UnityEngine.Object.Destroy(buffFxInstance.gameObject);
		}
		if ((bool)buffFxPfb && (bool)manager.Master && (bool)manager.Master.characterModel)
		{
			buffFxInstance = UnityEngine.Object.Instantiate(buffFxPfb);
			Transform armorSocket = manager.Master.characterModel.ArmorSocket;
			if (armorSocket == null)
			{
				armorSocket = manager.Master.transform;
			}
			buffFxInstance.transform.SetParent(armorSocket);
			buffFxInstance.transform.position = armorSocket.position;
			buffFxInstance.transform.localRotation = Quaternion.identity;
		}
		foreach (Effect effect in effects)
		{
			effect.SetItem(CharacterItem);
		}
		OnSetup();
		OnSetupEvent?.Invoke();
	}

	internal void NotifyUpdate()
	{
		OnUpdate();
	}

	internal void NotifyOutOfTime()
	{
		OnNotifiedOutOfTime();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	internal virtual void NotifyIncomingBuffWithSameID(Buff incomingPrefab)
	{
		timeWhenStarted = Time.time;
		if (CurrentLayers < maxLayers)
		{
			CurrentLayers += incomingPrefab.CurrentLayers;
		}
	}

	protected virtual void OnSetup()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void OnNotifiedOutOfTime()
	{
	}

	private void OnDestroy()
	{
		if ((bool)buffFxInstance)
		{
			UnityEngine.Object.Destroy(buffFxInstance.gameObject);
		}
	}
}
