using System;
using System.Text;
using ItemStatsSystem;
using Sirenix.OdinInspector;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.PerkTrees;

public class Perk : MonoBehaviour, ISelfValidator
{
	[SerializeField]
	private PerkTree master;

	[SerializeField]
	private bool lockInDemo;

	[SerializeField]
	private Sprite icon;

	[SerializeField]
	private DisplayQuality quality;

	[LocalizationKey("Perks")]
	[SerializeField]
	private string displayName = "未命名技能";

	[SerializeField]
	private bool hasDescription;

	[SerializeField]
	private PerkRequirement requirement;

	[SerializeField]
	private bool defaultUnlocked;

	[SerializeField]
	internal bool unlocking;

	[DateTime]
	[SerializeField]
	internal long unlockingBeginTimeRaw;

	[SerializeField]
	private bool _unlocked;

	public bool LockInDemo => lockInDemo;

	public DisplayQuality DisplayQuality => quality;

	public Sprite Icon => icon;

	[LocalizationKey("Perks")]
	private string description
	{
		get
		{
			if (!hasDescription)
			{
				return string.Empty;
			}
			return displayName + "_Desc";
		}
		set
		{
		}
	}

	public string DisplayName => displayName.ToPlainText();

	public string Description
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			string value = description.ToPlainText();
			if (!string.IsNullOrEmpty(value))
			{
				stringBuilder.AppendLine(value);
			}
			PerkBehaviour[] components = GetComponents<PerkBehaviour>();
			for (int i = 0; i < components.Length; i++)
			{
				string value2 = components[i].Description;
				if (!string.IsNullOrEmpty(value2))
				{
					stringBuilder.AppendLine(value2);
				}
			}
			return stringBuilder.ToString();
		}
	}

	public PerkRequirement Requirement => requirement;

	public bool DefaultUnlocked => defaultUnlocked;

	private DateTime UnlockingBeginTime
	{
		get
		{
			DateTime dateTime = DateTime.FromBinary(unlockingBeginTimeRaw);
			if (dateTime > DateTime.UtcNow)
			{
				dateTime = DateTime.UtcNow;
				unlockingBeginTimeRaw = DateTime.UtcNow.ToBinary();
				GameManager.TimeTravelDetected();
			}
			return dateTime;
		}
	}

	public bool Unlocked
	{
		get
		{
			return _unlocked;
		}
		internal set
		{
			_unlocked = value;
			this.onUnlockStateChanged?.Invoke(this, value);
		}
	}

	public bool Unlocking => unlocking;

	public PerkTree Master
	{
		get
		{
			return master;
		}
		internal set
		{
			master = value;
		}
	}

	public string DisplayNameRaw => displayName;

	public string DescriptionRaw => description;

	public event Action<Perk, bool> onUnlockStateChanged;

	public static event Action<Perk> OnPerkUnlockConfirmed;

	public bool AreAllParentsUnlocked()
	{
		return master.AreAllParentsUnlocked(this);
	}

	private void OnValidate()
	{
		if (master == null)
		{
			master = GetComponentInParent<PerkTree>();
		}
	}

	private bool CheckAndPay()
	{
		if (requirement == null)
		{
			return true;
		}
		if (EXPManager.Level < requirement.level)
		{
			return false;
		}
		if (!requirement.cost.Pay())
		{
			return false;
		}
		return true;
	}

	public bool SubmitItemsAndBeginUnlocking()
	{
		if (Unlocked)
		{
			Debug.LogError("Perk " + displayName + " already unlocked!");
			return false;
		}
		if (!CheckAndPay())
		{
			return false;
		}
		unlocking = true;
		unlockingBeginTimeRaw = DateTime.UtcNow.ToBinary();
		master.NotifyChildStateChanged(this);
		this.onUnlockStateChanged?.Invoke(this, _unlocked);
		return true;
	}

	public bool ConfirmUnlock()
	{
		if (Unlocked)
		{
			return false;
		}
		if (!unlocking)
		{
			return false;
		}
		if (DateTime.UtcNow - UnlockingBeginTime < requirement.RequireTime)
		{
			return false;
		}
		Unlocked = true;
		unlocking = false;
		master.NotifyChildStateChanged(this);
		Perk.OnPerkUnlockConfirmed?.Invoke(this);
		return true;
	}

	public bool ForceUnlock()
	{
		if (Unlocked)
		{
			return false;
		}
		Debug.Log("Unlock default:" + displayName);
		Unlocked = true;
		unlocking = false;
		master.NotifyChildStateChanged(this);
		return true;
	}

	public TimeSpan GetRemainingTime()
	{
		if (Unlocked)
		{
			return TimeSpan.Zero;
		}
		if (!unlocking)
		{
			return TimeSpan.Zero;
		}
		TimeSpan timeSpan = DateTime.UtcNow - UnlockingBeginTime;
		TimeSpan timeSpan2 = requirement.RequireTime - timeSpan;
		if (timeSpan2 < TimeSpan.Zero)
		{
			return TimeSpan.Zero;
		}
		return timeSpan2;
	}

	public float GetProgress01()
	{
		TimeSpan remainingTime = GetRemainingTime();
		double totalSeconds = requirement.RequireTime.TotalSeconds;
		if (totalSeconds <= 0.0)
		{
			return 1f;
		}
		double totalSeconds2 = remainingTime.TotalSeconds;
		return 1f - (float)(totalSeconds2 / totalSeconds);
	}

	public void Validate(SelfValidationResult result)
	{
		if (master == null)
		{
			result.AddWarning("未指定PerkTree");
		}
		if (!master)
		{
			return;
		}
		if (!master.Perks.Contains(this))
		{
			result.AddError("PerkTree未包含此Perk").WithFix(delegate
			{
				master.perks.Add(this);
			});
		}
		if (master?.RelationGraphOwner?.GetRelatedNode(this) == null)
		{
			result.AddError("未在Graph中指定技能的关系");
		}
	}

	internal Vector2 GetLayoutPosition()
	{
		if (master == null)
		{
			return Vector2.zero;
		}
		return (master.RelationGraphOwner?.GetRelatedNode(this)).cachedPosition;
	}

	internal void NotifyParentStateChanged()
	{
		this.onUnlockStateChanged?.Invoke(this, Unlocked);
	}
}
