using System.Collections.Generic;
using Duckov.Utilities;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
	private LayerMask damageReceiverLayers;

	private LayerMask obsticleLayers;

	private List<Health> damagedHealth;

	private Collider[] colliders;

	public GameObject normalFxPfb;

	public GameObject flashFxPfb;

	private RaycastHit[] ObsHits;

	private void Awake()
	{
		ObsHits = new RaycastHit[3];
	}

	public void CreateExplosion(Vector3 center, float radius, DamageInfo dmgInfo, ExplosionFxTypes fxType = ExplosionFxTypes.normal, float shakeStrength = 1f, bool canHurtSelf = true)
	{
		Vector3.Distance(center, CharacterMainControl.Main.transform.position);
		if (Vector3.Distance(center, CharacterMainControl.Main.transform.position) < 30f)
		{
			CameraShaker.Shake((center - LevelManager.Instance.MainCharacter.transform.position).normalized * 0.4f * shakeStrength, CameraShaker.CameraShakeTypes.explosion);
		}
		dmgInfo.isExplosion = true;
		if (damagedHealth == null)
		{
			damagedHealth = new List<Health>();
			colliders = new Collider[8];
			damageReceiverLayers = GameplayDataSettings.Layers.damageReceiverLayerMask;
		}
		damagedHealth.Clear();
		Teams selfTeam = Teams.all;
		if ((bool)dmgInfo.fromCharacter && !canHurtSelf)
		{
			selfTeam = dmgInfo.fromCharacter.Team;
		}
		int num = Physics.OverlapSphereNonAlloc(center, radius, colliders, damageReceiverLayers);
		for (int i = 0; i < num; i++)
		{
			DamageReceiver component = colliders[i].gameObject.GetComponent<DamageReceiver>();
			if (!(component != null) || !Team.IsEnemy(selfTeam, component.Team) || (component.health != null && CheckObsticle(center + Vector3.up * 0.2f, colliders[i].gameObject.transform.position + Vector3.up * 0.6f)))
			{
				continue;
			}
			bool flag = false;
			bool flag2 = false;
			if (component.health != null)
			{
				if (damagedHealth.Contains(component.health))
				{
					flag = true;
				}
				else
				{
					damagedHealth.Add(component.health);
				}
				CharacterMainControl characterMainControl = component.health.TryGetCharacter();
				if ((bool)characterMainControl && characterMainControl.Dashing)
				{
					flag2 = true;
				}
			}
			if (!flag && !flag2)
			{
				dmgInfo.toDamageReceiver = component;
				dmgInfo.damagePoint = component.transform.position + Vector3.up * 0.6f;
				dmgInfo.damageNormal = (dmgInfo.damagePoint - center).normalized;
				component.Hurt(dmgInfo);
			}
		}
		switch (fxType)
		{
		case ExplosionFxTypes.normal:
			Object.Instantiate(normalFxPfb, center, Quaternion.identity);
			break;
		case ExplosionFxTypes.flash:
			Object.Instantiate(flashFxPfb, center, Quaternion.identity);
			break;
		case ExplosionFxTypes.fire:
		case ExplosionFxTypes.ice:
			break;
		}
	}

	private bool CheckObsticle(Vector3 startPoint, Vector3 endPoint)
	{
		obsticleLayers = (int)GameplayDataSettings.Layers.wallLayerMask | (int)GameplayDataSettings.Layers.groundLayerMask;
		startPoint.y = 0.5f;
		endPoint.y = 0.5f;
		return Physics.RaycastNonAlloc(new Ray(startPoint, (endPoint - startPoint).normalized), ObsHits, (endPoint - startPoint).magnitude, obsticleLayers) > 0;
	}
}
