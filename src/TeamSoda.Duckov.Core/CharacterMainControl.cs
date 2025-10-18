using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.Buffs;
using Duckov.Scenes;
using Duckov.UI;
using Duckov.UI.DialogueBubbles;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using SodaCraft.Localizations;
using Unity.Mathematics;
using UnityEngine;

public class CharacterMainControl : MonoBehaviour
{
	public enum WeightStates
	{
		light,
		normal,
		heavy,
		superHeavy,
		overWeight
	}

	public CharacterRandomPreset characterPreset;

	private AudioManager.VoiceType audioVoiceType;

	private AudioManager.FootStepMaterialType footStepMaterialType;

	[SerializeField]
	private Teams team;

	private Item characterItem;

	public Movement movementControl;

	public ItemAgentHolder agentHolder;

	public CA_Carry carryAction;

	public CharacterModel characterModel;

	private bool hidden;

	public InteractableLootbox deadLootBoxPrefab;

	public Transform modelRoot;

	private Vector3 moveInput;

	private bool runInput;

	private bool adsInput;

	private const float defaultAimRange = 8f;

	private AimTypes aimType;

	private float disableTriggerTimer;

	public List<Buff.BuffExclusiveTags> buffResist;

	private Vector3 inputAimPoint;

	private CharacterActionBase currentAction;

	public CA_Reload reloadAction;

	public CA_Skill skillAction;

	public CA_UseItem useItemAction;

	public static Action<CharacterMainControl, Inventory, int> OnMainCharacterInventoryChangedEvent;

	public static Action<CharacterMainControl, Slot> OnMainCharacterSlotContentChangedEvent;

	private int relatedScene;

	[SerializeField]
	private Health health;

	[SerializeField]
	private CharacterItemControl itemControl;

	public CA_Interact interactAction;

	[SerializeField]
	private CharacterEquipmentController equipmentController;

	public static Action<CharacterMainControl, DuckovItemAgent> OnMainCharacterChangeHoldItemAgentEvent;

	[SerializeField]
	private CharacterBuffManager buffManager;

	private int holdWeaponBeforeUse;

	public CA_Dash dashAction;

	public CA_Attack attackAction;

	public DamageReceiver mainDamageReceiver;

	private HashSet<GameObject> nearByHalfObsticles;

	private List<Item> weaponsTemp = new List<Item>();

	private int weaponSwitchIndex;

	private WeightStates weightState = WeightStates.normal;

	private int meleeWeaponSlotHash = "MeleeWeapon".GetHashCode();

	private int primWeaponSlotHash = "PrimaryWeapon".GetHashCode();

	private int secWeaponSlotHash = "SecondaryWeapon".GetHashCode();

	private float staminaRecoverTimer;

	private float variableTickTimer;

	public const float weightThreshold_Light = 0.25f;

	public const float weightThreshold_Heavy = 0.5f;

	public const float weightThreshold_superWeight = 0.75f;

	private string hideShowRecorder;

	private int walkSpeedHash = "WalkSpeed".GetHashCode();

	private int walkAccHash = "WalkAcc".GetHashCode();

	private int runSpeedHash = "RunSpeed".GetHashCode();

	private int stormProtectionHash = "StormProtection".GetHashCode();

	private int waterEnergyRecoverMultiplierHash = "WaterEnergyRecoverMultiplier".GetHashCode();

	private int gunDistanceMultiplierHash = "GunDistanceMultiplier".GetHashCode();

	private int moveabilityHash = "Moveability".GetHashCode();

	private int runAccHash = "RunAcc".GetHashCode();

	private int turnSpeedHash = "TurnSpeed".GetHashCode();

	private int aimTurnSpeedHash = "AimTurnSpeed".GetHashCode();

	private int dashSpeedHash = "DashSpeed".GetHashCode();

	private int dashCanControlHash = "DashCanControl".GetHashCode();

	private int PetCapcityHash = "PetCapcity".GetHashCode();

	private int maxStaminaHash = "Stamina".GetHashCode();

	private float currentStamina;

	private int staminaDrainRateHash = "StaminaDrainRate".GetHashCode();

	private int staminaRecoverRateHash = "StaminaRecoverRate".GetHashCode();

	private int staminaRecoverTimeHash = "StaminaRecoverTime".GetHashCode();

	private int visableDistanceFactorHash = "VisableDistanceFactor".GetHashCode();

	private int maxWeightHash = "MaxWeight".GetHashCode();

	private int foodGainHash = "FoodGain".GetHashCode();

	private int healGainHash = "HealGain".GetHashCode();

	private int maxEnergyHash = "MaxEnergy".GetHashCode();

	private int energyCostPerMinHash = "EnergyCost".GetHashCode();

	private int currentEnergyHash = "CurrentEnergy".GetHashCode();

	private int maxWaterHash = "MaxWater".GetHashCode();

	private int waterCostPerMinHash = "WaterCost".GetHashCode();

	private bool starve;

	private bool thirsty;

	private int currentWaterHash = "CurrentWater".GetHashCode();

	private int NightVisionAbilityHash = "NightVisionAbility".GetHashCode();

	private int NightVisionTypeHash = "NightVisionType".GetHashCode();

	private int HearingAbilityHash = "HearingAbility".GetHashCode();

	private int SoundVisableHash = "SoundVisable".GetHashCode();

	private int viewAngleHash = "ViewAngle".GetHashCode();

	private int viewDistanceHash = "ViewDistance".GetHashCode();

	private int senseRangeHash = "SenseRange".GetHashCode();

	private static int meleeDamageMultiplierHash = "MeleeDamageMultiplier".GetHashCode();

	private static int meleeCritRateGainHash = "MeleeCritRateGain".GetHashCode();

	private static int meleeCritDamageGainHash = "MeleeCritDamageGain".GetHashCode();

	private static int gunDamageMultiplierHash = "GunDamageMultiplier".GetHashCode();

	private static int reloadSpeedGainHash = "ReloadSpeedGain".GetHashCode();

	private static int gunCritRateGainHash = "GunCritRateGain".GetHashCode();

	private static int gunBulletSpeedMultiplierHash = "BulletSpeedMultiplier".GetHashCode();

	private static int gunCritDamageGainHash = "GunCritDamageGain".GetHashCode();

	private static int recoilControlHash = "RecoilControl".GetHashCode();

	private static int GunScatterMultiplierHash = "GunScatterMultiplier".GetHashCode();

	private static int InventoryCapacityHash = "InventoryCapacity".GetHashCode();

	private static int GasMaskHash = "GasMask".GetHashCode();

	private int walkSoundRangeHash = "WalkSoundRange".GetHashCode();

	private int runSoundRangeHash = "RunSoundRange".GetHashCode();

