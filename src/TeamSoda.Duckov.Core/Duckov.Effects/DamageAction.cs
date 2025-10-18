using Duckov.Buffs;
using ItemStatsSystem;
using UnityEngine;

namespace Duckov.Effects;

public class DamageAction : EffectAction
{
	[SerializeField]
	private Buff buff;

	[SerializeField]
	private bool percentDamage;

	[SerializeField]
	private float damageValue = 1f;

	[SerializeField]
	private float percentDamageValue;

	[SerializeField]
	private DamageInfo damageInfo = new DamageInfo(null);

	[SerializeField]
	private GameObject fx;

	private CharacterMainControl MainControl => base.Master?.Item?.GetCharacterMainControl();

	protected override void OnTriggeredPositive()
	{
		if (!(MainControl == null) && !(MainControl.Health == null))
		{
			damageInfo.isFromBuffOrEffect = true;
			if (buff != null)
			{
				damageInfo.fromCharacter = buff.fromWho;
				damageInfo.fromWeaponItemID = buff.fromWeaponID;
			}
			damageInfo.damagePoint = MainControl.transform.position + Vector3.up * 0.8f;
			damageInfo.damageNormal = Vector3.up;
			if (percentDamage && MainControl.Health != null)
			{
				damageInfo.damageValue = percentDamageValue * MainControl.Health.MaxHealth * ((buff == null) ? 1f : ((float)buff.CurrentLayers));
			}
			else
			{
				damageInfo.damageValue = damageValue * ((buff == null) ? 1f : ((float)buff.CurrentLayers));
			}
			MainControl.Health.Hurt(damageInfo);
			if ((bool)fx)
			{
				Object.Instantiate(fx, damageInfo.damagePoint, Quaternion.identity);
			}
		}
	}
}
