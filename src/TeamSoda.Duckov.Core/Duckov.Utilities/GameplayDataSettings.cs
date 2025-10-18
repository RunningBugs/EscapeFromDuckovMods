using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Duckov.Achievements;
using Duckov.Buffs;
using Duckov.Buildings;
using Duckov.Crops;
using Duckov.Quests;
using Duckov.Quests.Relations;
using Duckov.UI;
using Eflatun.SceneReference;
using ItemStatsSystem;
using LeTai.TrueShadow;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Duckov.Utilities;

[CreateAssetMenu(menuName = "Settings/Gameplay Data Settings")]
public class GameplayDataSettings : ScriptableObject
{
	[Serializable]
	public class LootingData
	{
		public float[] inspectingTimes;

		public float MGetInspectingTime(Item item)
		{
			int num = item.Quality;
			if (num < 0)
			{
				num = 0;
			}
			if (num >= inspectingTimes.Length)
			{
				num = inspectingTimes.Length - 1;
			}
			return inspectingTimes[num];
		}

		public static float GetInspectingTime(Item item)
		{
			return Looting?.MGetInspectingTime(item) ?? 1f;
		}
	}

	[Serializable]
	public class TagsData
	{
		[SerializeField]
		private Tag character;

		[SerializeField]
		private Tag lockInDemoTag;

		[SerializeField]
		private Tag helmat;

		[SerializeField]
		private Tag armor;

		[SerializeField]
		private Tag backpack;

		[SerializeField]
		private Tag bullet;

		[SerializeField]
		private Tag bait;

		[SerializeField]
		private Tag advancedDebuffMode;

		[SerializeField]
		private Tag special;

		[SerializeField]
		private Tag destroyOnLootBox;

		[FormerlySerializedAs("dontDropOnDead")]
		[SerializeField]
		private Tag dontDropOnDeadInSlot;

		[SerializeField]
		private List<Tag> allTags = new List<Tag>();

		private ReadOnlyCollection<Tag> tagsReadOnly;

		private Tag gun;

		public Tag Character => character;

		public Tag LockInDemoTag => lockInDemoTag;

		public Tag Helmat => helmat;

		public Tag Armor => armor;

		public Tag Backpack => backpack;

		public Tag Bullet => bullet;

		public Tag Bait => bait;

		public Tag AdvancedDebuffMode => advancedDebuffMode;

		public Tag Special => special;

		public Tag DestroyOnLootBox => destroyOnLootBox;

		public Tag DontDropOnDeadInSlot => dontDropOnDeadInSlot;

		public ReadOnlyCollection<Tag> AllTags
		{
			get
			{
				if (tagsReadOnly == null)
				{
					tagsReadOnly = allTags.AsReadOnly();
				}
				return tagsReadOnly;
			}
		}

		public Tag Gun
		{
			get
			{
				if (gun == null)
				{
					gun = Get("Gun");
				}
				return gun;
			}
		}

		internal Tag Get(string name)
		{
			foreach (Tag allTag in AllTags)
			{
				if (allTag.name == name)
				{
					return allTag;
				}
			}
			return null;
		}
	}

	[Serializable]
	public class PrefabsData
	{
		[SerializeField]
		private LevelManager levelManagerPrefab;

		[SerializeField]
		private CharacterMainControl characterPrefab;

		[SerializeField]
		private GameObject bulletHitObsticleFx;

		[SerializeField]
		private GameObject questMarker;

		[SerializeField]
		private DuckovItemAgent pickupAgentPrefab;

		[SerializeField]
		private DuckovItemAgent pickupAgentNoRendererPrefab;

		[SerializeField]
		private DuckovItemAgent handheldAgentPrefab;

		[SerializeField]
		private InteractableLootbox lootBoxPrefab;

		[SerializeField]
		private InteractableLootbox lootBoxPrefab_Tomb;

		[SerializeField]
		private InteractMarker interactMarker;

		[SerializeField]
		private HeadCollider headCollider;

		[SerializeField]
		private Projectile defaultBullet;

		[SerializeField]
		private UIInputManager uiInputManagerPrefab;

		[SerializeField]
		private GameObject buildingBlockAreaMesh;

		[SerializeField]
		private GameObject alertFxPrefab;

		public LevelManager LevelManagerPrefab => levelManagerPrefab;

		public CharacterMainControl CharacterPrefab => characterPrefab;

		public GameObject BulletHitObsticleFx => bulletHitObsticleFx;

		public GameObject QuestMarker => questMarker;

		public DuckovItemAgent PickupAgentPrefab => pickupAgentPrefab;

		public DuckovItemAgent PickupAgentNoRendererPrefab => pickupAgentNoRendererPrefab;

		public DuckovItemAgent HandheldAgentPrefab => handheldAgentPrefab;