	private static int flashLightHash = "FlashLight".GetHashCode();

	public AudioManager.VoiceType AudioVoiceType
	{
		get
		{
			return audioVoiceType;
		}
		set
		{
			audioVoiceType = value;
			if (base.gameObject.activeInHierarchy)
			{
				AudioManager.SetVoiceType(base.gameObject, audioVoiceType);
			}
		}
	}

	public AudioManager.FootStepMaterialType FootStepMaterialType
	{
		get
		{
			return footStepMaterialType;
		}
		set
		{
			footStepMaterialType = value;
		}
	}

	public static CharacterMainControl Main
	{
		get
		{
			if (LevelManager.Instance == null)
			{
				return null;
			}
			return LevelManager.Instance.MainCharacter;
		}
	}

	public Teams Team => team;

	public Item CharacterItem => characterItem;

	public DuckovItemAgent CurrentHoldItemAgent => agentHolder.CurrentHoldItemAgent;

	public bool Hidden => hidden;

	public Transform CurrentUsingAimSocket
	{
		get
		{
			if (agentHolder.CurrentUsingSocket == null || GetMeleeWeapon() != null)
			{
				return base.transform;
			}
			return agentHolder.CurrentUsingSocket;
		}
	}

	public Transform RightHandSocket
	{
		get
		{
			if ((bool)characterModel && (bool)characterModel.RightHandSocket)
			{
				return characterModel.RightHandSocket;
			}
			return null;
		}
	}

	public Vector3 CurrentAimDirection => modelRoot.forward;

	public Vector3 CurrentMoveDirection => movementControl.CurrentMoveDirectionXZ;

	public float AnimationMoveSpeedValue => movementControl.GetMoveAnimationValue();

	public Vector2 AnimationLocalMoveDirectionValue => movementControl.GetLocalMoveDirectionAnimationValue();

	public bool Running => movementControl.Running;

	public bool IsOnGround => movementControl.IsOnGround;

	public Vector3 Velocity => movementControl.Velocity;

	public bool ThermalOn
	{
		get
		{
			int num = Mathf.RoundToInt(NightVisionType);
			return GameManager.NightVision.nightVisionTypes[num].thermalOn;
		}
	}

	public bool IsInAdsInput
	{
		get
		{
			if (!adsInput)
			{
				return false;
			}
			if (CurrentAction != null && CurrentAction.Running)
			{
				return false;
			}
			if (Running)
			{
				return false;
			}
			return true;
		}
	}

	public float AdsValue
	{
		get
		{
			ItemAgent_Gun gun = GetGun();
			if ((bool)gun)
			{
				return gun.AdsValue;
			}
			if (CurrentAction != null && CurrentAction.Running)
			{
				return 0f;
			}
			if (Running)
			{
				return 0f;
			}
			return adsInput ? 1 : 0;
		}
	}

	public AimTypes AimType => aimType;

	public bool NeedToSearchTarget
	{
		get
		{
			if (InputManager.InputDevice != InputManager.InputDevices.touch)
			{
				return false;
			}
			if ((bool)GetGun() || (bool)GetMeleeWeapon())
			{
				return true;
			}
			return false;
		}
	}

	public CharacterActionBase CurrentAction => currentAction;

	public Health Health => health;

	public Vector3 MoveInput => movementControl.MoveInput;

	public CharacterEquipmentController EquipmentController => equipmentController;

	public bool Dashing
	{
		get
		{
			if (dashAction != null)
			{
				return dashAction.Running;
			}
			return false;
		}
	}

	public bool IsMainCharacter
	{
		get
		{
			if (LevelManager.Instance == null)
			{
				return false;
			}
			return LevelManager.Instance.MainCharacter == this;
		}
	}

	public float CharacterWalkSpeed
	{
		get
		{
			float floatStatValue = GetFloatStatValue(walkSpeedHash);
			floatStatValue *= CharacterMoveability;
			ItemAgent_Gun gun = GetGun();
			if ((bool)gun)
			{
				float moveSpeedMultiplier = gun.MoveSpeedMultiplier;
				if (moveSpeedMultiplier > 0f)
				{
					floatStatValue *= moveSpeedMultiplier;
				}
			}
			else
			{
				ItemAgent_MeleeWeapon meleeWeapon = GetMeleeWeapon();
				if ((bool)meleeWeapon)
				{
					float moveSpeedMultiplier2 = meleeWeapon.MoveSpeedMultiplier;
					if (moveSpeedMultiplier2 > 0f)
					{
						floatStatValue *= moveSpeedMultiplier2;
					}
				}
			}
			return floatStatValue;
		}
	}

	public float AdsWalkSpeedMultiplier
	{
		get
		{
			ItemAgent_Gun gun = GetGun();
			if ((bool)gun)
			{
				return gun.AdsWalkSpeedMultiplier;
			}
			return 0.5f;
		}
	}

