using System;
using Duckov;
using Duckov.Utilities;
using FMOD.Studio;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

public class ItemAgent_Gun : DuckovItemAgent
{
	public enum GunStates
	{
		shootCooling,
		ready,
		fire,
		burstEachShotCooling,
		empty,
		reloading
	}

	private Item _bulletItem;

	private static int ShootSpeedHash = "ShootSpeed".GetHashCode();

	private static int ReloadTimeHash = "ReloadTime".GetHashCode();

	private static int CapacityHash = "Capacity".GetHashCode();

	private static int DurabilityHash = "Durability".GetHashCode();

	private float maxDurability;

	private static int DamageHash = "Damage".GetHashCode();

	private static int BurstCountHash = "BurstCount".GetHashCode();

	private static int BulletSpeedHash = "BulletSpeed".GetHashCode();

	private static int BulletDistanceHash = "BulletDistance".GetHashCode();

	private static int PenetrateHash = "Penetrate".GetHashCode();

	private static int explosionDamageMultiplierHash = "ExplosionDamageMultiplier".GetHashCode();

	private static int CritRateHash = "CritRate".GetHashCode();

	private static int CritDamageFactorHash = "CritDamageFactor".GetHashCode();

	private static int SoundRangeHash = "SoundRange".GetHashCode();

	private static int ArmorPiercingHash = "ArmorPiercing".GetHashCode();

	private static int ArmorBreakHash = "ArmorBreak".GetHashCode();

	private static int ShotCountHash = "ShotCount".GetHashCode();

	private static int ShotAngleHash = "ShotAngle".GetHashCode();

	private static int ADSAimDistanceFactorHash = "ADSAimDistanceFactor".GetHashCode();

	private static int AdsTimeHash = "ADSTime".GetHashCode();

	private float scatterFactorHips = 1f;

	private float scatterFactorAds = 1f;

	private static int ScatterFactorHash = "ScatterFactor".GetHashCode();

	private static int ScatterFactorHashADS = "ScatterFactorADS".GetHashCode();

	private static int DefaultScatterHash = "DefaultScatter".GetHashCode();

	private static int DefaultScatterHashADS = "DefaultScatterADS".GetHashCode();

	private static int MaxScatterHash = "MaxScatter".GetHashCode();

	private static int MaxScatterHashADS = "MaxScatterADS".GetHashCode();

	private static int ScatterGrowHash = "ScatterGrow".GetHashCode();

	private static int ScatterGrowHashADS = "ScatterGrowADS".GetHashCode();

	private static int ScatterRecoverHash = "ScatterRecover".GetHashCode();

	private static int ScatterRecoverHashADS = "ScatterRecoverADS".GetHashCode();

	private static int RecoilVMinHash = "RecoilVMin".GetHashCode();

	private static int RecoilVMaxHash = "RecoilVMax".GetHashCode();

	private static int RecoilHMinHash = "RecoilHMin".GetHashCode();

	private static int RecoilHMaxHash = "RecoilHMax".GetHashCode();

	private static int RecoilScaleVHash = "RecoilScaleV".GetHashCode();

	private static int RecoilScaleHHash = "RecoilScaleH".GetHashCode();

	private static int RecoilRecoverHash = "RecoilRecover".GetHashCode();

	private static int RecoilTimeHash = "RecoilTime".GetHashCode();

	private static int RecoilRecoverTimeHash = "RecoilRecoverTime".GetHashCode();

	private static int MoveSpeedMultiplierHash = "MoveSpeedMultiplier".GetHashCode();

	private static int AdsWalkSpeedMultiplierHash = "AdsWalkSpeedMultiplier".GetHashCode();

	private static int BuffChanceHash = "BuffChance".GetHashCode();

	private static int bulletCritRateGainHash = "CritRateGain".GetHashCode();

	private static int bulletCritDamageFactorGainHash = "CritDamageFactorGain".GetHashCode();

	private static int bulletArmorPiercingGainHash = "ArmorPiercingGain".GetHashCode();

	private static int BulletDamageMultiplierHash = "damageMultiplier".GetHashCode();

	private static int bulletExplosionRangeHash = "ExplosionRange".GetHashCode();

	private static int BulletBuffChanceMultiplierHash = "buffChanceMultiplier".GetHashCode();

	private static int BulletBleedChanceHash = "bleedChance".GetHashCode();

	private static int bulletExplosionDamageHash = "ExplosionDamage".GetHashCode();

	private static int armorBreakGainHash = "ArmorBreakGain".GetHashCode();