		public InteractableLootbox LootBoxPrefab => lootBoxPrefab;

		public InteractableLootbox LootBoxPrefab_Tomb => lootBoxPrefab_Tomb;

		public InteractMarker InteractMarker => interactMarker;

		public HeadCollider HeadCollider => headCollider;

		public Projectile DefaultBullet => defaultBullet;

		public GameObject BuildingBlockAreaMesh => buildingBlockAreaMesh;

		public GameObject AlertFxPrefab => alertFxPrefab;

		public UIInputManager UIInputManagerPrefab => uiInputManagerPrefab;
	}

	[Serializable]
	public class BuffsData
	{
		[SerializeField]
		private Buff bleedSBuff;

		[SerializeField]
		private Buff unlimitBleedBuff;

		[SerializeField]
		private Buff boneCrackBuff;

		[SerializeField]
		private Buff woundBuff;

		[SerializeField]
		private Buff weight_Light;

		[SerializeField]
		private Buff weight_Heavy;

		[SerializeField]
		private Buff weight_SuperHeavy;

		[SerializeField]
		private Buff weight_Overweight;

		[SerializeField]
		private Buff pain;

		[SerializeField]
		private Buff baseBuff;

		[SerializeField]
		private Buff starve;

		[SerializeField]
		private Buff thirsty;

		[SerializeField]
		private Buff burn;

		[SerializeField]
		private Buff poison;

		[SerializeField]
		private Buff electric;

		[SerializeField]
		private Buff space;

		[SerializeField]
		private List<Buff> allBuffs;

		public Buff BleedSBuff => bleedSBuff;

		public Buff UnlimitBleedBuff => unlimitBleedBuff;

		public Buff BoneCrackBuff => boneCrackBuff;

		public Buff WoundBuff => woundBuff;

		public Buff Weight_Light => weight_Light;

		public Buff Weight_Heavy => weight_Heavy;

		public Buff Weight_SuperHeavy => weight_SuperHeavy;

		public Buff Weight_Overweight => weight_Overweight;

		public Buff Pain => pain;

		public Buff BaseBuff => baseBuff;

		public Buff Starve => starve;

		public Buff Thirsty => thirsty;

		public Buff Burn => burn;

		public Buff Poison => poison;

		public Buff Electric => electric;

		public Buff Space => space;

		public string GetBuffDisplayName(int id)
		{
			Buff buff = allBuffs.Find((Buff e) => e != null && e.ID == id);
			if (buff == null)
			{
				return "?";
			}
			return buff.DisplayName;
		}
	}

	[Serializable]
	public class ItemAssetsData
	{
		[SerializeField]
		[ItemTypeID]
		private int defaultCharacterItemTypeID;

		[SerializeField]
		[ItemTypeID]
		private int cashItemTypeID;

		public int DefaultCharacterItemTypeID => defaultCharacterItemTypeID;

		public int CashItemTypeID => cashItemTypeID;
	}

	public class StringListsData
	{
		public static StringList StatKeys;

		public static StringList SlotTypes;

		public static StringList ItemAgentKeys;
	}

	[Serializable]
	public class LayersData
	{
		public LayerMask damageReceiverLayerMask;

		public LayerMask wallLayerMask;

		public LayerMask groundLayerMask;

		public LayerMask halfObsticleLayer;

		public LayerMask fowBlockLayers;

		public LayerMask fowBlockLayersWithThermal;

		public static bool IsLayerInLayerMask(int layer, LayerMask layerMask)
		{
			if (((1 << layer) & (int)layerMask) != 0)
			{
				return true;
			}
			return false;
		}
	}

	[Serializable]
	public class SceneManagementData
	{
		[SerializeField]
		private SceneInfoCollection sceneInfoCollection;

		[SerializeField]
		private SceneReference prologueScene;

		[SerializeField]
		private SceneReference mainMenuScene;

		[SerializeField]
		private SceneReference baseScene;

		[SerializeField]
		private SceneReference failLoadingScreenScene;

		[SerializeField]
		private SceneReference evacuateScreenScene;

		public SceneInfoCollection SceneInfoCollection => sceneInfoCollection;

		public SceneReference PrologueScene => prologueScene;

		public SceneReference MainMenuScene => mainMenuScene;

		public SceneReference BaseScene => baseScene;

		public SceneReference FailLoadingScreenScene => failLoadingScreenScene;

		public SceneReference EvacuateScreenScene => evacuateScreenScene;
	}

	[Serializable]
	public class QuestsData
	{
		[Serializable]
		public class QuestGiverInfo
		{
			public QuestGiverID id;

			[SerializeField]
			private string displayName;

			public string DisplayName => displayName;
		}

		[SerializeField]
		private QuestCollection questCollection;