	public float CharacterOriginWalkSpeed
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStat(walkSpeedHash).BaseValue;
			}
			return 0f;
		}
	}

	public float CharacterRunSpeed
	{
		get
		{
			float floatStatValue = GetFloatStatValue(runSpeedHash);
			floatStatValue *= CharacterMoveability;
			ItemAgent_Gun gun = GetGun();
			if ((bool)gun)
			{
				float moveSpeedMultiplier = gun.MoveSpeedMultiplier;
				if (moveSpeedMultiplier > 0f)
				{
					floatStatValue *= moveSpeedMultiplier;
				}
			}
			else
			{
				ItemAgent_MeleeWeapon meleeWeapon = GetMeleeWeapon();
				if ((bool)meleeWeapon)
				{
					float moveSpeedMultiplier2 = meleeWeapon.MoveSpeedMultiplier;
					if (moveSpeedMultiplier2 > 0f)
					{
						floatStatValue *= moveSpeedMultiplier2;
					}
				}
			}
			return floatStatValue;
		}
	}

	public float StormProtection => GetFloatStatValue(stormProtectionHash);

	public float WaterEnergyRecoverMultiplier => GetFloatStatValue(waterEnergyRecoverMultiplierHash);

	public float GunDistanceMultiplier => GetFloatStatValue(gunDistanceMultiplierHash);

	public float CharacterMoveability => GetFloatStatValue(moveabilityHash);

	public float CharacterRunAcc => GetFloatStatValue(runAccHash);

	public float CharacterTurnSpeed => GetFloatStatValue(turnSpeedHash) * CharacterMoveability;

	public float CharacterAimTurnSpeed => GetFloatStatValue(aimTurnSpeedHash) * CharacterMoveability;

	public float DashSpeed => GetFloatStatValue(dashSpeedHash);

	public int PetCapcity => Mathf.RoundToInt(GetFloatStatValue(PetCapcityHash));

	public bool DashCanControl
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(dashCanControlHash) > 0f;
			}
			return true;
		}
		set
		{
			characterItem.GetStat(dashCanControlHash).BaseValue = (value ? 1 : 0);
		}
	}

	public float MaxStamina
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(maxStaminaHash);
			}
			return 0f;
		}
	}

	public float CurrentStamina => currentStamina;

	public float StaminaDrainRate
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(staminaDrainRateHash);
			}
			return 0f;
		}
	}

	public float StaminaRecoverRate
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(staminaRecoverRateHash);
			}
			return 0f;
		}
	}

	public float StaminaRecoverTime
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(staminaRecoverTimeHash);
			}
			return 0f;
		}
	}

	public float CharacterWalkAcc => GetFloatStatValue(walkAccHash);

	public float VisableDistanceFactor => GetFloatStatValue(visableDistanceFactorHash);

	public float MaxWeight
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(maxWeightHash);
			}
			return 0f;
		}
	}

	public float FoodGain
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(foodGainHash);
			}
			return 0f;
		}
	}

	public float HealGain
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(healGainHash);
			}
			return 0f;
		}
	}

	public float MaxEnergy
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(maxEnergyHash);
			}
			return 0f;
		}
	}

	public float EnergyCostPerMin
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(energyCostPerMinHash);
			}
			return 0f;
		}
	}

	public float CurrentEnergy
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.Variables.GetFloat(currentEnergyHash);
			}
			return 0f;
		}
		set
		{
			if ((bool)characterItem)
			{
				characterItem.Variables.SetFloat(currentEnergyHash, value);
			}
		}
	}

	public float MaxWater
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(maxWaterHash);
			}
			return 0f;
		}
	}

	public float WaterCostPerMin
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(waterCostPerMinHash);
			}
			return 0f;
		}
	}

	public float CurrentWater
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.Variables.GetFloat(currentWaterHash);
			}
			return 0f;
		}
		set
		{
			if ((bool)characterItem)
			{
				characterItem.Variables.SetFloat(currentWaterHash, value);
			}
		}
	}

	public float NightVisionAbility => GetFloatStatValue(NightVisionAbilityHash);

	public float NightVisionType => GetFloatStatValue(NightVisionTypeHash);

	public float HearingAbility => GetFloatStatValue(HearingAbilityHash);

	public float SoundVisable => GetFloatStatValue(SoundVisableHash);

	public float ViewAngle => GetFloatStatValue(viewAngleHash);

	public float ViewDistance => GetFloatStatValue(viewDistanceHash);

	public float SenseRange => GetFloatStatValue(senseRangeHash);

	public float MeleeDamageMultiplier
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(meleeDamageMultiplierHash);
			}
			return 0f;
		}
	}

	public float MeleeCritRateGain
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(meleeCritRateGainHash);
			}
			return 0f;
		}
	}

	public float MeleeCritDamageGain
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(meleeCritDamageGainHash);
			}
			return 0f;
		}
	}

	public float GunDamageMultiplier
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(gunDamageMultiplierHash);
			}
			return 0f;
		}
	}

	public float ReloadSpeedGain
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(reloadSpeedGainHash);
			}
			return 0f;
		}
	}

	public float GunCritRateGain
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(gunCritRateGainHash);
			}
			return 0f;
		}
	}

	public float GunCritDamageGain
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(gunCritDamageGainHash);
			}
			return 0f;
		}
	}

	public float GunBulletSpeedMultiplier
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(gunBulletSpeedMultiplierHash);
			}
			return 1f;
		}
	}

	public float RecoilControl
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(recoilControlHash);
			}
			return 1f;
		}
	}

	public float GunScatterMultiplier
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(GunScatterMultiplierHash);
			}
			return 1f;
		}
	}

	public float InventoryCapacity
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(InventoryCapacityHash);
			}
			return 16f;
		}
	}

	public bool HasGasMask
	{
		get
		{
			float num = 0f;
			if ((bool)characterItem)
			{
				num = characterItem.GetStatValue(GasMaskHash);
			}
			return num > 0.1f;
		}
	}

	public float WalkSoundRange
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(walkSoundRangeHash);
			}
			return 0f;
		}
	}

	public float RunSoundRange
	{
		get
		{
			if ((bool)characterItem)
			{
				return characterItem.GetStatValue(runSoundRangeHash);
			}
			return 0f;
		}
	}

	public bool FlashLight
	{
		get
		{
			if ((bool)CurrentHoldItemAgent)
			{
				return CurrentHoldItemAgent.Item.GetStatValue(flashLightHash) > 0f;
			}
			return false;
		}
	}

	public string SoundKey => "Default";

	public event Action<Teams> OnTeamChanged;

	public event Action<CharacterMainControl, Vector3> OnSetPositionEvent;

	public event Action<DamageInfo> BeforeCharacterSpawnLootOnDead;

	public static event Action<Item> OnMainCharacterStartUseItem;

	public event Action<CharacterActionBase> OnActionStartEvent;

	public event Action<CharacterActionBase> OnActionProgressFinishEvent;

	public event Action<DuckovItemAgent> OnHoldAgentChanged;

	public event Action<DuckovItemAgent> OnShootEvent;

	public event Action TryCatchFishInputEvent;

	public event Action<DuckovItemAgent> OnAttackEvent;

	public event Action OnSkillStartReleaseEvent;

	public float GetAimRange()
	{
		float result = 8f;
		switch (aimType)
		{
		case AimTypes.normalAim:
		{
			ItemAgent_Gun gun = GetGun();
			if (gun != null)
			{
				result = gun.BulletDistance;
				result -= 0.4f;
				break;
			}
			ItemAgent_MeleeWeapon meleeWeapon = GetMeleeWeapon();
			if (meleeWeapon != null)
			{
				result = meleeWeapon.AttackRange;
			}
			break;
		}
		case AimTypes.characterSkill:
		{
			SkillBase skill3 = skillAction.characterSkillKeeper.Skill;
			if ((bool)skill3)
			{
				result = skill3.SkillContext.castRange;
			}
			break;
		}
		case AimTypes.handheldSkill:
		{
			ItemSetting_Skill skill = agentHolder.Skill;
			if ((bool)skill)
			{
				SkillBase skill2 = skill.Skill;
				if ((bool)skill2)
				{
					result = skill2.SkillContext.castRange;
				}
			}
			break;
		}
		}
		return result;
	}

	public Vector3 GetCurrentAimPoint()
	{
		return inputAimPoint;
	}

	public Vector3 GetCurrentSkillAimPoint()
	{
		SkillBase currentRunningSkill = skillAction.CurrentRunningSkill;
		if (!currentRunningSkill)
		{
			return inputAimPoint;
		}
		float castRange = currentRunningSkill.SkillContext.castRange;
		float y = inputAimPoint.y;
		Vector3 vector = inputAimPoint - base.transform.position;
		vector.y = 0f;
		float num = vector.magnitude;
		vector.Normalize();
		if (num > castRange)
		{
			num = castRange;
		}
		Vector3 result = base.transform.position + vector * num;
		result.y = y;
		return result;
	}

	private void OnMainCharacterInventoryChanged(Inventory inventory, int index)
	{
		ItemUtilities.NotifyPlayerItemOperation();
		OnMainCharacterInventoryChangedEvent?.Invoke(this, inventory, index);
	}

	private void OnMainCharacterSlotContentChanged(Item item, Slot slot)
	{
		OnMainCharacterSlotContentChangedEvent?.Invoke(this, slot);
		CheckTakeoutWeaponWhileEquip(slot);
	}

	private void CheckTakeoutWeaponWhileEquip(Slot slot)
	{
		if (!(slot.Content == null) && !(CurrentHoldItemAgent != null) && slot.Content.Tags.Contains("Weapon"))
		{
			agentHolder.ChangeHoldItem(slot.Content);
		}
	}

	public void SwitchWeapon(int dir)
	{
		weaponsTemp.Clear();
		weaponSwitchIndex = -1;
		Item item = null;
		if (CurrentHoldItemAgent != null)
		{
			item = CurrentHoldItemAgent.Item;
		}
		Item content = PrimWeaponSlot().Content;
		if ((bool)content)
		{
			weaponsTemp.Add(content);
			if (item == content)
			{
				weaponSwitchIndex = weaponsTemp.Count - 1;
			}
		}
		content = SecWeaponSlot().Content;
		if ((bool)content)
		{
			weaponsTemp.Add(content);
			if (item == content)
			{
				weaponSwitchIndex = weaponsTemp.Count - 1;
			}
		}
		content = MeleeWeaponSlot().Content;
		if ((bool)content)
		{
			weaponsTemp.Add(content);
			if (item == content)
			{
				weaponSwitchIndex = weaponsTemp.Count - 1;
			}
		}
		if (weaponsTemp.Count > 0)
		{
			weaponSwitchIndex -= dir;
			if (weaponSwitchIndex < 0)
			{
				weaponSwitchIndex = weaponsTemp.Count - 1;
			}
			if (weaponSwitchIndex >= weaponsTemp.Count)
			{
				weaponSwitchIndex = 0;
			}
			ChangeHoldItem(weaponsTemp[weaponSwitchIndex]);
		}
	}

	public bool CanEditInventory()
	{
		if ((bool)currentAction && currentAction.Running && !currentAction.CanEditInventory())
		{
			return false;
		}
		return true;
	}

	public void SetMoveInput(Vector3 moveInput)
	{
		movementControl.SetMoveInput(moveInput);
	}

	public Slot MeleeWeaponSlot()
	{
		return GetSlot(meleeWeaponSlotHash);
	}

	public Slot PrimWeaponSlot()
	{
		return GetSlot(primWeaponSlotHash);
	}

	public Slot SecWeaponSlot()
	{
		return GetSlot(secWeaponSlotHash);
	}

	public Slot GetSlot(int hash)
	{
		if (characterItem == null)
		{
			return null;
		}
		return characterItem.Slots.GetSlot(hash);
	}

	private void Awake()
	{
		nearByHalfObsticles = new HashSet<GameObject>();
		agentHolder.OnHoldAgentChanged += OnChangeItemAgentChangedFunc;
	}

	private void StoreHoldWeaponBeforeUse()
	{
		if ((bool)agentHolder.CurrentHoldItemAgent)
		{
			Item item = agentHolder.CurrentHoldItemAgent.Item;
			if (item == MeleeWeaponSlot().Content)
			{
				holdWeaponBeforeUse = -1;
			}
			else if (item == PrimWeaponSlot().Content)
			{
				holdWeaponBeforeUse = 0;
			}
			else if (item == SecWeaponSlot().Content)
			{
				holdWeaponBeforeUse = 1;
			}
		}
	}

	public bool SwitchToFirstAvailableWeapon()
	{
		if (SwitchToWeapon(0))
		{
			return true;
		}
		if (SwitchToWeapon(1))
		{
			return true;
		}
		return SwitchToWeapon(-1);
	}

	public bool SwitchToWeapon(int index)
	{
		Slot slot = null;
		if (index == -1)
		{
			slot = MeleeWeaponSlot();
		}
		switch (index)
		{
		case 0:
			slot = PrimWeaponSlot();
			break;
		case 1:
			slot = SecWeaponSlot();
			break;
		}
		if (slot == null)
		{
			return false;
		}
		Item content = slot.Content;
		if (content == null)
		{
			return false;
		}
		ChangeHoldItem(content);
		return true;
	}

	public void ToggleNightVision()
	{
		Item faceMaskItem = GetFaceMaskItem();
		if ((bool)faceMaskItem)
		{
			ItemSetting_NightVision component = faceMaskItem.GetComponent<ItemSetting_NightVision>();
			if ((bool)component)
			{
				component.ToggleNightVison();
			}
		}
	}

	public void Dash()
	{
		if (!(dashAction == null) && (!attackAction.Running || attackAction.DamageDealed) && StartAction(dashAction) && !DashCanControl && disableTriggerTimer < 0.6f)
		{
			disableTriggerTimer = 0.6f;
		}
	}

	public void TryCatchFishInput()
	{
		if ((bool)currentAction && currentAction.Running)
		{
			Action_FishingV2 action_FishingV = currentAction as Action_FishingV2;
			if ((bool)action_FishingV)
			{
				action_FishingV.TryCatch();
			}
		}
	}

	public bool HasNearByHalfObsticle()
	{
		if (nearByHalfObsticles.Count <= 0)
		{
			return false;
		}
		foreach (GameObject nearByHalfObsticle in nearByHalfObsticles)
		{
			if (nearByHalfObsticle != null)
			{
				return true;
			}
		}
		return false;
	}

	public void SwitchToWeaponBeforeUse()
	{
		SwitchToWeapon(holdWeaponBeforeUse);
		holdWeaponBeforeUse = -1;
	}

	public void SetForceMoveVelocity(Vector3 _velocity)
	{
		movementControl.SetForceMoveVelocity(_velocity);
	}

	public void SetAimPoint(Vector3 _aimPoint)
	{
		inputAimPoint = _aimPoint;
	}

	public bool Attack()
	{
		if (GetMeleeWeapon() == null)
		{
			return false;
		}
		if (!attackAction.IsReady())
		{
			return false;
		}
		bool result = StartAction(attackAction);
		Action<DuckovItemAgent> action = this.OnAttackEvent;
		if (action != null)
		{
			action(GetMeleeWeapon());
			return result;
		}
		return result;
	}

	public void SetAimType(AimTypes _aimType)
	{
		aimType = _aimType;
	}

	public void SetRunInput(bool _runInput)
	{
		runInput = _runInput;
	}

	public void SetAdsInput(bool _adsInput)
	{
		adsInput = _adsInput;
	}

	public bool TryToReload(Item preferedBulletToLoad = null)
	{
		reloadAction.preferedBulletToReload = preferedBulletToLoad;
		bool num = StartAction(reloadAction);
		if (!num)
		{
			reloadAction.preferedBulletToReload = null;
		}
		return num;
	}

	public bool SetSkill(SkillTypes skillType, SkillBase skill, GameObject bindingObject)
	{
		return skillAction.SetSkillOfType(skillType, skill, bindingObject);
	}

	public bool StartSkillAim(SkillTypes skillType)
	{
		if (skillAction.Running)
		{
			return false;
		}
		skillAction.SetNextSkillType(skillType);
		return StartAction(skillAction);
	}

	public bool ReleaseSkill(SkillTypes skillType)
	{
		this.OnSkillStartReleaseEvent?.Invoke();
		return skillAction.ReleaseSkill(skillType);
	}

	public bool CancleSkill()
	{
		return skillAction.StopAction();
	}

	public SkillBase GetCurrentRunningSkill()
	{
		if (!skillAction.Running)
		{
			return null;
		}
		return skillAction.CurrentRunningSkill;
	}

	public bool GetGunReloadable()
	{
		return reloadAction.GetGunReloadable();
	}

	public bool CanUseHand()
	{
		if (currentAction != null && !currentAction.CanUseHand())
		{
			return false;
		}
		return true;
	}

	public bool CanControlAim()
	{
		if (currentAction != null && !currentAction.CanControlAim())
		{
			return false;
		}
		return true;
	}

	public bool StartAction(CharacterActionBase newAction)
	{
		if (!newAction.IsReady())
		{
			return false;
		}
		bool flag = true;
		if ((bool)currentAction && currentAction.Running)
		{
			flag = newAction.ActionPriority() > currentAction.ActionPriority() && currentAction.StopAction();
		}
		if (flag)
		{
			currentAction = null;
			if (newAction.StartActionByCharacter(this))
			{
				currentAction = newAction;
				this.OnActionStartEvent?.Invoke(currentAction);
				return true;
			}
		}
		return false;
	}

	public void SwitchHoldAgentInSlot(int slotHash)
	{
		ChangeHoldItem(characterItem.Slots.GetSlot(slotHash)?.Content);
	}

	public void SwitchInteractSelection(int dir)
	{
		interactAction.SwitchInteractable(dir);
	}

	public void SetTeam(Teams _team)
	{
		team = _team;
		health.team = team;
		this.OnTeamChanged?.Invoke(_team);
		if (Main == this)
		{
			characterItem.Inventory.onContentChanged -= OnMainCharacterInventoryChanged;
			characterItem.Inventory.onContentChanged += OnMainCharacterInventoryChanged;
			characterItem.onSlotContentChanged -= OnMainCharacterSlotContentChanged;
			characterItem.onSlotContentChanged += OnMainCharacterSlotContentChanged;
		}
		if ((bool)characterModel)
		{
			characterModel.SyncHiddenToMainCharacter();
		}
	}

	public ItemAgent_Gun GetGun()
	{
		return agentHolder.CurrentHoldGun;
	}

	public ItemAgent_MeleeWeapon GetMeleeWeapon()
	{
		return agentHolder.CurrentHoldMeleeWeapon;
	}

	public bool ChangeHoldItem(Item item)
	{
		if (!CanEditInventory())
		{
			return false;
		}
		if (agentHolder.CurrentHoldItemAgent != null && item == agentHolder.CurrentHoldItemAgent.Item)
		{
			return false;
		}
		agentHolder.ChangeHoldItem(item);
		return true;
	}

	private void OnChangeItemAgentChangedFunc(DuckovItemAgent agent)
	{
		this.OnHoldAgentChanged?.Invoke(agent);
		if (IsMainCharacter)
		{
			OnMainCharacterChangeHoldItemAgentEvent?.Invoke(this, agent);
		}
	}

	private void Update()
	{
		if (LevelManager.LevelInited)
		{
			interactAction.SearchInteractableAround();
			UpdateAction(Time.deltaTime);
			movementControl.UpdateMovement();
			UpdateStats(Time.deltaTime);
			TickVariables(Time.deltaTime, 1f);
			if (IsMainCharacter)
			{
				UpdateThirstyAndStarve();
				UpdateWeightState();
			}
			disableTriggerTimer -= Time.deltaTime;
		}
	}

	private void LateUpdate()
	{
		UpdateInventoryCapacity();
	}

	public void SetItem(Item _item)
	{
		if (!(_item == null))
		{
			characterItem = _item;
			_item.transform.SetParent(base.transform, worldPositionStays: false);
			currentStamina = MaxStamina;
			health.SetItemAndCharacter(_item, this);
			health.OnDeadEvent.AddListener(OnDead);
			equipmentController.SetItem(_item);
			_item.Inventory.SetCapacity(Mathf.RoundToInt(InventoryCapacity));
			health.Init();
		}
	}

	private void UpdateInventoryCapacity()
	{
		if (LevelManager.Instance.MainCharacter != this || characterItem == null || characterItem.Inventory == null || characterItem.Inventory.Loading)
		{
			return;
		}
		int num = Mathf.RoundToInt(InventoryCapacity);
		int capacity = characterItem.Inventory.Capacity;
		if (capacity == num)
		{
			return;
		}
		characterItem.Inventory.SetCapacity(num);
		if (capacity <= num)
		{
			return;
		}
		int count = characterItem.Inventory.Content.Count;
		if (count < num)
		{
			return;
		}
		List<Item> list = new List<Item>();
		for (int i = num; i < count; i++)
		{
			Item item = characterItem.Inventory.Content[i];
			if (item != null)
			{
				list.Add(item);
				item.Detach();
			}
		}
		foreach (Item item2 in list)
		{
			if (!characterItem.Inventory.AddAndMerge(item2))
			{
				item2.Drop(base.transform.position, createRigidbody: true, Vector3.forward, 360f);
			}
		}
	}

	private void OnDead(DamageInfo dmgInfo)
	{
		if (LevelManager.Instance.MainCharacter != this)
		{
			Quaternion rotation = Quaternion.identity;
			if ((bool)characterModel)
			{
				rotation = characterModel.transform.rotation;
			}
			if ((bool)dmgInfo.fromCharacter && dmgInfo.fromCharacter.IsMainCharacter && (bool)characterPreset && characterPreset.nameKey != "")
			{
				SavesCounter.AddKillCount(characterPreset.nameKey);
			}
			this.BeforeCharacterSpawnLootOnDead?.Invoke(dmgInfo);
			InteractableLootbox.CreateFromItem(characterItem, base.transform.position + Vector3.up * 0.1f, rotation, moveToMainScene: true, deadLootBoxPrefab, IsMainCharacter);
		}
		if (relatedScene != -1)
		{
			SetActiveByPlayerDistance.Unregister(base.gameObject, relatedScene);
		}
	}

	public void Trigger(bool trigger, bool triggerThisFrame, bool releaseThisFrame)
	{
		if (Running || disableTriggerTimer > 0f)
		{
			trigger = false;
			triggerThisFrame = false;
		}
		else if (trigger && CharacterMoveability > 0.5f)
		{
			movementControl.ForceSetAimDirectionToAimPoint();
		}
		agentHolder.SetTrigger(trigger, triggerThisFrame, releaseThisFrame);
	}

	public bool CanMove()
	{
		if (currentAction != null && !currentAction.CanMove())
		{
			return false;
		}
		if (CharacterWalkSpeed <= 0f)
		{
			return false;
		}
		return true;
	}

	public void PopText(string text, float speed = -1f)
	{
		if (LevelManager.LevelInited && (bool)Main && !(Vector3.Distance(base.transform.position, Main.transform.position) > 55f))
		{
			float yOffset = 2f;
			if ((bool)characterModel && (bool)characterModel.HelmatSocket)
			{
				yOffset = Vector3.Distance(base.transform.position, characterModel.HelmatSocket.position) + 0.5f;
			}
			DialogueBubblesManager.Show(text, base.transform, yOffset, needInteraction: false, skippable: false, speed).Forget();
		}
	}

	public bool CanRun()
	{
		if (currentAction != null && !currentAction.CanRun())
		{
			return false;
		}
		float num = currentStamina / MaxStamina;
		if (num < 0.2f && !Running)
		{
			return false;
		}
		if (num <= 0f)
		{
			return false;
		}
		return runInput;
	}

	public bool IsAiming()
	{
		if (!movementControl.Running && (currentAction == null || !currentAction.Running || currentAction.CanControlAim()))
		{
			return true;
		}
		return false;
	}

	public void DestroyCharacter()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void TriggerShootEvent(DuckovItemAgent shootByAgent)
	{
		this.OnShootEvent?.Invoke(shootByAgent);
	}

	public void SetCharacterModel(CharacterModel _characterModel)
	{
		bool flag = true;
		if (characterModel != null)
		{
			flag = false;
			UnityEngine.Object.Destroy(characterModel.gameObject);
		}
		characterModel = _characterModel;
		_characterModel.OnMainCharacterSetted(this);
		_characterModel.transform.SetParent(modelRoot, worldPositionStays: false);
		_characterModel.transform.localPosition = Vector3.zero;
		_characterModel.transform.localRotation = quaternion.identity;
		Transform helmatSocket = _characterModel.HelmatSocket;
		if ((bool)helmatSocket)
		{
			HeadCollider headCollider = UnityEngine.Object.Instantiate(GameplayDataSettings.Prefabs.HeadCollider, helmatSocket);
			headCollider.transform.localPosition = Vector3.zero;
			headCollider.transform.localScale = Vector3.one;
			headCollider.Init(this);
			CapsuleCollider component = mainDamageReceiver.GetComponent<CapsuleCollider>();
			if ((bool)component)
			{
				float num = (component.height = headCollider.transform.localScale.y * 0.5f + headCollider.transform.position.y - base.transform.position.y + 0.5f);
				component.center = Vector3.up * num * 0.5f;
			}
		}
		if (!LevelManager.LevelInited || flag || !(characterItem != null))
		{
			return;
		}
		foreach (Slot slot in characterItem.Slots)
		{
			if ((bool)slot.Content)
			{
				slot.ForceInvokeSlotContentChangedEvent();
			}
		}
	}

	private void OnDestroy()
	{
		if ((bool)characterItem && (bool)characterItem.Inventory)
		{
			characterItem.Inventory.onContentChanged -= OnMainCharacterInventoryChanged;
		}
		if ((bool)characterItem)
		{
			characterItem.DestroyTree();
		}
		if ((bool)health)
		{
			health.OnDeadEvent.RemoveListener(OnDead);
		}
		if (relatedScene != -1)
		{
			SetActiveByPlayerDistance.Unregister(base.gameObject, relatedScene);
		}
	}

	private void UpdateAction(float deltaTime)
	{
		if ((bool)currentAction)
		{
			currentAction.UpdateAction(deltaTime);
			if ((bool)currentAction && !currentAction.Running)
			{
				currentAction = null;
			}
		}
	}

	private void UpdateStats(float deltaTime)
	{
		if (movementControl.Running)
		{
			UseStamina(StaminaDrainRate * deltaTime);
			return;
		}
		staminaRecoverTimer += deltaTime;
		if (staminaRecoverTimer >= StaminaRecoverTime)
		{
			currentStamina = Mathf.MoveTowards(currentStamina, MaxStamina, StaminaRecoverRate * deltaTime);
		}
	}

	public void TickVariables(float deltaTime, float tickTime)
	{
		variableTickTimer += deltaTime;
		if (variableTickTimer < tickTime)
		{
			return;
		}
		variableTickTimer = 0f;
		if (!IsMainCharacter)
		{
			return;
		}
		float currentEnergy = CurrentEnergy;
		if (!LevelManager.Instance.IsRaidMap || health.Invincible)
		{
			currentEnergy += 10f * WaterEnergyRecoverMultiplier * tickTime / 60f;
			if (currentEnergy < MaxEnergy * 0.25f)
			{
				currentEnergy = MaxEnergy * 0.25f;
			}
			else if (currentEnergy > MaxEnergy)
			{
				currentEnergy = MaxEnergy;
			}
		}
		else
		{
			currentEnergy -= EnergyCostPerMin * tickTime / 60f;
			if (currentEnergy < 0f)
			{
				currentEnergy = 0f;
			}
		}
		CurrentEnergy = currentEnergy;
		float currentWater = CurrentWater;
		if (!LevelManager.Instance.IsRaidMap || health.Invincible)
		{
			currentWater += 10f * WaterEnergyRecoverMultiplier * tickTime / 60f;
			if (currentWater < MaxWater * 0.25f)
			{
				currentWater = MaxWater * 0.25f;
			}
			else if (currentWater > MaxWater)
			{
				currentWater = MaxWater;
			}
		}
		else
		{
			currentWater -= WaterCostPerMin * tickTime / 60f;
			if (currentWater < 0f)
			{
				currentWater = 0f;
			}
		}
		CurrentWater = currentWater;
	}

	public void UpdateThirstyAndStarve()
	{
		if (CurrentWater <= 0f != thirsty)
		{
			thirsty = !thirsty;
			if (thirsty)
			{
				AddBuff(GameplayDataSettings.Buffs.Thirsty, this);
			}
			else
			{
				RemoveBuffsByTag(Buff.BuffExclusiveTags.Thirsty, removeOneLayer: false);
			}
		}
		if (CurrentEnergy <= 0f != starve)
		{
			starve = !starve;
			if (starve)
			{
				AddBuff(GameplayDataSettings.Buffs.Starve, this);
			}
			else
			{
				RemoveBuffsByTag(Buff.BuffExclusiveTags.Starve, removeOneLayer: false);
			}
		}
	}

	public void UpdateWeightState()
	{
		float num = CharacterItem.TotalWeight;
		if (carryAction.Running)
		{
			num += carryAction.GetWeight();
		}
		float num2 = num / MaxWeight;
		WeightStates weightStates = WeightStates.light;
		if (!LevelManager.Instance.IsRaidMap)
		{
			weightStates = WeightStates.normal;
		}
		else if (num2 > 1f)
		{
			weightStates = WeightStates.overWeight;
		}
		else if (num2 > 0.75f)
		{
			weightStates = WeightStates.superHeavy;
		}
		else if (num2 > 0.25f)
		{
			weightStates = WeightStates.normal;
		}
		if (weightStates != weightState)
		{
			weightState = weightStates;
			RemoveBuffsByTag(Buff.BuffExclusiveTags.Weight, removeOneLayer: false);
			switch (weightStates)
			{
			case WeightStates.light:
				AddBuff(GameplayDataSettings.Buffs.Weight_Light, this);
				break;
			case WeightStates.heavy:
				AddBuff(GameplayDataSettings.Buffs.Weight_Heavy, this);
				break;
			case WeightStates.superHeavy:
				AddBuff(GameplayDataSettings.Buffs.Weight_SuperHeavy, this);
				break;
			case WeightStates.overWeight:
				AddBuff(GameplayDataSettings.Buffs.Weight_Overweight, this);
				break;
			case WeightStates.normal:
				break;
			}
		}
	}

	public bool PickupItem(Item item)
	{
		if (health.IsDead)
		{
			return false;
		}
		item.Inspected = true;
		return itemControl.PickupItem(item);
	}

	public InteractableBase GetInteractableTargetToInteract()
	{
		if ((bool)currentAction && currentAction.ActionPriority() >= interactAction.ActionPriority())
		{
			return null;
		}
		return interactAction.InteractTarget;
	}

	public void Interact(InteractableBase _target)
	{
		if (!currentAction || currentAction.ActionPriority() < interactAction.ActionPriority())
		{
			interactAction.SetInteractableTarget(_target);
			Interact();
		}
	}

	public void Interact()
	{
		if (!health.IsDead)
		{
			if (carryAction.Running)
			{
				carryAction.StopAction();
			}
			else if (!currentAction && GetInteractableTargetToInteract() != null)
			{
				StartAction(interactAction);
			}
		}
	}

	public void AddHealth(float healthValue)
	{
		health.AddHealth(healthValue * (1f + HealGain));
	}

	public void SetRelatedScene(int _relatedScene, bool setActiveByPlayerDistance = true)
	{
		relatedScene = _relatedScene;
		if ((bool)MultiSceneCore.Instance)
		{
			MultiSceneCore.MoveToActiveWithScene(base.gameObject, _relatedScene);
			if (setActiveByPlayerDistance)
			{
				SetActiveByPlayerDistance.Register(base.gameObject, relatedScene);
			}
		}
	}

	public void Carry(Carriable target)
	{
		if ((bool)carryAction && (!(currentAction != null) || !currentAction.Running))
		{
			carryAction.carryTarget = target;
			StartAction(carryAction);
		}
	}

	public void AddEnergy(float energyValue)
	{
		float currentEnergy = CurrentEnergy;
		currentEnergy += energyValue * (1f + FoodGain);
		if (currentEnergy > MaxEnergy)
		{
			currentEnergy = MaxEnergy;
		}
		if (currentEnergy < 0f)
		{
			currentEnergy = 0f;
		}
		CurrentEnergy = currentEnergy;
	}

	public void AddWater(float waterValue)
	{
		float currentWater = CurrentWater;
		currentWater += waterValue * (1f + FoodGain);
		if (currentWater > MaxWater)
		{
			currentWater = MaxWater;
		}
		if (currentWater < 0f)
		{
			currentWater = 0f;
		}
		CurrentWater = currentWater;
	}

	public void DropAllItems()
	{
		if (characterItem == null)
		{
			return;
		}
		List<Item> list = new List<Item>();
		if (characterItem.Inventory != null)
		{
			foreach (Item item in characterItem.Inventory)
			{
				if ((!IsMainCharacter || !item.Tags.Contains(GameplayDataSettings.Tags.DontDropOnDeadInSlot)) && (!IsMainCharacter || !item.Sticky))
				{
					list.Add(item);
				}
			}
		}
		foreach (Slot slot in characterItem.Slots)
		{
			if (slot.Content != null && (!IsMainCharacter || !slot.Content.Tags.Contains(GameplayDataSettings.Tags.DontDropOnDeadInSlot)) && (!IsMainCharacter || !slot.Content.Sticky))
			{
				list.Add(slot.Content);
			}
		}
		foreach (Item item2 in list)
		{
			if (!IsMainCharacter || !item2.Sticky)
			{
				item2.Drop(base.transform.position, createRigidbody: true, Vector3.forward, 360f);
			}
		}
	}

	public void DestroyAllItem()
	{
		if (characterItem == null)
		{
			return;
		}
		List<Item> list = new List<Item>();
		if (characterItem.Inventory != null)
		{
			foreach (Item item in characterItem.Inventory)
			{
				list.Add(item);
			}
		}
		foreach (Slot slot in characterItem.Slots)
		{
			if (slot.Content != null && (!IsMainCharacter || !slot.Content.Tags.Contains(GameplayDataSettings.Tags.DontDropOnDeadInSlot)) && (!IsMainCharacter || !slot.Content.Sticky))
			{
				list.Add(slot.Content);
			}
		}
		foreach (Item item2 in list)
		{
			if (!IsMainCharacter || !item2.Sticky)
			{
				item2.DestroyTree();
			}
		}
	}

	public void DestroyItemsThatNeededToBeDestriedInBase()
	{
		if (characterItem == null)
		{
			return;
		}
		List<Item> list = new List<Item>();
		if (characterItem.Inventory != null)
		{
			foreach (Item item in characterItem.Inventory)
			{
				list.Add(item);
			}
		}
		foreach (Slot slot in characterItem.Slots)
		{
			if (slot.Content != null)
			{
				list.Add(slot.Content);
			}
		}
		foreach (Item item2 in list)
		{
			if (item2.Tags.Contains("DestroyInBase"))
			{
				item2.DestroyTree();
			}
		}
	}

	public void AddSubVisuals(CharacterSubVisuals subVisuals)
	{
		if ((bool)characterModel)
		{
			characterModel.AddSubVisuals(subVisuals);
		}
	}

	public void RemoveVisual(CharacterSubVisuals subVisuals)
	{
		if ((bool)characterModel)
		{
			characterModel.RemoveVisual(subVisuals);
		}
	}

	public void Hide()
	{
		if (!hidden)
		{
			hidden = true;
			if ((bool)characterModel)
			{
				characterModel.SyncHiddenToMainCharacter();
			}
		}
	}

	public void Show()
	{
		health?.RequestHealthBar();
		if (hidden)
		{
			hidden = false;
			if ((bool)characterModel)
			{
				characterModel.SyncHiddenToMainCharacter();
			}
		}
	}

	private void OnEnable()
	{
		if (IsMainCharacter && (bool)health)
		{
			health.showHealthBar = true;
			health.RequestHealthBar();
		}
		AudioManager.SetVoiceType(base.gameObject, audioVoiceType);
	}

	public bool IsNearByHalfObsticle(GameObject target)
	{
		if (target == null || nearByHalfObsticles.Count == 0)
		{
			return false;
		}
		return nearByHalfObsticles.Contains(target);
	}

	public GameObject[] GetNearByHalfObsticles()
	{
		nearByHalfObsticles.RemoveWhere((GameObject go) => go == null);
		return nearByHalfObsticles.ToArray();
	}

	public void AddnearByHalfObsticles(List<GameObject> objs)
	{
		foreach (GameObject obj in objs)
		{
			if (!(obj == null) && !nearByHalfObsticles.Contains(obj))
			{
				nearByHalfObsticles.Add(obj);
			}
		}
	}

	public void RemoveNearByHalfObsticles(List<GameObject> objs)
	{
		foreach (GameObject obj in objs)
		{
			if (!(obj == null) && nearByHalfObsticles.Contains(obj))
			{
				nearByHalfObsticles.Remove(obj);
			}
		}
	}

	public void UseItem(Item item)
	{
		if (IsMainCharacter && !item.UsageUtilities.IsUsable(item, this))
		{
			NotificationText.Push("UI_Item_NotUsable".ToPlainText());
			return;
		}
		StoreHoldWeaponBeforeUse();
		if (item.GetRoot() != characterItem)
		{
			Debug.Log("pick fail");
			item.Detach();
			item.AgentUtilities.ReleaseActiveAgent();
			item.transform.SetParent(base.transform);
		}
		if (interactAction.Running && interactAction.InteractingTarget is InteractableLootbox)
		{
			interactAction.StopAction();
		}
		useItemAction.SetUseItem(item);
		bool flag = StartAction(useItemAction);
		Debug.Log($"UseItemSuccess:{flag}");
		if (flag && IsMainCharacter)
		{
			CharacterMainControl.OnMainCharacterStartUseItem?.Invoke(item);
		}
	}

	public CharacterBuffManager GetBuffManager()
	{
		return buffManager;
	}

	public void AddBuff(Buff buffPrefab, CharacterMainControl fromWho = null, int overrideWeaponID = 0)
	{
		if (!buffPrefab || buffResist.Contains(buffPrefab.ExclusiveTag))
		{
			return;
		}
		Buff.BuffExclusiveTags exclusiveTag = buffPrefab.ExclusiveTag;
		if (exclusiveTag != Buff.BuffExclusiveTags.NotExclusive)
		{
			Buff buffByTag = buffManager.GetBuffByTag(exclusiveTag);
			if (buffByTag != null && buffByTag.ID != buffPrefab.ID)
			{
				if (buffByTag.ExclusiveTagPriority > buffPrefab.ExclusiveTagPriority)
				{
					return;
				}
				if (buffByTag.ExclusiveTagPriority == buffPrefab.ExclusiveTagPriority && buffByTag.LimitedLifeTime && buffPrefab.LimitedLifeTime && buffByTag.CurrentLifeTime > buffPrefab.TotalLifeTime)
				{
					buffByTag.fromWho = fromWho;
					if (overrideWeaponID > 0)
					{
						buffByTag.fromWeaponID = overrideWeaponID;
					}
					return;
				}
				buffManager.RemoveBuff(buffByTag, oneLayer: false);
			}
		}
		buffManager.AddBuff(buffPrefab, fromWho, overrideWeaponID);
	}

	public void RemoveBuff(int buffID, bool removeOneLayer)
	{
		buffManager.RemoveBuff(buffID, removeOneLayer);
	}

	public void RemoveBuffsByTag(Buff.BuffExclusiveTags tag, bool removeOneLayer)
	{
		buffManager.RemoveBuffsByTag(tag, removeOneLayer);
	}

	public bool HasBuff(int buffID)
	{
		return buffManager.HasBuff(buffID);
	}

	public void SetPosition(Vector3 pos)
	{
		movementControl.ForceSetPosition(pos);
		this.OnSetPositionEvent?.Invoke(this, pos);
	}

	public Item GetArmorItem()
	{
		return characterItem.Slots["Armor"]?.Content;
	}

	public Item GetHelmatItem()
	{
		return characterItem.Slots["Helmat"]?.Content;
	}

	public Item GetFaceMaskItem()
	{
		return characterItem.Slots["FaceMask"]?.Content;
	}

	public static float WeaponRepairLossFactor()
	{
		if (!Main || Main.characterItem == null)
		{
			return 1f;
		}
		return Main.characterItem.Constants.GetFloat("WeaponRepairLossFactor", 1f);
	}

	public static float EquipmentRepairLossFactor()
	{
		if (!Main || Main.characterItem == null)
		{
			return 1f;
		}
		return Main.characterItem.Constants.GetFloat("EquipmentRepairLossFactor", 1f);
	}

	private float GetFloatStatValue(int hash)
	{
		if ((bool)characterItem)
		{
			return characterItem.GetStatValue(hash);
		}
		return 0f;
	}

	public void UseStamina(float value)
	{
		if ((bool)LevelManager.Instance && !LevelManager.Instance.IsBaseLevel && !(value <= 0f))
		{
			staminaRecoverTimer = 0f;
			currentStamina -= value;
			if (currentStamina < 0f)
			{
				currentStamina = 0f;
			}
		}
	}
}
