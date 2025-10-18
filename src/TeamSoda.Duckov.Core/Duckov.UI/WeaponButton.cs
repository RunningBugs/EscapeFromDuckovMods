using System;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using LeTai.TrueShadow;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Duckov.UI;

public class WeaponButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private string targetSlotKey = "PrimaryWeapon";

	[SerializeField]
	private GameObject displayParent;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TrueShadow iconShadow;

	[SerializeField]
	private GameObject selectionFrame;

	public UnityEvent<WeaponButton> onClick;

	public UnityEvent<WeaponButton> onRefresh;

	public UnityEvent<WeaponButton> onSelected;

	private CharacterMainControl _character;

	private Slot _targetSlot;

	private bool isBeingDestroyed;

	private CharacterMainControl Character => _character;

	private Slot TargetSlot => _targetSlot;

	private Item TargetItem => TargetSlot?.Content;

	private bool IsSelected
	{
		get
		{
			if (TargetItem?.ActiveAgent != null)
			{
				return TargetItem.ActiveAgent == _character.agentHolder?.CurrentHoldItemAgent;
			}
			return false;
		}
	}

	public static event Action<WeaponButton> OnWeaponButtonSelected;

	private void Awake()
	{
		RegisterStaticEvents();
		if (LevelManager.Instance?.MainCharacter != null)
		{
			Initialize(LevelManager.Instance.MainCharacter);
		}
	}

	private void OnDestroy()
	{
		UnregisterStaticEvents();
		isBeingDestroyed = true;
	}

	private void RegisterStaticEvents()
	{
		LevelManager.OnLevelInitialized += OnLevelInitialized;
		CharacterMainControl.OnMainCharacterChangeHoldItemAgentEvent = (Action<CharacterMainControl, DuckovItemAgent>)Delegate.Combine(CharacterMainControl.OnMainCharacterChangeHoldItemAgentEvent, new Action<CharacterMainControl, DuckovItemAgent>(OnMainCharacterChangeHoldItemAgent));
	}

	private void UnregisterStaticEvents()
	{
		LevelManager.OnLevelInitialized -= OnLevelInitialized;
		CharacterMainControl.OnMainCharacterChangeHoldItemAgentEvent = (Action<CharacterMainControl, DuckovItemAgent>)Delegate.Remove(CharacterMainControl.OnMainCharacterChangeHoldItemAgentEvent, new Action<CharacterMainControl, DuckovItemAgent>(OnMainCharacterChangeHoldItemAgent));
	}

	private void OnMainCharacterChangeHoldItemAgent(CharacterMainControl control, DuckovItemAgent agent)
	{
		if ((bool)_character && control == _character)
		{
			Refresh();
		}
	}

	private void OnLevelInitialized()
	{
		Initialize(LevelManager.Instance?.MainCharacter);
	}

	private void Initialize(CharacterMainControl character)
	{
		UnregisterEvents();
		_character = character;
		if (character == null)
		{
			Debug.LogError("Character 不存在，初始化失败");
		}
		if (character.CharacterItem == null)
		{
			Debug.LogError("Character item 不存在，初始化失败");
		}
		_targetSlot = character.CharacterItem.Slots.GetSlot(targetSlotKey);
		if (_targetSlot == null)
		{
			Debug.LogError("Slot " + targetSlotKey + " 不存在，初始化失败");
		}
		RegisterEvents();
		Refresh();
	}

	private void RegisterEvents()
	{
		if (_targetSlot != null)
		{
			_targetSlot.onSlotContentChanged += OnSlotContentChanged;
		}
	}

	private void UnregisterEvents()
	{
		if (_targetSlot != null)
		{
			_targetSlot.onSlotContentChanged -= OnSlotContentChanged;
		}
	}

	private void OnSlotContentChanged(Slot slot)
	{
		Refresh();
	}

	private void Refresh()
	{
		if (!isBeingDestroyed)
		{
			displayParent.SetActive(TargetItem);
			bool isSelected = IsSelected;
			if ((bool)TargetItem)
			{
				icon.sprite = TargetItem.Icon;
				(float, Color, bool) shadowOffsetAndColorOfQuality = GameplayDataSettings.UIStyle.GetShadowOffsetAndColorOfQuality(TargetItem.DisplayQuality);
				iconShadow.Inset = shadowOffsetAndColorOfQuality.Item3;
				iconShadow.Color = shadowOffsetAndColorOfQuality.Item2;
				iconShadow.OffsetDistance = shadowOffsetAndColorOfQuality.Item1;
				selectionFrame.SetActive(isSelected);
			}
			onRefresh?.Invoke(this);
			if (isSelected)
			{
				onSelected?.Invoke(this);
				WeaponButton.OnWeaponButtonSelected?.Invoke(this);
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!(Character == null))
		{
			onClick?.Invoke(this);
		}
	}
}
