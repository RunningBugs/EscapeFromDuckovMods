using Duckov.Utilities;
using UnityEngine;

public class ItemAgent_MeleeWeapon : DuckovItemAgent
{
	public GameObject hitFx;

	public GameObject slashFx;

	public float slashFxDelayTime = 0.05f;

	[SerializeField]
	private string soundKey = "Default";

	private Collider[] colliders;

	private ItemSetting_MeleeWeapon setting;

	private static int DamageHash = "Damage".GetHashCode();

	private static int CritRateHash = "CritRate".GetHashCode();

	private static int CritDamageFactorHash = "CritDamageFactor".GetHashCode();

	private static int ArmorPiercingHash = "ArmorPiercing".GetHashCode();

	private static int AttackSpeedHash = "AttackSpeed".GetHashCode();

	private static int AttackRangeHash = "AttackRange".GetHashCode();

	private static int DealDamageTimeHash = "DealDamageTime".GetHashCode();

	private static int StaminaCostHash = "StaminaCost".GetHashCode();

	private static int BleedChanceHash = "BleedChance".GetHashCode();

	private static int MoveSpeedMultiplierHash = "MoveSpeedMultiplier".GetHashCode();

	public float Damage => base.Item.GetStatValue(DamageHash);

	public float CritRate => base.Item.GetStatValue(CritRateHash);

	public float CritDamageFactor => base.Item.GetStatValue(CritDamageFactorHash);

	public float ArmorPiercing => base.Item.GetStatValue(ArmorPiercingHash);

	public float AttackSpeed => Mathf.Max(0.1f, base.Item.GetStatValue(AttackSpeedHash));

	public float AttackRange => base.Item.GetStatValue(AttackRangeHash);

	public float DealDamageTime => base.Item.GetStatValue(DealDamageTimeHash);

	public float StaminaCost => base.Item.GetStatValue(StaminaCostHash);

	public float BleedChance => base.Item.GetStatValue(BleedChanceHash);

	public float MoveSpeedMultiplier => base.Item.GetStatValue(MoveSpeedMultiplierHash);

	public float CharacterDamageMultiplier
	{
		get
		{
			if (!base.Holder)
			{
				return 1f;
			}
			return base.Holder.MeleeDamageMultiplier;
		}
	}

	public float CharacterCritRateGain
	{
		get
		{
			if (!base.Holder)
			{
				return 0f;
			}
			return base.Holder.MeleeCritRateGain;
		}
	}

	public float CharacterCritDamageGain
	{
		get
		{
			if (!base.Holder)
			{
				return 0f;
			}
			return base.Holder.MeleeCritDamageGain;
		}
	}

	public string SoundKey
	{
		get
		{
			if (string.IsNullOrWhiteSpace(soundKey))
			{
				return "Default";
			}
			return soundKey;
		}
	}

	private int UpdateColliders()
	{
		if (colliders == null)
		{
			colliders = new Collider[6];
		}
		return Physics.OverlapSphereNonAlloc(base.Holder.transform.position, AttackRange, colliders, GameplayDataSettings.Layers.damageReceiverLayerMask);
	}

	public void CheckAndDealDamage()
	{
		CheckCollidersInRange(dealDamage: true);
	}

	public bool AttackableTargetInRange()
	{
		return CheckCollidersInRange(dealDamage: false) > 0;
	}

	private int CheckCollidersInRange(bool dealDamage)
	{
		if (colliders == null)
		{
			colliders = new Collider[6];
		}
		int num = UpdateColliders();
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			Collider collider = colliders[i];
			DamageReceiver component = collider.GetComponent<DamageReceiver>();
			if (component == null || !Team.IsEnemy(component.Team, base.Holder.Team))
			{
				continue;
			}
			Health health = component.health;
			if ((bool)health)
			{
				CharacterMainControl characterMainControl = health.TryGetCharacter();
				if (characterMainControl == base.Holder || ((bool)characterMainControl && characterMainControl.Dashing))
				{
					continue;
				}
			}
			Vector3 vector = collider.transform.position - base.Holder.transform.position;
			vector.y = 0f;
			vector.Normalize();
			if (!(Vector3.Angle(vector, base.Holder.CurrentAimDirection) < 90f))
			{
				continue;
			}
			num2++;
			if (dealDamage)
			{
				DamageInfo damageInfo = new DamageInfo(base.Holder);
				damageInfo.damageValue = Damage * CharacterDamageMultiplier;
				damageInfo.armorPiercing = ArmorPiercing;
				damageInfo.critDamageFactor = CritDamageFactor * (1f + CharacterCritDamageGain);
				damageInfo.critRate = CritRate * (1f + CharacterCritRateGain);
				damageInfo.crit = -1;
				damageInfo.damageNormal = -base.Holder.modelRoot.right;
				damageInfo.damagePoint = collider.transform.position - vector * 0.2f;
				damageInfo.damagePoint.y = base.transform.position.y;
				damageInfo.fromWeaponItemID = base.Item.TypeID;
				damageInfo.bleedChance = BleedChance;
				if ((bool)setting)
				{
					damageInfo.isExplosion = setting.dealExplosionDamage;
				}
				component.Hurt(damageInfo);
				component.AddBuff(GameplayDataSettings.Buffs.Pain, base.Holder);
				if ((bool)hitFx)
				{
					Object.Instantiate(hitFx, damageInfo.damagePoint, Quaternion.LookRotation(damageInfo.damageNormal, Vector3.up));
				}
				if ((bool)base.Holder && base.Holder == CharacterMainControl.Main)
				{
					Vector3 right = base.Holder.modelRoot.right;
					right += Random.insideUnitSphere * 0.3f;
					right.Normalize();
					CameraShaker.Shake(right * 0.05f, CameraShaker.CameraShakeTypes.meleeAttackHit);
				}
			}
		}
		return num2;
	}

	private void Update()
	{
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		setting = base.Item.GetComponent<ItemSetting_MeleeWeapon>();
	}
}