	private static int bulletDurabilityCostHash = "DurabilityCost".GetHashCode();

	private int muzzleIndex;

	public GameObject loadedVisualObject;

	private float adsValue;

	private Transform _mz1;

	private Transform _mz2;

	private bool hasMz2 = true;

	[SerializeField]
	private ParticleSystem shellParticle;

	private ItemSetting_Gun _gunItemSetting;

	private bool triggerInput;

	private bool triggerThisFrame;

	private bool releaseThisFrame;

	private bool triggerBuffer;

	private float scatterBeforeControl;

	private EventInstance? _shootSoundEvent;

	private EventInstance? _reloadSoundLoopEvent;

	private float stateTimer;

	private int burstCounter;

	private Projectile projInst;

	private GunStates gunState = GunStates.ready;

	private bool needAutoReload;

	private bool loadBulletsStarted;

	private float _recoilMoveValue;

	private float _recoilDistance = 0.2f;

	private float _recoilBackSpeed = 20f;

	private float _recoilRecoverSpeed = 8f;

	private bool _recoilBack;

	public Item BulletItem
	{
		get
		{
			if (_bulletItem == null || _bulletItem.ParentItem != base.Item)
			{
				foreach (Item item in base.Item.Inventory)
				{
					if (item != null)
					{
						_bulletItem = item;
						break;
					}
				}
			}
			return _bulletItem;
		}
	}

	public float ShootSpeed => base.Item.GetStatValue(ShootSpeedHash);

	public float ReloadTime => base.Item.GetStatValue(ReloadTimeHash) / (1f + CharacterReloadSpeedGain);

	public int Capacity => Mathf.RoundToInt(base.Item.GetStatValue(CapacityHash));

	public float durabilityPercent => Durability / MaxDurability;

	public float Durability => base.Item.Variables.GetFloat(DurabilityHash);

	public float MaxDurability
	{
		get
		{
			if (maxDurability <= 0f)
			{
				maxDurability = base.Item.Constants.GetFloat("MaxDurability", 50f);
			}
			return maxDurability;
		}
	}

	public float Damage => base.Item.GetStatValue(DamageHash);

	public int BurstCount => Mathf.Max(1, Mathf.RoundToInt(base.Item.GetStatValue(BurstCountHash)));

	public float BulletSpeed => base.Item.GetStatValue(BulletSpeedHash);

	public float BulletDistance => base.Item.GetStatValue(BulletDistanceHash) * (base.Holder ? base.Holder.GunDistanceMultiplier : 1f);

	public int Penetrate => Mathf.RoundToInt(base.Item.GetStatValue(PenetrateHash));

	public float ExplosionDamageMultiplier => base.Item.GetStatValue(explosionDamageMultiplierHash);

	public float CritRate => base.Item.GetStatValue(CritRateHash);

	public float CritDamageFactor => base.Item.GetStatValue(CritDamageFactorHash);

	public float SoundRange => base.Item.GetStatValue(SoundRangeHash);

	public bool Silenced
	{
		get
		{
			Stat stat = base.Item.GetStat(SoundRangeHash);
			return stat.Value < stat.BaseValue * 0.95f;
		}
	}

	public float ArmorPiercing => base.Item.GetStatValue(ArmorPiercingHash);

	public float ArmorBreak => base.Item.GetStatValue(ArmorBreakHash);

	public int ShotCount => Mathf.RoundToInt(base.Item.GetStatValue(ShotCountHash));

	public float ShotAngle => base.Item.GetStatValue(ShotAngleHash) * (IsInAds ? 0.5f : 1f);

	public float ADSAimDistanceFactor => base.Item.GetStatValue(ADSAimDistanceFactorHash);

	public float AdsSpeed => 1f / base.Item.GetStatValue(AdsTimeHash);

	public float DefaultScatter
	{
		get
		{
			float a = base.Item.GetStatValue(DefaultScatterHash) * scatterFactorHips;
			float b = base.Item.GetStatValue(DefaultScatterHashADS) * scatterFactorAds;
			return Mathf.Lerp(a, b, adsValue);
		}
	}

	public float MaxScatter
	{
		get
		{
			float a = base.Item.GetStatValue(MaxScatterHash) * scatterFactorHips;
			float b = base.Item.GetStatValue(MaxScatterHashADS) * scatterFactorAds;
			return Mathf.Lerp(a, b, adsValue);
		}
	}