		[SerializeField]
		private QuestRelationGraph questRelation;

		[SerializeField]
		private List<QuestGiverInfo> questGiverInfos;

		[SerializeField]
		private string defaultQuestGiverDisplayName = "佚名";

		private string DefaultQuestGiverDisplayName => defaultQuestGiverDisplayName;

		public QuestCollection QuestCollection => questCollection;

		public QuestRelationGraph QuestRelation => questRelation;

		public QuestGiverInfo GetInfo(QuestGiverID id)
		{
			return questGiverInfos.Find((QuestGiverInfo e) => e != null && e.id == id);
		}

		public string GetDisplayName(QuestGiverID id)
		{
			return $"Character_{id}".ToPlainText();
		}
	}

	[Serializable]
	public class EconomyData
	{
		[SerializeField]
		[ItemTypeID]
		private List<int> unlockItemByDefault = new List<int>();

		public ReadOnlyCollection<int> UnlockedItemByDefault => unlockItemByDefault.AsReadOnly();
	}

	[Serializable]
	public class UIStyleData
	{
		[Serializable]
		public class DisplayQualityLook
		{
			public DisplayQuality quality;

			public float shadowOffset;

			public Color shadowColor;

			public bool innerGlow;

			public void Apply(TrueShadow trueShadow)
			{
				trueShadow.OffsetDistance = shadowOffset;
				trueShadow.Color = shadowColor;
				trueShadow.Inset = innerGlow;
			}
		}

		[Serializable]
		public class DisplayElementDamagePopTextLook
		{
			public ElementTypes elementType;

			public float normalSize;

			public float critSize;

			public Color color;
		}

		[SerializeField]
		private List<DisplayQualityLook> displayQualityLooks = new List<DisplayQualityLook>();

		[SerializeField]
		private List<DisplayElementDamagePopTextLook> elementDamagePopTextLook = new List<DisplayElementDamagePopTextLook>();

		[SerializeField]
		private float defaultDisplayQualityShadowOffset = 8f;

		[SerializeField]
		private Color defaultDisplayQualityShadowColor = Color.black;

		[SerializeField]
		private bool defaultDIsplayQualityShadowInnerGlow;

		[SerializeField]
		private Sprite defaultTeleporterIcon;

		[SerializeField]
		private float teleporterIconScale = 0.5f;

		[SerializeField]
		private Sprite critPopSprite;

		[SerializeField]
		private Sprite fallbackItemIcon;

		[SerializeField]
		private Sprite eleteCharacterIcon;

		[SerializeField]
		private Sprite bossCharacterIcon;

		[SerializeField]
		private Sprite pmcCharacterIcon;

		[SerializeField]
		private Sprite merchantCharacterIcon;

		[SerializeField]
		private Sprite petCharacterIcon;

		[SerializeField]
		private TMP_Asset defaultFont;

		[SerializeField]
		private TextMeshProUGUI templateTextUGUI;

		public Sprite CritPopSprite => critPopSprite;

		public Sprite DefaultTeleporterIcon => defaultTeleporterIcon;

		public Sprite EleteCharacterIcon => eleteCharacterIcon;

		public Sprite BossCharacterIcon => bossCharacterIcon;

		public Sprite PmcCharacterIcon => pmcCharacterIcon;

		public Sprite MerchantCharacterIcon => merchantCharacterIcon;

		public Sprite PetCharacterIcon => petCharacterIcon;

		public float TeleporterIconScale => teleporterIconScale;

		public Sprite FallbackItemIcon => fallbackItemIcon;

		public TextMeshProUGUI TemplateTextUGUI => templateTextUGUI;

		[SerializeField]
		private TMP_Asset DefaultFont => defaultFont;

		public (float shadowOffset, Color color, bool innerGlow) GetShadowOffsetAndColorOfQuality(DisplayQuality displayQuality)
		{
			DisplayQualityLook displayQualityLook = displayQualityLooks.Find((DisplayQualityLook e) => e != null && e.quality == displayQuality);
			if (displayQualityLook == null)
			{
				return (shadowOffset: defaultDisplayQualityShadowOffset, color: defaultDisplayQualityShadowColor, innerGlow: defaultDIsplayQualityShadowInnerGlow);
			}
			return (shadowOffset: displayQualityLook.shadowOffset, color: displayQualityLook.shadowColor, innerGlow: displayQualityLook.innerGlow);
		}

		public void ApplyDisplayQualityShadow(DisplayQuality displayQuality, TrueShadow target)
		{
			(target.OffsetDistance, target.Color, target.Inset) = GetShadowOffsetAndColorOfQuality(displayQuality);
		}

