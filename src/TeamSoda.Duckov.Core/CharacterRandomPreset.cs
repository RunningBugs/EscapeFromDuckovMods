using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.Buffs;
using Duckov.Scenes;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Character Random Preset", menuName = "Character Random Preset", order = 51)]
public class CharacterRandomPreset : ScriptableObject
{
	[Serializable]
	private struct SetCharacterStatInfo
	{
		public string statName;

		public Vector2 statBaseValue;
	}

	[LocalizationKey("Characters")]
	public string nameKey;

	public AudioManager.VoiceType voiceType;

	public AudioManager.FootStepMaterialType footstepMaterialType;

	public InteractableLootbox lootBoxPrefab;

	public List<AISpecialAttachmentBase> specialAttachmentBases;

	public Teams team = Teams.scav;

	public bool showName;

	[FormerlySerializedAs("iconType")]
	[SerializeField]
	private CharacterIconTypes characterIconType;

	public float health;

	public bool hasSoul = true;

	public bool showHealthBar = true;

	public int exp = 100;

	[SerializeField]
	private CharacterModel characterModel;

	[SerializeField]
	private bool usePlayerPreset;

	[SerializeField]
	private CustomFacePreset facePreset;

	[SerializeField]
	private AICharacterController aiController;

	public bool setActiveByPlayerDistance = true;

	public float forceTracePlayerDistance;

	public bool shootCanMove;

	public float sightDistance = 17f;

	public float sightAngle = 100f;

	public float reactionTime = 0.2f;

	public float nightReactionTimeFactor = 1.5f;

	public float shootDelay = 0.2f;

	public Vector2 shootTimeRange = new Vector2(0.4f, 1.5f);

	public Vector2 shootTimeSpaceRange = new Vector2(2f, 3f);

	public Vector2 combatMoveTimeRange = new Vector2(1f, 3f);

	public float hearingAbility = 1f;

	public float patrolRange = 8f;

	[FormerlySerializedAs("combatRange")]
	public float combatMoveRange = 8f;

	public bool canDash;

	public Vector2 dashCoolTimeRange = new Vector2(2f, 4f);

	[Range(0f, 1f)]
	public float minTraceTargetChance = 1f;

	[Range(0f, 1f)]
	public float maxTraceTargetChance = 1f;

	public float forgetTime = 8f;

	public bool defaultWeaponOut = true;

	public bool canTalk = true;

	public float patrolTurnSpeed = 180f;

	public float combatTurnSpeed = 1200f;

	[ItemTypeID]
	public int wantItem = -1;

	public float moveSpeedFactor = 1f;

	public float bulletSpeedMultiplier = 1f;

	[Range(1f, 2f)]
	public float gunDistanceMultiplier = 1f;

	public float nightVisionAbility = 0.5f;

	public float gunScatterMultiplier = 1f;

	public float scatterMultiIfTargetRunning = 3f;

	public float scatterMultiIfOffScreen = 4f;

	[FormerlySerializedAs("gunDamageMultiplier")]
	public float damageMultiplier = 1f;

	public float gunCritRateGain;

	[Tooltip("用来决定双方造成伤害缩放")]
	public float aiCombatFactor = 1f;

	public bool hasSkill;

	public SkillBase skillPfb;

	[Range(0.01f, 1f)]
	public float hasSkillChance = 1f;

	public Vector2 skillCoolTimeRange = Vector2.one;

	[Range(0.01f, 1f)]
	public float skillSuccessChance = 1f;

	private float tryReleaseSkillTimeMarker = -1f;

	[Range(0f, 1f)]
	public float itemSkillChance = 0.3f;

	public float itemSkillCoolTime = 6f;

	public List<Buff> buffs;

	public List<Buff.BuffExclusiveTags> buffResist;

	public float elementFactor_Physics = 1f;

	public float elementFactor_Fire = 1f;

	public float elementFactor_Poison = 1f;

	public float elementFactor_Electricity = 1f;

	public float elementFactor_Space = 1f;

	[SerializeField]
	private List<SetCharacterStatInfo> setStats;

	[Range(0f, 1f)]
	public float hasCashChance;

	public Vector2Int cashRange;

	[SerializeField]
	private List<RandomItemGenerateDescription> itemsToGenerate;

	[Space(12f)]
	[SerializeField]
	private RandomContainer<int> bulletQualityDistribution;

	[SerializeField]
	private Tag[] bulletExclusiveTags;

	[HideInInspector]
	[SerializeField]
	private ItemFilter bulletFilter;

	[SerializeField]
	private Vector2 bulletCountRange = Vector2.one;

	public string Name => nameKey.ToPlainText();

	public string DisplayName => nameKey.ToPlainText();

	private int characterItemTypeID => GameplayDataSettings.ItemAssets.DefaultCharacterItemTypeID;