	public float ScatterGrow
	{
		get
		{
			float a = base.Item.GetStatValue(ScatterGrowHash) * scatterFactorHips;
			float b = base.Item.GetStatValue(ScatterGrowHashADS) * scatterFactorAds;
			return Mathf.Lerp(a, b, adsValue);
		}
	}

	public float ScatterRecover
	{
		get
		{
			float statValue = base.Item.GetStatValue(ScatterRecoverHash);
			float statValue2 = base.Item.GetStatValue(ScatterRecoverHashADS);
			return Mathf.Lerp(statValue, statValue2, adsValue) * ScatterGrow * ShootSpeed;
		}
	}

	public float RecoilVMin => base.Item.GetStatValue(RecoilVMinHash);

	public float RecoilVMax => base.Item.GetStatValue(RecoilVMaxHash);

	public float RecoilHMin => base.Item.GetStatValue(RecoilHMinHash);

	public float RecoilHMax => base.Item.GetStatValue(RecoilHMaxHash);

	public float RecoilScaleV => base.Item.GetStatValue(RecoilScaleVHash);

	public float RecoilScaleH => base.Item.GetStatValue(RecoilScaleHHash);

	public float RecoilRecover => base.Item.GetStatValue(RecoilRecoverHash);

	public float RecoilTime => base.Item.GetStatValue(RecoilTimeHash);

	public float RecoilRecoverTime => base.Item.GetStatValue(RecoilRecoverTimeHash);

	public float MoveSpeedMultiplier => base.Item.GetStatValue(MoveSpeedMultiplierHash);

	public float AdsWalkSpeedMultiplier => Mathf.Min(1f, base.Item.GetStatValue(AdsWalkSpeedMultiplierHash));

	public float burstCoolTime => 1f / ShootSpeed * ((float)(3 * BurstCount) / ((float)BurstCount + 2f));

	public float burstShotTimeSpace => 1f / ShootSpeed * ((float)BurstCount / ((float)BurstCount + 2f));

	public float BuffChance => base.Item.GetStatValue(BuffChanceHash);

	public float bulletCritRateGain
	{
		get
		{
			if ((bool)BulletItem)
			{
				return BulletItem.Constants.GetFloat(bulletCritRateGainHash);
			}
			return 0f;
		}
	}

	public float BulletCritDamageFactorGain
	{
		get
		{
			if ((bool)BulletItem)
			{
				return BulletItem.Constants.GetFloat(bulletCritDamageFactorGainHash);
			}
			return 0f;
		}
	}

	public float BulletArmorPiercingGain
	{
		get
		{
			if ((bool)BulletItem)
			{
				return BulletItem.Constants.GetFloat(bulletArmorPiercingGainHash);
			}
			return 0f;
		}
	}

	public float BulletDamageMultiplier
	{
		get
		{
			if ((bool)BulletItem)
			{
				return BulletItem.Constants.GetFloat(BulletDamageMultiplierHash);
			}
			return 0f;
		}
	}

	public float BulletExplosionRange
	{
		get
		{
			if ((bool)BulletItem)
			{
				return BulletItem.Constants.GetFloat(bulletExplosionRangeHash);
			}
			return 0f;
		}
	}

	public float BulletBuffChanceMultiplier
	{
		get
		{
			if ((bool)BulletItem)
			{
				return BulletItem.Constants.GetFloat(BulletBuffChanceMultiplierHash);
			}
			return 0f;
		}
	}

	public float BulletBleedChance
	{
		get
		{
			if ((bool)BulletItem)
			{
				return BulletItem.Constants.GetFloat(BulletBleedChanceHash);
			}
			return 0f;
		}
	}

	public float BulletExplosionDamage
	{
		get
		{
			if ((bool)BulletItem)
			{
				return BulletItem.Constants.GetFloat(bulletExplosionDamageHash);
			}
			return 0f;
		}
	}

	public float BulletArmorBreakGain
	{
		get
		{
			if ((bool)BulletItem)
			{
				return BulletItem.Constants.GetFloat(armorBreakGainHash);
			}
			return 0f;
		}
	}

	public float bulletDurabilityCost
	{
		get
		{
			if ((bool)BulletItem)
			{
				return BulletItem.Constants.GetFloat(bulletDurabilityCostHash);
			}
			return 0f;
		}
	}

	public float CharacterDamageMultiplier
	{
		get
		{
			if (!base.Holder)
			{
				return 0f;
			}
			return base.Holder.GunDamageMultiplier;
		}
	}

	public float CharacterReloadSpeedGain
	{
		get
		{
			if (!base.Holder)
			{
				return 0f;
			}
			return base.Holder.ReloadSpeedGain;
		}
	}

