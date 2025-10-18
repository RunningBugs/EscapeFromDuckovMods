using ItemStatsSystem;
using UnityEngine;

[RequireComponent(typeof(Zone))]
public class ZoneDamage : MonoBehaviour
{
	public Zone zone;

	public float timeSpace = 0.5f;

	private float timer;

	public DamageInfo damageInfo;

	public bool checkGasMask;

	public bool checkElecProtection;

	public bool checkFireProtection;

	private int hasMaskHash = "GasMask".GetHashCode();

	private int elecProtectionHash = "ElecProtection".GetHashCode();

	private int fireProtectionHash = "FireProtection".GetHashCode();

	private void Start()
	{
		if (zone == null)
		{
			zone = GetComponent<Zone>();
		}
	}

	private void Update()
	{
		if (LevelManager.LevelInited)
		{
			timer += Time.deltaTime;
			if (timer > timeSpace)
			{
				timer %= timeSpace;
				Damage();
			}
		}
	}

	private void Damage()
	{
		foreach (Health health in zone.Healths)
		{
			CharacterMainControl characterMainControl = health.TryGetCharacter();
			if (characterMainControl == null)
			{
				continue;
			}
			if (checkGasMask && characterMainControl.HasGasMask)
			{
				Item faceMaskItem = characterMainControl.GetFaceMaskItem();
				if ((bool)faceMaskItem && faceMaskItem.GetStat(hasMaskHash) != null)
				{
					faceMaskItem.Durability -= 0.1f * timeSpace;
				}
			}
			else if ((!checkElecProtection || !(characterMainControl.CharacterItem.GetStat(elecProtectionHash).Value > 0.99f)) && (!checkFireProtection || !(characterMainControl.CharacterItem.GetStat(fireProtectionHash).Value > 0.99f)))
			{
				damageInfo.fromCharacter = null;
				damageInfo.damagePoint = health.transform.position + Vector3.up * 0.5f;
				damageInfo.damageNormal = Vector3.up;
				health.Hurt(damageInfo);
			}
		}
	}
}
