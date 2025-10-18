using Duckov.Buffs;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.ItemUsage;

public class AddBuff : UsageBehavior
{
	public Buff buffPrefab;

	[Range(0.01f, 1f)]
	public float chance = 1f;

	[LocalizationKey("Default")]
	private string chanceKey = "UI_AddBuffChance";

	public override DisplaySettingsData DisplaySettings
	{
		get
		{
			DisplaySettingsData result = default(DisplaySettingsData);
			result.display = true;
			result.description = "";
			result.description = buffPrefab.DisplayName ?? "";
			if (buffPrefab.LimitedLifeTime)
			{
				result.description += $" : {buffPrefab.TotalLifeTime}s ";
			}
			if (chance < 1f)
			{
				result.description += $" ({chanceKey.ToPlainText()} : {Mathf.RoundToInt(chance * 100f)}%)";
			}
			return result;
		}
	}

	public override bool CanBeUsed(Item item, object user)
	{
		return true;
	}

	protected override void OnUse(Item item, object user)
	{
		CharacterMainControl characterMainControl = user as CharacterMainControl;
		if (!(characterMainControl == null) && !(Random.Range(0f, 1f) > chance))
		{
			characterMainControl.AddBuff(buffPrefab, characterMainControl);
		}
	}
}