	public float CharacterGunCritRateGain
	{
		get
		{
			if (!base.Holder)
			{
				return 0f;
			}
			return base.Holder.GunCritRateGain;
		}
	}

	public float CharacterGunCritDamageGain
	{
		get
		{
			if (!base.Holder)
			{
				return 0f;
			}
			return base.Holder.GunCritDamageGain;
		}
	}

	public float CharacterRecoilControl
	{
		get
		{
			if (!base.Holder)
			{
				return 1f;
			}
			return base.Holder.RecoilControl;
		}
	}

	public float CharacterScatterMultiplier
	{
		get
		{
			if (!base.Holder)
			{
				return 1f;
			}
			return Mathf.Max(0.1f, base.Holder.GunScatterMultiplier);
		}
	}

	public int BulletCount
	{
		get
		{
			if (!GunItemSetting)
			{
				return 0;
			}
			return GunItemSetting.BulletCount;
		}
	}

	public float AdsValue => adsValue;

	public Transform muzzle
	{
		get
		{
			if (muzzleIndex != 0 && muzzle2 != null)
			{
				return muzzle2;
			}
			return muzzle1;
		}
	}

	private Transform muzzle1
	{
		get
		{
			if (_mz1 == null)
			{
				_mz1 = GetSocket("Muzzle", createNew: true);
			}
			return _mz1;
		}
	}

	private Transform muzzle2
	{
		get
		{
			if (_mz2 == null && hasMz2)
			{
				_mz2 = GetSocket("Muzzle2", createNew: false);
				if (_mz2 == null)
				{
					hasMz2 = false;
				}
			}
			return _mz2;
		}
	}

	private GameObject muzzleFxPfb => GunItemSetting.muzzleFxPfb;

	public ItemSetting_Gun GunItemSetting
	{
		get
		{
			if (!_gunItemSetting && (bool)base.Item)
			{
				_gunItemSetting = base.Item.GetComponent<ItemSetting_Gun>();
			}
			return _gunItemSetting;
		}
	}

	public bool IsInAds
	{
		get
		{
			if (!base.Holder)
			{
				return false;
			}
			return base.Holder.IsInAdsInput;
		}
	}

	public float CurrentScatter => scatterBeforeControl * CharacterScatterMultiplier;

	public float MinScatter => DefaultScatter;

	public float StateTimer => stateTimer;

	public GunStates GunState => gunState;

	public static event Action<ItemAgent_Gun> OnMainCharacterShootEvent;

	public event Action OnShootEvent;

	public event Action OnLoadedEvent;

	private void Update()
	{
		UpdateGun();
		UpdateScatterFactor();
		triggerInput = false;
		triggerThisFrame = false;
		releaseThisFrame = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		StopReloadSound();
	}

	private void UpdateScatterFactor()
	{
		scatterFactorHips = base.Item.GetStatValue(ScatterFactorHash);
		scatterFactorAds = base.Item.GetStatValue(ScatterFactorHashADS);
	}

	private void UpdateGun()
	{
		float maxScatter = MaxScatter;
		if (scatterBeforeControl > maxScatter)
		{
			scatterBeforeControl = maxScatter;
		}
		scatterBeforeControl = Mathf.MoveTowards(scatterBeforeControl, DefaultScatter, ScatterRecover * Time.deltaTime * ((scatterBeforeControl < DefaultScatter) ? 6f : 1f));
		UpdateStates();
		UpdateAds();
		UpdateVisualRecoil();
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		if ((bool)GunItemSetting)
		{
			if ((bool)base.Holder && (bool)base.Holder.CharacterItem && (bool)base.Holder.CharacterItem.Inventory)
			{
				GunItemSetting.AutoSetTypeInInventory(base.Holder.CharacterItem.Inventory);
			}
			else
			{
				GunItemSetting.AutoSetTypeInInventory(null);
			}
			scatterBeforeControl = DefaultScatter;
			if (loadedVisualObject != null)
			{
				loadedVisualObject.SetActive(GunItemSetting.BulletCount > 0);
			}
		}
	}

