using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.ItemUsage;

[MenuPath("概率死亡")]
public class DeadByChance : UsageBehavior
{
	public int damageValue = 9999;

	public float chance;

	[LocalizationKey("Default")]
	public string descriptionKey = "Usage_DeadByChance";

	[LocalizationKey("Default")]
	public string popTextKey = "Usage_DeadByChance_PopText";

	public override DisplaySettingsData DisplaySettings => new DisplaySettingsData
	{
		display = true,
		description = $"{descriptionKey.ToPlainText()}:  {chance * 100f:0}%"
	};

	public override bool CanBeUsed(Item item, object user)
	{
		if (!(user as CharacterMainControl))
		{
			return false;
		}
		return true;
	}

	protected override void OnUse(Item item, object user)
	{
		CharacterMainControl characterMainControl = user as CharacterMainControl;
		if ((bool)characterMainControl && !(Random.Range(0f, 1f) > chance))
		{
			KillSelf(characterMainControl, item.TypeID).Forget();
		}
	}

	private async UniTaskVoid KillSelf(CharacterMainControl character, int weaponID)
	{
		DamageInfo dmgInfo = default(DamageInfo);
		dmgInfo.fromCharacter = character;
		dmgInfo.fromWeaponItemID = weaponID;
		dmgInfo.damageValue = 1f;
		dmgInfo.ignoreArmor = true;
		dmgInfo.isFromBuffOrEffect = true;
		dmgInfo.elementFactors = new List<ElementFactor>();
		dmgInfo.elementFactors.Add(new ElementFactor(ElementTypes.poison, 1f));
		await UniTask.WaitForSeconds(0.5f);
		if (!character)
		{
			return;
		}
		character.PopText(popTextKey.ToPlainText());
		await UniTask.WaitForSeconds(1f);
		if (!character)
		{
			return;
		}
		character.Health.Hurt(dmgInfo);
		await UniTask.WaitForSeconds(1f);
		if (!character)
		{
			return;
		}
		character.Health.Hurt(dmgInfo);
		character.PopText("????");
		await UniTask.WaitForSeconds(1f);
		if (!character)
		{
			return;
		}
		character.Health.Hurt(dmgInfo);
		await UniTask.WaitForSeconds(1f);
		if ((bool)character)
		{
			character.Health.Hurt(dmgInfo);
			await UniTask.WaitForSeconds(1f);
			if ((bool)character)
			{
				dmgInfo.damageValue = damageValue;
				character.Health.Hurt(dmgInfo);
			}
		}
	}
}
