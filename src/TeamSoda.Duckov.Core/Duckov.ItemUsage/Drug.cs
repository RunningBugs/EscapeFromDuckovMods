using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.ItemUsage;

[MenuPath("医疗/药")]
public class Drug : UsageBehavior
{
	public int healValue;

	[LocalizationKey("Default")]
	public string healValueDescriptionKey = "Usage_HealValue";

	[LocalizationKey("Default")]
	public string durabilityUsageDescriptionKey = "Usage_Durability";

	public bool useDurability;

	public float durabilityUsage;

	public bool canUsePart;

	public override DisplaySettingsData DisplaySettings
	{
		get
		{
			DisplaySettingsData result = new DisplaySettingsData
			{
				display = true,
				description = $"{healValueDescriptionKey.ToPlainText()} : {healValue}"
			};
			if (useDurability)
			{
				result.description += $" ({durabilityUsageDescriptionKey.ToPlainText()} : {durabilityUsage})";
			}
			return result;
		}
	}

	public override bool CanBeUsed(Item item, object user)
	{
		CharacterMainControl characterMainControl = user as CharacterMainControl;
		if (!characterMainControl)
		{
			return false;
		}
		if (!CheckCanHeal(characterMainControl))
		{
			return false;
		}
		return true;
	}

	protected override void OnUse(Item item, object user)
	{
		CharacterMainControl characterMainControl = user as CharacterMainControl;
		if (!characterMainControl)
		{
			return;
		}
		float num = healValue;
		if (useDurability && item.UseDurability)
		{
			float num2 = durabilityUsage;
			if (canUsePart)
			{
				num = characterMainControl.Health.MaxHealth - characterMainControl.Health.CurrentHealth;
				if (num > (float)healValue)
				{
					num = healValue;
				}
				num2 = num / (float)healValue * durabilityUsage;
				if (num2 > item.Durability)
				{
					num2 = item.Durability;
					num = (float)healValue * item.Durability / durabilityUsage;
				}
				Debug.Log($"治疗：{num}耐久消耗：{num2}");
				item.Durability -= num2;
			}
		}
		Heal(characterMainControl, item, num);
	}

	private bool CheckCanHeal(CharacterMainControl character)
	{
		if (healValue > 0 && character.Health.CurrentHealth >= character.Health.MaxHealth)
		{
			return false;
		}
		return true;
	}

	private void Heal(CharacterMainControl character, Item selfItem, float _healValue)
	{
		if (_healValue > 0f)
		{
			character.AddHealth(Mathf.CeilToInt(_healValue));
		}
		else if (_healValue < 0f)
		{
			DamageInfo damageInfo = new DamageInfo(null);
			damageInfo.damageValue = 0f - _healValue;
			damageInfo.damagePoint = character.transform.position;
			damageInfo.damageNormal = Vector3.up;
			character.Health.Hurt(damageInfo);
		}
	}
}