	public void UpdateStates()
	{
		if (GunItemSetting.LoadingBullets)
		{
			return;
		}
		if (triggerThisFrame && ShootSpeed >= 5f)
		{
			triggerBuffer = true;
			triggerThisFrame = false;
		}
		switch (gunState)
		{
		case GunStates.shootCooling:
			stateTimer += Time.deltaTime;
			if (stateTimer >= burstCoolTime)
			{
				TransToReady();
			}
			break;
		case GunStates.ready:
		{
			bool flag = false;
			if (BulletCount <= 0)
			{
				TransToEmpty();
			}
			else if (GunItemSetting.triggerMode == ItemSetting_Gun.TriggerModes.auto)
			{
				if (triggerInput)
				{
					flag = true;
				}
			}
			else if ((GunItemSetting.triggerMode == ItemSetting_Gun.TriggerModes.semi || GunItemSetting.triggerMode == ItemSetting_Gun.TriggerModes.bolt) && (triggerBuffer || triggerThisFrame))
			{
				triggerThisFrame = false;
				triggerBuffer = false;
				flag = true;
			}
			if (flag)
			{
				TransToFire(triggerThisFrame);
			}
			else if (needAutoReload)
			{
				needAutoReload = false;
				CharacterReload();
			}
			break;
		}
		case GunStates.fire:
			triggerBuffer = false;
			if (BulletCount <= 0)
			{
				muzzleIndex = ((muzzleIndex == 0) ? 1 : 0);
				TransToEmpty();
			}
			else if (burstCounter >= BurstCount)
			{
				muzzleIndex = ((muzzleIndex == 0) ? 1 : 0);
				TransToBurstCooling();
			}
			else
			{
				TransToBurstEachShotCooling();
			}
			break;
		case GunStates.burstEachShotCooling:
			stateTimer += Time.deltaTime;
			if (stateTimer >= burstShotTimeSpace)
			{
				TransToFire(isFirstShot: false);
			}
			break;
		case GunStates.empty:
			if (needAutoReload)
			{
				needAutoReload = false;
				CharacterReload();
			}
			else if ((triggerThisFrame || triggerBuffer) && base.Holder != null)
			{
				triggerThisFrame = false;
				triggerBuffer = false;
				base.Holder.TryToReload();
			}
			break;
		case GunStates.reloading:
			triggerBuffer = false;
			stateTimer += Time.deltaTime;
			if (stateTimer < ReloadTime)
			{
				loadBulletsStarted = false;
			}
			else if (!loadBulletsStarted)
			{
				loadBulletsStarted = true;
				StartLoadBullets();
			}
			else if (!GunItemSetting.LoadingBullets)
			{
				if (GunItemSetting.LoadBulletsSuccess)
				{
					PostReloadSuccessSound();
				}
				needAutoReload = GunItemSetting.reloadMode == ItemSetting_Gun.ReloadModes.singleBullet && !GunItemSetting.IsFull();
				loadBulletsStarted = false;
				if (GunItemSetting.BulletCount > 0 && loadedVisualObject != null)
				{
					loadedVisualObject.SetActive(value: true);
				}
				this.OnLoadedEvent?.Invoke();
				TransToReady();
			}
			break;
		}
	}

	private void UpdateAds()
	{
		float num = 0f;
		if ((bool)base.Holder && base.Holder.IsInAdsInput)
		{
			num = 1f;
		}
		float num2 = AdsSpeed;
		if (num == 0f)
		{
			num2 = Mathf.Max(num2, 4f);
		}
		adsValue = Mathf.MoveTowards(adsValue, num, Time.deltaTime * num2);
	}

	private void TransToBurstCooling()
	{
		gunState = GunStates.shootCooling;
		burstCounter = 0;
		stateTimer = 0f;
	}

	private void TransToReady()
	{
		gunState = GunStates.ready;
		burstCounter = 0;
	}