	public Sprite GetCharacterIcon()
	{
		return characterIconType switch
		{
			CharacterIconTypes.none => null, 
			CharacterIconTypes.elete => GameplayDataSettings.UIStyle.EleteCharacterIcon, 
			CharacterIconTypes.pmc => GameplayDataSettings.UIStyle.PmcCharacterIcon, 
			CharacterIconTypes.boss => GameplayDataSettings.UIStyle.BossCharacterIcon, 
			CharacterIconTypes.merchant => GameplayDataSettings.UIStyle.MerchantCharacterIcon, 
			CharacterIconTypes.pet => GameplayDataSettings.UIStyle.PetCharacterIcon, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public async UniTask<CharacterMainControl> CreateCharacterAsync(Vector3 pos, Vector3 dir, int relatedScene, CharacterSpawnerGroup group, bool isLeader)
	{
		Item characterItemInstance = await LevelManager.Instance.CharacterCreator.LoadOrCreateCharacterItemInstance(characterItemTypeID);
		MultiSceneCore.MoveToMainScene(characterItemInstance.gameObject);
		characterItemInstance.GetStat("MaxHealth".GetHashCode()).BaseValue = health * LevelManager.Rule.EnemyHealthFactor;
		characterItemInstance.SetInt("Exp", exp);
		for (int i = 0; i < setStats.Count; i++)
		{
			SetCharacterStatInfo setCharacterStatInfo = setStats[i];
			SetCharacterStat(setCharacterStatInfo.statName, UnityEngine.Random.Range(setCharacterStatInfo.statBaseValue.x, setCharacterStatInfo.statBaseValue.y));
		}
		SetCharacterStat("ElementFactor_Physics", elementFactor_Physics);
		SetCharacterStat("ElementFactor_Fire", elementFactor_Fire);
		SetCharacterStat("ElementFactor_Poison", elementFactor_Poison);
		SetCharacterStat("ElementFactor_Electricity", elementFactor_Electricity);
		SetCharacterStat("ElementFactor_Space", elementFactor_Space);
		SetCharacterStat("GunDistanceMultiplier", gunDistanceMultiplier);
		MultiplyCharacterStat("WalkSpeed", moveSpeedFactor);
		MultiplyCharacterStat("RunSpeed", moveSpeedFactor);
		SetCharacterStat("NightVisionAbility", nightVisionAbility);
		SetCharacterStat("GunScatterMultiplier", gunScatterMultiplier);
		SetCharacterStat("GunDamageMultiplier", damageMultiplier);
		SetCharacterStat("MeleeDamageMultiplier", damageMultiplier);
		SetCharacterStat("GunCritRateGain", gunCritRateGain);
		characterItemInstance.GetStat("BulletSpeedMultiplier".GetHashCode()).BaseValue = bulletSpeedMultiplier;
		List<Item> initialItems = await GenerateItems();
		dir = ((!(dir.magnitude > 0f)) ? Vector3.back : dir.normalized);
		CharacterMainControl character = await LevelManager.Instance.CharacterCreator.CreateCharacter(characterItemInstance, characterModel, pos, Quaternion.LookRotation(dir, Vector3.up));
		if (character == null)
		{
			return null;
		}
		character.characterPreset = this;
		character.SetAimPoint(pos + dir * 10f);
		character.deadLootBoxPrefab = lootBoxPrefab;
		if ((bool)character.characterModel)
		{
			if (usePlayerPreset)
			{
				CustomFaceSettingData faceFromData = LevelManager.Instance.CustomFaceManager.LoadMainCharacterSetting();
				character.characterModel.SetFaceFromData(faceFromData);
			}
			else
			{
				character.characterModel.SetFaceFromPreset(facePreset);
			}
		}
		if (MultiSceneCore.MainScene.HasValue)
		{
			character.SetRelatedScene(relatedScene, setActiveByPlayerDistance);
		}
		character.SetPosition(pos + Vector3.up * 0.5f);
		character.SetTeam(team);
		character.Health.hasSoul = hasSoul;
		character.Health.showHealthBar = showHealthBar;
		AICharacterController ai = null;
		if ((bool)aiController)
		{
			ai = UnityEngine.Object.Instantiate(aiController);
			if (hasSkill && UnityEngine.Random.Range(0f, 1f) < hasSkillChance)
			{
				ai.hasSkill = true;
				ai.skillPfb = skillPfb;
				ai.skillCoolTimeRange = skillCoolTimeRange;
				ai.skillSuccessChance = skillSuccessChance;
			}
			else
			{
				ai.hasSkill = false;
			}
			ai.Init(character, pos, voiceType, footstepMaterialType);
			ai.sightDistance = sightDistance;
			ai.forceTracePlayerDistance = forceTracePlayerDistance;
			ai.sightAngle = sightAngle;
			ai.shootCanMove = shootCanMove;
			ai.shootTimeRange = shootTimeRange * LevelManager.Rule.EnemyAttackTimeFactor;
			ai.shootTimeSpaceRange = shootTimeSpaceRange * LevelManager.Rule.EnemyAttackTimeSpaceFactor;
			ai.baseReactionTime = reactionTime * LevelManager.Rule.EnemyReactionTimeFactor;
			ai.reactionTime = ai.baseReactionTime;
			ai.nightReactionTimeFactor = nightReactionTimeFactor;
			ai.hearingAbility = hearingAbility;
			ai.patrolRange = patrolRange;
			ai.combatMoveRange = combatMoveRange;
			ai.combatMoveTimeRange = combatMoveTimeRange;
			ai.forgetTime = forgetTime;
			ai.traceTargetChance = UnityEngine.Random.Range(minTraceTargetChance, maxTraceTargetChance);
			ai.canDash = canDash;
			ai.canTalk = canTalk;
			ai.patrolTurnSpeed = patrolTurnSpeed;
			ai.combatTurnSpeed = combatTurnSpeed;
			ai.wantItem = wantItem;
			ai.dashCoolTimeRange = dashCoolTimeRange;
			ai.scatterMultiIfTargetRunning = scatterMultiIfTargetRunning;
			ai.scatterMultiIfOffScreen = scatterMultiIfOffScreen;
			ai.shootDelay = shootDelay;
			ai.itemSkillCoolTime = itemSkillCoolTime;
			ai.itemSkillChance = itemSkillChance;
			if ((bool)group)
			{
				group.AddCharacterSpawned(ai, isLeader);
			}
			ai.defaultWeaponOut = defaultWeaponOut;
		}
		foreach (Buff buff in buffs)
		{
			character.AddBuff(buff);
		}
		for (int j = 0; j < buffResist.Count; j++)
		{
			Buff.BuffExclusiveTags buffExclusiveTags = buffResist[j];
			if (buffExclusiveTags != Buff.BuffExclusiveTags.NotExclusive)
			{
				character.buffResist.Add(buffExclusiveTags);
			}
		}
		foreach (AISpecialAttachmentBase specialAttachmentBasis in specialAttachmentBases)
		{
			if (!(specialAttachmentBasis == null))
			{
				AISpecialAttachmentBase aISpecialAttachmentBase = UnityEngine.Object.Instantiate(specialAttachmentBasis, character.transform);
				aISpecialAttachmentBase.transform.localPosition = Vector3.zero;
				aISpecialAttachmentBase.transform.localRotation = Quaternion.identity;
				aISpecialAttachmentBase.Init(ai, character);
			}
		}
		await UniTask.NextFrame();
		character.CharacterItem.Inventory.SetCapacity(15);
		if (initialItems != null)
		{
			foreach (Item item in initialItems)
			{
				if (item == null || characterItemInstance.TryPlug(item))
				{
					continue;
				}
				bool flag = false;
				Item content = character.PrimWeaponSlot().Content;
				if (content != null)
				{
					flag = content.TryPlug(item);
				}
				if (flag)
				{
					continue;
				}
				Item content2 = character.MeleeWeaponSlot().Content;
				if (content2 != null)
				{
					flag = content2.TryPlug(item);
				}
				if (flag)
				{
					continue;
				}
				if (characterItemInstance.Inventory.AddAndMerge(item))
				{
					if ((bool)item)
					{
						ItemSetting_Skill component = item.GetComponent<ItemSetting_Skill>();
						if ((bool)component)
						{
							ai.AddItemSkill(component);
						}
						else
						{
							ai.CheckAndAddDrugItem(item);
						}
					}
				}
				else
				{
					item.DestroyTree();
				}
			}
		}
		await AddBullet(character);
		return character;
		void MultiplyCharacterStat(string statName, float multiplier)
		{
			Stat stat = characterItemInstance.GetStat(statName.GetHashCode());
			if (stat != null)
			{
				stat.BaseValue *= multiplier;
			}
		}
		void SetCharacterStat(string statName, float value)
		{
			Stat stat = characterItemInstance.GetStat(statName.GetHashCode());
			if (stat != null)
			{
				stat.BaseValue = value;
			}
		}
	}

	private async UniTask<List<Item>> GenerateItems()
	{
		List<Item> items = new List<Item>();
		foreach (RandomItemGenerateDescription item2 in itemsToGenerate)
		{
			items.AddRange(await item2.Generate());
		}
		if (UnityEngine.Random.Range(0f, 1f) < hasCashChance)
		{
			Item item = await ItemAssetsCollection.InstantiateAsync(GameplayDataSettings.ItemAssets.CashItemTypeID);
			item.StackCount = UnityEngine.Random.Range(cashRange.x, cashRange.y);
			items.Add(item);
		}
		return items;
	}

	private async UniTask AddBullet(CharacterMainControl character)
	{
		Item item = character.PrimWeaponSlot()?.Content;
		if (!(item != null))
		{
			return;
		}
		string text = item.Constants.GetString("Caliber");
		if (!string.IsNullOrEmpty(text))
		{
			int random = bulletQualityDistribution.GetRandom();
			bulletFilter.caliber = text;
			bulletFilter.minQuality = random;
			bulletFilter.maxQuality = random;
			bulletFilter.excludeTags = bulletExclusiveTags;
			int[] array = ItemAssetsCollection.Search(bulletFilter);
			if (array.Length >= 1)
			{
				Item item2 = await ItemAssetsCollection.InstantiateAsync(array.GetRandom());
				item2.StackCount = Mathf.RoundToInt((float)item2.StackCount * UnityEngine.Random.Range(bulletCountRange.x, bulletCountRange.y));
				character?.CharacterItem?.Inventory?.AddItem(item2);
			}
		}
	}
}