		public DisplayQualityLook GetDisplayQualityLook(DisplayQuality q)
		{
			DisplayQualityLook displayQualityLook = displayQualityLooks.Find((DisplayQualityLook e) => e != null && e.quality == q);
			if (displayQualityLook == null)
			{
				return new DisplayQualityLook
				{
					quality = q,
					shadowOffset = defaultDisplayQualityShadowOffset,
					shadowColor = defaultDisplayQualityShadowColor,
					innerGlow = defaultDIsplayQualityShadowInnerGlow
				};
			}
			return displayQualityLook;
		}

		public DisplayElementDamagePopTextLook GetElementDamagePopTextLook(ElementTypes elementType)
		{
			DisplayElementDamagePopTextLook displayElementDamagePopTextLook = elementDamagePopTextLook.Find((DisplayElementDamagePopTextLook e) => e != null && e.elementType == elementType);
			if (displayElementDamagePopTextLook == null)
			{
				return new DisplayElementDamagePopTextLook
				{
					elementType = ElementTypes.physics,
					normalSize = 1f,
					critSize = 1.6f,
					color = Color.white
				};
			}
			return displayElementDamagePopTextLook;
		}
	}

	[Serializable]
	public class SpritesData
	{
		[Serializable]
		public struct Entry
		{
			public string key;

			public Sprite sprite;
		}

		public List<Entry> entries;

		public Sprite GetSprite(string key)
		{
			foreach (Entry entry in entries)
			{
				if (entry.key == key)
				{
					return entry.sprite;
				}
			}
			return null;
		}
	}

	private static GameplayDataSettings cachedDefault;

	[SerializeField]
	private TagsData tags;

	[SerializeField]
	private PrefabsData prefabs;

	[SerializeField]
	private UIPrefabsReference uiPrefabs;

	[SerializeField]
	private ItemAssetsData itemAssets;

	[SerializeField]
	private StringListsData stringLists;

	[SerializeField]
	private LayersData layers;

	[SerializeField]
	private SceneManagementData sceneManagement;

	[SerializeField]
	private BuffsData buffs;

	[SerializeField]
	private QuestsData quests;

	[SerializeField]
	private EconomyData economyData;

	[SerializeField]
	private UIStyleData uiStyleData;

	[SerializeField]
	private InputActionAsset inputActions;

	[SerializeField]
	private BuildingDataCollection buildingDataCollection;

	[SerializeField]
	private CustomFaceData customFaceData;

	[SerializeField]
	private CraftingFormulaCollection craftingFormulas;

	[SerializeField]
	private DecomposeDatabase decomposeDatabase;

	[SerializeField]
	private StatInfoDatabase statInfo;

	[SerializeField]
	private StockShopDatabase stockShopDatabase;

	[SerializeField]
	private LootingData looting;

	[SerializeField]
	private AchievementDatabase achivementDatabase;

	[SerializeField]
	private CropDatabase cropDatabase;

	[SerializeField]
	private SpritesData spriteData;

	private static GameplayDataSettings Default
	{
		get
		{
			if (cachedDefault == null)
			{
				cachedDefault = Resources.Load<GameplayDataSettings>("GameplayDataSettings");
			}
			return cachedDefault;
		}
	}

	public static InputActionAsset InputActions => Default.inputActions;

	public static CustomFaceData CustomFaceData => Default.customFaceData;

	public static TagsData Tags => Default.tags;

	public static PrefabsData Prefabs => Default.prefabs;

	public static UIPrefabsReference UIPrefabs => Default.uiPrefabs;

	public static ItemAssetsData ItemAssets => Default.itemAssets;

	public static StringListsData StringLists => Default.stringLists;

	public static LayersData Layers => Default.layers;

	public static SceneManagementData SceneManagement => Default.sceneManagement;

	public static BuffsData Buffs => Default.buffs;

	public static QuestsData Quests => Default.quests;

	public static QuestCollection QuestCollection => Default.quests.QuestCollection;

	public static QuestRelationGraph QuestRelation => Default.quests.QuestRelation;

	public static EconomyData Economy => Default.economyData;

	public static UIStyleData UIStyle => Default.uiStyleData;

	public static BuildingDataCollection BuildingDataCollection => Default.buildingDataCollection;

	public static CraftingFormulaCollection CraftingFormulas => Default.craftingFormulas;

	public static DecomposeDatabase DecomposeDatabase => Default.decomposeDatabase;

	public static StatInfoDatabase StatInfo => Default.statInfo;

	public static StockShopDatabase StockshopDatabase => Default.stockShopDatabase;

	public static LootingData Looting => Default.looting;

	public static AchievementDatabase AchievementDatabase => Default.achivementDatabase;

	public static CropDatabase CropDatabase => Default.cropDatabase;

	internal static Sprite GetSprite(string key)
	{
		return Default.spriteData.GetSprite(key);
	}
}