	private void TransToFire(bool isFirstShot)
	{
		if (BulletCount <= 0 || base.Item.Durability <= 0f)
		{
			return;
		}
		gunState = GunStates.fire;
		Vector3 vector = muzzle.forward;
		if ((bool)base.Holder && base.Holder.CharacterMoveability > 0.5f)
		{
			Vector3 currentAimPoint = base.Holder.GetCurrentAimPoint();
			currentAimPoint.y = 0f;
			Vector3 position = base.Holder.transform.position;
			position.y = 0f;
			Vector3 position2 = muzzle.position;
			position2.y = 0f;
			if (Vector3.Distance(position, currentAimPoint) > Vector3.Distance(position, position2) + 0.1f)
			{
				vector = base.Holder.GetCurrentAimPoint() - muzzle.position;
				vector.Normalize();
			}
		}
		for (int i = 0; i < ShotCount; i++)
		{
			Vector3 vector2 = vector;
			float num = ShotAngle;
			bool flag = num > 359f;
			if (flag)
			{
				num -= num / (float)ShotCount;
			}
			float num2 = (0f - num) * 0.5f;
			float num3 = num / ((float)ShotCount - 1f);
			if ((float)ShotCount % 2f < 0.01f && flag)
			{
				num2 -= num3 * 0.5f;
			}
			if (ShotCount > 1)
			{
				vector2 = Quaternion.Euler(0f, num2 + (float)i * num3, 0f) * vector;
			}
			Vector3 localPosition = muzzle.localPosition;
			localPosition.y = 0f;
			float magnitude = localPosition.magnitude;
			ShootOneBullet(muzzle.position, vector2, muzzle.position - magnitude * vector2);
			if (base.Holder != null)
			{
				AIMainBrain.MakeSound(new AISound
				{
					fromCharacter = base.Holder,
					fromObject = base.gameObject,
					pos = muzzle.position,
					soundType = SoundTypes.combatSound,
					fromTeam = base.Holder.Team,
					radius = SoundRange
				});
			}
		}
		PostShootSound();
		scatterBeforeControl = Mathf.Clamp(scatterBeforeControl + ScatterGrow, DefaultScatter, MaxScatter);
		AimRecoil(vector);
		if (base.Holder == LevelManager.Instance.MainCharacter)
		{
			LevelManager.Instance.InputManager.AddRecoil(this);
		}
		StartVisualRecoil();
		GunItemSetting.UseABullet();
		base.Holder.TriggerShootEvent(this);
		this.OnShootEvent?.Invoke();
		if (BulletCount <= 0 && GunItemSetting.autoReload)
		{
			needAutoReload = true;
		}
		if (GunItemSetting.BulletCount <= 0 && loadedVisualObject != null)
		{
			loadedVisualObject.SetActive(value: false);
		}
		if ((bool)base.Holder && base.Holder.IsMainCharacter && LevelManager.Instance.IsRaidMap)
		{
			base.Item.Durability = Mathf.Max(0f, base.Item.Durability - bulletDurabilityCost);
		}
		if ((bool)muzzleFxPfb)
		{
			UnityEngine.Object.Instantiate(muzzleFxPfb, muzzle.position, muzzle.rotation).transform.SetParent(muzzle);
		}
		if ((bool)shellParticle)
		{
			shellParticle.Emit(1);
		}
		burstCounter++;
		if ((bool)base.Holder && base.Holder.IsMainCharacter)
		{
			CameraShaker.Shake(-muzzle.forward * 0.07f, CameraShaker.CameraShakeTypes.recoil);
			ItemAgent_Gun.OnMainCharacterShootEvent?.Invoke(this);
		}
	}

	private void TransToBurstEachShotCooling()
	{
		gunState = GunStates.burstEachShotCooling;
		stateTimer = 0f;
	}

	private void TransToEmpty()
	{
		gunState = GunStates.empty;
	}

