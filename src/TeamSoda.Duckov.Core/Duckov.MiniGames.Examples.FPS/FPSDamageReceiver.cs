using System;
using UnityEngine;

namespace Duckov.MiniGames.Examples.FPS;

public class FPSDamageReceiver : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem damageEffectPrefab;

	[SerializeField]
	private ParticleSystem damageEffectPrefab_Censored;

	public ParticleSystem DamageFX
	{
		get
		{
			if (GameManager.BloodFxOn)
			{
				return damageEffectPrefab;
			}
			return damageEffectPrefab_Censored;
		}
	}

	public event Action<FPSDamageReceiver, FPSDamageInfo> onReceiveDamage;

	internal void CastDamage(FPSDamageInfo damage)
	{
		if (!(DamageFX == null))
		{
			FXPool.Play(DamageFX, damage.point, Quaternion.FromToRotation(Vector3.forward, damage.normal));
			this.onReceiveDamage?.Invoke(this, damage);
		}
	}
}
