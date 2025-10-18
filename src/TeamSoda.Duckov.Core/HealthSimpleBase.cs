using System;
using UnityEngine;

public class HealthSimpleBase : MonoBehaviour
{
	public Teams team;

	public bool onlyReceiveExplosion;

	public float maxHealthValue = 250f;

	private float healthValue;

	public DamageReceiver dmgReceiver;

	public float damageMultiplierIfNotMainCharacter = 1f;

	public float HealthValue => healthValue;

	public event Action<DamageInfo> OnHurtEvent;

	public static event Action<HealthSimpleBase, DamageInfo> OnSimpleHealthHit;

	public event Action<DamageInfo> OnDeadEvent;

	public static event Action<HealthSimpleBase, DamageInfo> OnSimpleHealthDead;

	private void Awake()
	{
		healthValue = maxHealthValue;
		dmgReceiver.OnHurtEvent.AddListener(OnHurt);
	}

	private void OnHurt(DamageInfo dmgInfo)
	{
		if (!onlyReceiveExplosion || dmgInfo.isExplosion)
		{
			float num = 1f;
			bool flag = UnityEngine.Random.Range(0f, 1f) <= dmgInfo.critRate;
			dmgInfo.crit = (flag ? 1 : 0);
			if (!dmgInfo.fromCharacter || !dmgInfo.fromCharacter.IsMainCharacter)
			{
				num = damageMultiplierIfNotMainCharacter;
			}
			healthValue -= (flag ? dmgInfo.critDamageFactor : 1f) * dmgInfo.damageValue * num;
			this.OnHurtEvent?.Invoke(dmgInfo);
			HealthSimpleBase.OnSimpleHealthHit?.Invoke(this, dmgInfo);
			if (healthValue <= 0f)
			{
				Dead(dmgInfo);
			}
		}
	}

	private void Dead(DamageInfo dmgInfo)
	{
		dmgReceiver.OnDead(dmgInfo);
		this.OnDeadEvent?.Invoke(dmgInfo);
		HealthSimpleBase.OnSimpleHealthDead?.Invoke(this, dmgInfo);
	}
}