	private void ShootOneBullet(Vector3 _muzzlePoint, Vector3 _shootDirection, Vector3 firstFrameCheckStartPoint)
	{
		bool flag = false;
		if (GunItemSetting.LoadingBullets || !BulletItem)
		{
			return;
		}
		if ((bool)base.Holder && base.Holder.IsMainCharacter)
		{
			flag = true;
		}
		ItemSetting_Bullet component = BulletItem.GetComponent<ItemSetting_Bullet>();
		float num = 0f;
		if (flag)
		{
			num = Mathf.Max(1f, CurrentScatter) * Mathf.Lerp(1.5f, 0f, Mathf.InverseLerp(0f, 0.5f, durabilityPercent));
		}
		float y = UnityEngine.Random.Range(-0.5f, 0.5f) * (CurrentScatter + num);
		_shootDirection = Quaternion.Euler(0f, y, 0f) * _shootDirection;
		_shootDirection.Normalize();
		Projectile projectile = _gunItemSetting.bulletPfb;
		if (projectile == null)
		{
			projectile = GameplayDataSettings.Prefabs.DefaultBullet;
		}
		projInst = LevelManager.Instance.BulletPool.GetABullet(projectile);
		projInst.transform.position = _muzzlePoint;
		projInst.transform.rotation = Quaternion.LookRotation(_shootDirection, Vector3.up);
		ProjectileContext context = new ProjectileContext
		{
			firstFrameCheck = true,
			firstFrameCheckStartPoint = firstFrameCheckStartPoint,
			direction = _shootDirection.normalized,
			speed = BulletSpeed
		};
		if ((bool)base.Holder)
		{
			context.team = base.Holder.Team;
			context.speed *= base.Holder.GunBulletSpeedMultiplier;
		}
		context.distance = BulletDistance + 0.4f;
		context.halfDamageDistance = context.distance * 0.5f;
		if (!flag)
		{
			context.distance *= 1.05f;
		}
		context.penetrate = Penetrate;
		float characterDamageMultiplier = CharacterDamageMultiplier;
		float num2 = 1f;
		context.damage = Damage * BulletDamageMultiplier * num2 * characterDamageMultiplier / (float)ShotCount;
		if (Damage > 1f && context.damage < 1f)
		{
			context.damage = 1f;
		}
		context.critDamageFactor = (CritDamageFactor + BulletCritDamageFactorGain) * (1f + CharacterGunCritDamageGain);
		context.critRate = CritRate * (1f + CharacterGunCritRateGain + bulletCritRateGain);
		if (flag)
		{
			context.critRate = (LevelManager.Instance.InputManager.AimingEnemyHead ? 1f : 0f);
		}
		context.armorPiercing = ArmorPiercing + BulletArmorPiercingGain;
		context.armorBreak = ArmorBreak + BulletArmorBreakGain;
		context.fromCharacter = base.Holder;
		context.explosionRange = BulletExplosionRange;
		context.explosionDamage = BulletExplosionDamage * ExplosionDamageMultiplier;
		switch (_gunItemSetting.element)
		{
		case ElementTypes.physics:
			context.element_Physics = 1f;
			break;
		case ElementTypes.fire:
			context.element_Fire = 1f;
			break;
		case ElementTypes.poison:
			context.element_Poison = 1f;
			break;
		case ElementTypes.electricity:
			context.element_Electricity = 1f;
			break;
		case ElementTypes.space:
			context.element_Space = 1f;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		context.fromWeaponItemID = base.Item.TypeID;
		context.buff = _gunItemSetting.buff;
		if ((bool)component)
		{
			context.buffChance = BulletBuffChanceMultiplier * BuffChance;
		}
		context.bleedChance = BulletBleedChance;
		if ((bool)base.Holder)
		{
			if (flag)
			{
				if (base.Holder.HasNearByHalfObsticle())
				{
					context.ignoreHalfObsticle = true;
				}
			}
			else
			{
				projInst.damagedObjects.AddRange(base.Holder.GetNearByHalfObsticles());
			}
		}
		if (context.critRate > 0.99f)
		{
			context.ignoreHalfObsticle = true;
		}
		projInst.Init(context);
	}

	private void AimRecoil(Vector3 shootDir)
	{
		if ((bool)base.Holder && base.Holder == CharacterMainControl.Main)
		{
			Vector3 currentAimDirection = base.Holder.CurrentAimDirection;
			currentAimDirection.y = 0f;
			currentAimDirection = currentAimDirection.normalized * 0.2f;
		}
	}

	public bool CharacterReload(Item prefererdBullet = null)
	{
		if (!base.Holder)
		{
			return false;
		}
		return base.Holder.TryToReload(prefererdBullet);
	}

	public bool BeginReload()
	{
		if (gunState != GunStates.ready && gunState != GunStates.empty && gunState != GunStates.shootCooling)
		{
			return false;
		}
		burstCounter = 0;
		if (GunItemSetting.PreferdBulletsToLoad != null)
		{
			GunItemSetting.SetTargetBulletType(GunItemSetting.PreferdBulletsToLoad);
		}
		if (GunItemSetting.TargetBulletID == -1)
		{
			GunItemSetting.AutoSetTypeInInventory(base.Holder.CharacterItem.Inventory);
		}
		if (GunItemSetting.TargetBulletID == -1)
		{
			return false;
		}
		int num = -1;
		Item currentLoadedBullet = GunItemSetting.GetCurrentLoadedBullet();
		if (currentLoadedBullet != null)
		{
			num = currentLoadedBullet.TypeID;
		}
		if (BulletCount >= Capacity && num == GunItemSetting.TargetBulletID)
		{
			return false;
		}
		if (GunItemSetting.PreferdBulletsToLoad == null && GunItemSetting.GetBulletCountofTypeInInventory(GunItemSetting.TargetBulletID, base.Holder.CharacterItem.Inventory) <= 0)
		{
			if ((bool)base.Holder && GunItemSetting.BulletCount <= 0)
			{
				base.Holder.PopText("Poptext_OutOfAmmo".ToPlainText());
			}
			return false;
		}
		gunState = GunStates.reloading;
		stateTimer = 0f;
		PostStartReloadSound();
		return true;
	}

	private void PostStartReloadSound()
	{
		if (_reloadSoundLoopEvent.HasValue)
		{
			_reloadSoundLoopEvent.Value.stop(STOP_MODE.IMMEDIATE);
		}
		if (base.gameObject.activeInHierarchy)
		{
			string soundkey = GunItemSetting.reloadKey.ToLower() + "_start";
			string eventName = "SFX/Combat/Gun/Reload/{soundkey}".Format(new { soundkey });
			_reloadSoundLoopEvent = AudioManager.Post(eventName, base.gameObject);
		}
	}

	private void PostReloadSuccessSound()
	{
		if (_reloadSoundLoopEvent.HasValue)
		{
			_reloadSoundLoopEvent.Value.stop(STOP_MODE.IMMEDIATE);
		}
		if (base.gameObject.activeInHierarchy)
		{
			string soundkey = GunItemSetting.reloadKey.ToLower() + "_end";
			AudioManager.Post("SFX/Combat/Gun/Reload/{soundkey}".Format(new { soundkey }), base.gameObject);
		}
	}

	private void PostShootSound()
	{
		string text = GunItemSetting.shootKey.ToLower();
		if (Silenced)
		{
			text += "_mute";
		}
		string eventName = "SFX/Combat/Gun/Shoot/{soundkey}".Format(new
		{
			soundkey = text
		});
		_shootSoundEvent = AudioManager.Post(eventName, base.gameObject);
	}

	private void StopAllSound()
	{
		AudioManager.StopAll(base.gameObject);
	}

	private void StopReloadSound()
	{
		if (_reloadSoundLoopEvent.HasValue)
		{
			_reloadSoundLoopEvent.Value.stop(STOP_MODE.IMMEDIATE);
		}
	}

	public void CancleReload()
	{
		StopReloadSound();
		if (gunState == GunStates.reloading)
		{
			TransToBurstCooling();
		}
	}

	public bool IsFull()
	{
		return BulletCount >= Capacity;
	}

	public int GetBulletCountInInventory()
	{
		if (!GunItemSetting || !base.Holder || !base.Holder.CharacterItem)
		{
			return 0;
		}
		return GunItemSetting.GetBulletCountofTypeInInventory(GunItemSetting.TargetBulletID, base.Holder.CharacterItem.Inventory);
	}

	private void StartLoadBullets()
	{
		GunItemSetting.LoadBulletsFromInventory(base.Holder.CharacterItem.Inventory).Forget();
	}

	private void StartVisualRecoil()
	{
		_recoilBack = true;
	}

	private void UpdateVisualRecoil()
	{
		bool flag = false;
		if (_recoilBack)
		{
			flag = true;
			_recoilMoveValue = Mathf.MoveTowards(_recoilMoveValue, 1f, _recoilBackSpeed * Time.deltaTime);
			if (Mathf.Approximately(_recoilMoveValue, 1f))
			{
				_recoilBack = false;
			}
		}
		else if (_recoilMoveValue > 0f)
		{
			flag = true;
			_recoilMoveValue = Mathf.MoveTowards(_recoilMoveValue, 0f, _recoilRecoverSpeed * Time.deltaTime);
		}
		if (flag)
		{
			base.transform.localPosition = Vector3.back * _recoilMoveValue * _recoilDistance;
		}
	}

	public void SetTrigger(bool trigger, bool _triggerThisFrame, bool _releaseThisFrame)
	{
		triggerInput = trigger;
		triggerThisFrame = _triggerThisFrame;
		releaseThisFrame = _releaseThisFrame;
	}

	public bool IsReloading()
	{
		return gunState == GunStates.reloading;
	}

	public Progress GetReloadProgress()
	{
		Progress result = default(Progress);
		if (IsReloading())
		{
			result.inProgress = true;
			result.total = ReloadTime;
			result.current = stateTimer;
		}
		else
		{
			result.inProgress = false;
		}
		return result;
	}

	public ADSAimMarker GetAimMarkerPfb()
	{
		Slot slot = base.Item.Slots.GetSlot("Scope");
		if (slot != null && slot.Content != null)
		{
			ItemSetting_Accessory component = slot.Content.GetComponent<ItemSetting_Accessory>();
			if ((bool)component.overrideAdsAimMarker)
			{
				return component.overrideAdsAimMarker;
			}
		}
		return _gunItemSetting.adsAimMarker;
	}
}
