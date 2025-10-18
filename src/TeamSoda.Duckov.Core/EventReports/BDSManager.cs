using System;
using System.Globalization;
using Bilibili.BDS;
using Duckov;
using Duckov.Buffs;
using Duckov.Buildings;
using Duckov.Economy;
using Duckov.MasterKeys;
using Duckov.PerkTrees;
using Duckov.Quests;
using Duckov.Rules;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using Saves;
using Steamworks;
using UnityEngine;

namespace EventReports;

public class BDSManager : MonoBehaviour
{
	private enum EventName
	{
		none,
		app_start,
		begin_new_game,
		delete_save_data,
		raid_new,
		raid_end,
		scene_load_start,
		scene_load_finish,
		level_initialized,
		level_evacuated,
		main_character_dead,
		quest_activate,
		quest_complete,
		pay_money,
		pay_cost,
		item_to_inventory,
		item_to_storage,
		shop_purchased,
		craft_craft,
		craft_formula_unlock,
		enemy_kill,
		role_level_changed,
		building_built,
		building_destroyed,
		perk_unlocked,
		masterkey_unlocked,
		role_equip,
		item_sold,
		reward_claimed,
		item_use,
		interact_start,
		face_customize_begin,
		face_customize_finish,
		heartbeat,
		cheat_mode_changed,
		app_end
	}

	private struct CheatModeStatusChangeContext
	{
		public bool cheatModeActive;
	}

	private struct InteractEventContext
	{
		public string interactGameObjectName;

		public string typeName;
	}

	private struct ItemUseEventContext
	{
		public int itemTypeID;
	}

	private struct RewardClaimEventContext
	{
		public int questID;

		public int rewardID;
	}

	private struct ItemSoldEventContext
	{
		public string stockShopID;

		public int itemID;

		public int price;
	}

	private struct EquipEventContext
	{
		public string slotKey;

		public int contentItemTypeID;
	}

	private struct MasterKeyUnlockContext
	{
		public int keyID;
	}

	private struct PerkInfo
	{
		public string perkTreeID;

		public string perkName;
	}

	private struct BuildingEventContext
	{
		public string buildingID;
	}

	private struct LevelChangedEventContext
	{
		public int from;

		public int to;

		public LevelChangedEventContext(int from, int to)
		{
			this.from = from;
			this.to = to;
		}
	}

	private struct EnemyKillInfo
	{
		public string enemyPresetName;

		public DamageInfo damageInfo;
	}

	[Serializable]
	public struct PurchaseInfo
	{
		public string shopID;

		public int itemTypeID;

		public int itemAmount;
	}

	private struct ItemInfo
	{
		public int itemId;

		public int amount;
	}

	public struct CharacterDeathContext
	{
		public DamageInfo damageInfo;

		public string fromCharacterPresetName;

		public string fromCharacterNameKey;

		public LevelManager.LevelInfo levelInfo;
	}

	[Serializable]
	private struct PlayerStatus
	{
		public bool valid;

		public float healthMax;

		public float health;

		public float waterMax;

		public float foodMax;

		public float water;

		public float food;

		public string[] activeEffects;

		public int totalItemValue;

		public static PlayerStatus CreateFromCurrent()
		{
			CharacterMainControl main = CharacterMainControl.Main;
			if (main == null)
			{
				return default(PlayerStatus);
			}
			Health health = main.Health;
			if (health == null)
			{
				return default(PlayerStatus);
			}
			CharacterBuffManager buffManager = main.GetBuffManager();
			if (buffManager == null)
			{
				return default(PlayerStatus);
			}
			if (main.CharacterItem == null)
			{
				return default(PlayerStatus);
			}
			string[] array = new string[buffManager.Buffs.Count];
			for (int i = 0; i < buffManager.Buffs.Count; i++)
			{
				Buff buff = buffManager.Buffs[i];
				if (!(buff == null))
				{
					array[i] = $"{buff.ID} {buff.DisplayNameKey}";
				}
			}
			int totalRawValue = main.CharacterItem.GetTotalRawValue();
			return new PlayerStatus
			{
				valid = true,
				healthMax = health.MaxHealth,
				health = main.CurrentEnergy,
				water = main.CurrentWater,
				food = main.CurrentEnergy,
				waterMax = main.MaxWater,
				foodMax = main.MaxEnergy,
				totalItemValue = totalRawValue
			};
		}
	}

	private struct EvacuationEventData
	{
		public EvacuationInfo evacuationInfo;

		public string mapID;

		public RaidUtilities.RaidInfo raidInfo;

		public PlayerStatus playerStatus;
	}

	[Serializable]
	private struct SessionInfo
	{
		public int startCount;

		public bool isFirstTimeStart;

		public int session_id;

		public int session_duration_seconds;
	}

	public struct PlayerInfo
	{
		public string role_name;

		public string profession_type;

		public string gender;

		public string level;

		public string b_account_id;

		public string b_role_id;

		public string b_tour_indicator;

		public string b_zone_id;

		public string b_sdk_uid;

		public PlayerInfo(int level, string steamAccountID, int saveSlot, string location, string language, string displayName, string difficulty, string platform, string version, string system)
		{
			role_name = displayName;
			profession_type = language;
			gender = version;
			this.level = $"{level}";
			b_account_id = steamAccountID;
			b_role_id = $"{saveSlot}|{difficulty}";
			b_tour_indicator = "0";
			b_zone_id = location;
			b_sdk_uid = platform + "|" + system;
		}

		public static PlayerInfo GetCurrent()
		{
			string iD = PlatformInfo.GetID();
			string displayName = PlatformInfo.GetDisplayName();
			PlayerInfo result = new PlayerInfo(EXPManager.Level, iD, SavesSystem.CurrentSlot, RegionInfo.CurrentRegion.Name, Application.systemLanguage.ToString(), displayName, GameRulesManager.Current.displayNameKey, PlatformInfo.Platform.ToString(), GameMetaData.Instance.Version.ToString(), Environment.OSVersion.Platform.ToString());
			result.gender = GameMetaData.Instance.Version.ToString();
			return result;
		}

		public static string GetCurrentJson()
		{
			return GetCurrent().ToJson();
		}

		public string ToJson()
		{
			return JsonUtility.ToJson(this);
		}
	}

	private float lastTimeHeartbeat;

	private int sessionID;

	private DateTime sessionStartTime;

	private SessionInfo sessionInfo;

	public static Action<string, string> OnReportCustomEvent;

	private float TimeSinceLastHeartbeat => Time.unscaledTime - lastTimeHeartbeat;

	private void Awake()
	{
		if (PlatformInfo.Platform == Platform.Steam)
		{
			if (SteamManager.Initialized && !SteamUtils.IsSteamChinaLauncher())
			{
			}
		}
		else
		{
			_ = $"{PlatformInfo.Platform}";
		}
		Debug.Log("Player Info:\n" + PlayerInfo.GetCurrent().ToJson());
	}

	private void Start()
	{
		OnGameStarted();
	}

	private void OnDestroy()
	{
	}

	private void Update()
	{
		_ = Application.isPlaying;
	}

	private void UpdateHeartbeat()
	{
		if (TimeSinceLastHeartbeat > 60f)
		{
			ReportCustomEvent(EventName.heartbeat);
			lastTimeHeartbeat = Time.unscaledTime;
		}
	}

	private void RegisterEvents()
	{
		UnregisterEvents();
		SavesSystem.OnSaveDeleted += OnSaveDeleted;
		RaidUtilities.OnNewRaid = (Action<RaidUtilities.RaidInfo>)Delegate.Combine(RaidUtilities.OnNewRaid, new Action<RaidUtilities.RaidInfo>(OnNewRaid));
		RaidUtilities.OnRaidEnd = (Action<RaidUtilities.RaidInfo>)Delegate.Combine(RaidUtilities.OnRaidEnd, new Action<RaidUtilities.RaidInfo>(OnRaidEnd));
		SceneLoader.onStartedLoadingScene += OnSceneLoadingStart;
		SceneLoader.onFinishedLoadingScene += OnSceneLoadingFinish;
		LevelManager.OnLevelInitialized += OnLevelInitialized;
		LevelManager.OnEvacuated += OnEvacuated;
		LevelManager.OnMainCharacterDead += OnMainCharacterDead;
		Quest.onQuestActivated += OnQuestActivated;
		Quest.onQuestCompleted += OnQuestCompleted;
		EconomyManager.OnCostPaid += OnCostPaid;
		EconomyManager.OnMoneyPaid += OnMoneyPaid;
		ItemUtilities.OnItemSentToPlayerInventory += OnItemSentToPlayerInventory;
		ItemUtilities.OnItemSentToPlayerStorage += OnItemSentToPlayerStorage;
		StockShop.OnItemPurchased += OnItemPurchased;
		CraftingManager.OnItemCrafted = (Action<CraftingFormula, Item>)Delegate.Combine(CraftingManager.OnItemCrafted, new Action<CraftingFormula, Item>(OnItemCrafted));
		CraftingManager.OnFormulaUnlocked = (Action<string>)Delegate.Combine(CraftingManager.OnFormulaUnlocked, new Action<string>(OnFormulaUnlocked));
		Health.OnDead += OnHealthDead;
		EXPManager.onLevelChanged = (Action<int, int>)Delegate.Combine(EXPManager.onLevelChanged, new Action<int, int>(OnLevelChanged));
		BuildingManager.OnBuildingBuiltComplex += OnBuildingBuilt;
		BuildingManager.OnBuildingDestroyedComplex += OnBuildingDestroyed;
		Perk.OnPerkUnlockConfirmed += OnPerkUnlockConfirmed;
		MasterKeysManager.OnMasterKeyUnlocked += OnMasterKeyUnlocked;
		CharacterMainControl.OnMainCharacterSlotContentChangedEvent = (Action<CharacterMainControl, Slot>)Delegate.Combine(CharacterMainControl.OnMainCharacterSlotContentChangedEvent, new Action<CharacterMainControl, Slot>(OnMainCharacterSlotContentChanged));
		StockShop.OnItemSoldByPlayer += OnItemSold;
		Reward.OnRewardClaimed += OnRewardClaimed;
		UsageUtilities.OnItemUsedStaticEvent += OnItemUsed;
		InteractableBase.OnInteractStartStaticEvent += OnInteractStart;
		LevelManager.OnNewGameReport += OnNewGameReport;
		Interact_CustomFace.OnCustomFaceStartEvent += OnCustomFaceStart;
		Interact_CustomFace.OnCustomFaceFinishedEvent += OnCustomFaceFinish;
		CheatMode.OnCheatModeStatusChanged += OnCheatModeStatusChanged;
	}

	private void UnregisterEvents()
	{
		SavesSystem.OnSaveDeleted -= OnSaveDeleted;
		RaidUtilities.OnNewRaid = (Action<RaidUtilities.RaidInfo>)Delegate.Remove(RaidUtilities.OnNewRaid, new Action<RaidUtilities.RaidInfo>(OnNewRaid));
		RaidUtilities.OnRaidEnd = (Action<RaidUtilities.RaidInfo>)Delegate.Remove(RaidUtilities.OnRaidEnd, new Action<RaidUtilities.RaidInfo>(OnRaidEnd));
		SceneLoader.onStartedLoadingScene -= OnSceneLoadingStart;
		SceneLoader.onFinishedLoadingScene -= OnSceneLoadingFinish;
		LevelManager.OnLevelInitialized -= OnLevelInitialized;
		LevelManager.OnEvacuated -= OnEvacuated;
		LevelManager.OnMainCharacterDead -= OnMainCharacterDead;
		Quest.onQuestActivated -= OnQuestActivated;
		Quest.onQuestCompleted -= OnQuestCompleted;
		EconomyManager.OnCostPaid -= OnCostPaid;
		EconomyManager.OnMoneyPaid -= OnMoneyPaid;
		ItemUtilities.OnItemSentToPlayerInventory -= OnItemSentToPlayerInventory;
		ItemUtilities.OnItemSentToPlayerStorage -= OnItemSentToPlayerStorage;
		StockShop.OnItemPurchased -= OnItemPurchased;
		CraftingManager.OnItemCrafted = (Action<CraftingFormula, Item>)Delegate.Remove(CraftingManager.OnItemCrafted, new Action<CraftingFormula, Item>(OnItemCrafted));
		CraftingManager.OnFormulaUnlocked = (Action<string>)Delegate.Remove(CraftingManager.OnFormulaUnlocked, new Action<string>(OnFormulaUnlocked));
		Health.OnDead -= OnHealthDead;
		EXPManager.onLevelChanged = (Action<int, int>)Delegate.Remove(EXPManager.onLevelChanged, new Action<int, int>(OnLevelChanged));
		BuildingManager.OnBuildingBuiltComplex -= OnBuildingBuilt;
		BuildingManager.OnBuildingDestroyedComplex -= OnBuildingDestroyed;
		Perk.OnPerkUnlockConfirmed -= OnPerkUnlockConfirmed;
		MasterKeysManager.OnMasterKeyUnlocked -= OnMasterKeyUnlocked;
		CharacterMainControl.OnMainCharacterSlotContentChangedEvent = (Action<CharacterMainControl, Slot>)Delegate.Remove(CharacterMainControl.OnMainCharacterSlotContentChangedEvent, new Action<CharacterMainControl, Slot>(OnMainCharacterSlotContentChanged));
		StockShop.OnItemSoldByPlayer -= OnItemSold;
		Reward.OnRewardClaimed -= OnRewardClaimed;
		UsageUtilities.OnItemUsedStaticEvent -= OnItemUsed;
		InteractableBase.OnInteractStartStaticEvent -= OnInteractStart;
		LevelManager.OnNewGameReport -= OnNewGameReport;
		Interact_CustomFace.OnCustomFaceStartEvent -= OnCustomFaceStart;
		Interact_CustomFace.OnCustomFaceFinishedEvent -= OnCustomFaceFinish;
		CheatMode.OnCheatModeStatusChanged -= OnCheatModeStatusChanged;
	}

	private void OnCheatModeStatusChanged(bool value)
	{
		ReportCustomEvent(EventName.cheat_mode_changed, new CheatModeStatusChangeContext
		{
			cheatModeActive = value
		});
	}

	private void OnCustomFaceFinish()
	{
		ReportCustomEvent(EventName.face_customize_finish);
	}

	private void OnCustomFaceStart()
	{
		ReportCustomEvent(EventName.face_customize_begin);
	}

	private void OnNewGameReport()
	{
		ReportCustomEvent(EventName.begin_new_game);
	}

	private void OnInteractStart(InteractableBase target)
	{
		if (!(target == null))
		{
			ReportCustomEvent(EventName.interact_start, new InteractEventContext
			{
				interactGameObjectName = target.name,
				typeName = target.GetType().Name
			});
		}
	}

	private void OnItemUsed(Item item)
	{
		ReportCustomEvent(EventName.item_use, new ItemUseEventContext
		{
			itemTypeID = item.TypeID
		});
	}

	private void OnRewardClaimed(Reward reward)
	{
		int questID = ((reward.Master != null) ? reward.Master.ID : (-1));
		ReportCustomEvent(EventName.reward_claimed, new RewardClaimEventContext
		{
			questID = questID,
			rewardID = reward.ID
		});
	}

	private void OnItemSold(StockShop shop, Item item, int price)
	{
		if (!(item == null))
		{
			string stockShopID = shop?.MerchantID;
			ReportCustomEvent(EventName.item_sold, new ItemSoldEventContext
			{
				stockShopID = stockShopID,
				itemID = item.TypeID,
				price = price
			});
		}
	}

	private void OnMainCharacterSlotContentChanged(CharacterMainControl control, Slot slot)
	{
		if (!(control == null) && slot != null && !(slot.Content == null))
		{
			ReportCustomEvent(EventName.role_equip, new EquipEventContext
			{
				slotKey = slot.Key,
				contentItemTypeID = slot.Content.TypeID
			});
		}
	}

	private void OnMasterKeyUnlocked(int id)
	{
		ReportCustomEvent(EventName.masterkey_unlocked, new MasterKeyUnlockContext
		{
			keyID = id
		});
	}

	private void OnPerkUnlockConfirmed(Perk perk)
	{
		if (!(perk == null))
		{
			ReportCustomEvent(EventName.perk_unlocked, new PerkInfo
			{
				perkTreeID = perk.Master?.ID,
				perkName = perk.name
			});
		}
	}

	private void OnBuildingBuilt(int guid, BuildingInfo info)
	{
		ReportCustomEvent(EventName.building_built, new BuildingEventContext
		{
			buildingID = info.id
		});
	}

	private void OnBuildingDestroyed(int guid, BuildingInfo info)
	{
		ReportCustomEvent(EventName.building_destroyed, new BuildingEventContext
		{
			buildingID = info.id
		});
	}

	private void OnLevelChanged(int from, int to)
	{
		ReportCustomEvent(EventName.role_level_changed, new LevelChangedEventContext(from, to));
	}

	private void OnHealthDead(Health health, DamageInfo info)
	{
		if (!(health == null))
		{
			_ = health.team;
			bool flag = false;
			if (info.fromCharacter != null && info.fromCharacter.IsMainCharacter())
			{
				flag = true;
			}
			if (flag)
			{
				ReportCustomEvent(EventName.enemy_kill, new EnemyKillInfo
				{
					enemyPresetName = GetPresetName(health),
					damageInfo = info
				});
			}
		}
		static string GetPresetName(Health health2)
		{
			CharacterMainControl characterMainControl = health2.TryGetCharacter();
			if (characterMainControl == null)
			{
				return "None";
			}
			CharacterRandomPreset characterPreset = characterMainControl.characterPreset;
			if (characterPreset == null)
			{
				return "None";
			}
			return characterPreset.Name;
		}
	}

	private void OnFormulaUnlocked(string formulaID)
	{
		ReportCustomEvent(EventName.craft_formula_unlock, StrJson.Create("id", formulaID));
	}

	private void OnItemCrafted(CraftingFormula formula, Item item)
	{
		ReportCustomEvent(EventName.craft_craft, formula);
	}

	private void OnItemPurchased(StockShop shop, Item item)
	{
		if (!(shop == null) && !(item == null))
		{
			ReportCustomEvent(EventName.shop_purchased, new PurchaseInfo
			{
				shopID = shop.MerchantID,
				itemTypeID = item.TypeID,
				itemAmount = item.StackCount
			});
		}
	}

	private void OnItemSentToPlayerStorage(Item item)
	{
		if (!(item == null))
		{
			ReportCustomEvent(EventName.item_to_storage, new ItemInfo
			{
				itemId = item.TypeID,
				amount = item.StackCount
			});
		}
	}

	private void OnItemSentToPlayerInventory(Item item)
	{
		if (!(item == null))
		{
			ReportCustomEvent(EventName.item_to_inventory, new ItemInfo
			{
				itemId = item.TypeID,
				amount = item.StackCount
			});
		}
	}

	private void OnMoneyPaid(long money)
	{
		ReportCustomEvent(EventName.pay_money, new Cost
		{
			money = money,
			items = new Cost.ItemEntry[0]
		});
	}

	private void OnCostPaid(Cost cost)
	{
		ReportCustomEvent(EventName.pay_cost, cost);
	}

	private void OnQuestActivated(Quest quest)
	{
		if (!(quest == null))
		{
			ReportCustomEvent(EventName.quest_activate, quest.GetInfo());
		}
	}

	private void OnQuestCompleted(Quest quest)
	{
		if (!(quest == null))
		{
			ReportCustomEvent(EventName.quest_complete, quest.GetInfo());
		}
	}

	private void OnMainCharacterDead(DamageInfo info)
	{
		string fromCharacterPresetName = "None";
		string fromCharacterNameKey = "None";
		if ((bool)info.fromCharacter)
		{
			CharacterRandomPreset characterPreset = info.fromCharacter.characterPreset;
			if (characterPreset != null)
			{
				fromCharacterPresetName = characterPreset.name;
				fromCharacterNameKey = characterPreset.nameKey;
			}
		}
		ReportCustomEvent(EventName.main_character_dead, new CharacterDeathContext
		{
			damageInfo = info,
			levelInfo = LevelManager.GetCurrentLevelInfo(),
			fromCharacterPresetName = fromCharacterPresetName,
			fromCharacterNameKey = fromCharacterNameKey
		});
	}

	private void OnEvacuated(EvacuationInfo evacuationInfo)
	{
		LevelManager.LevelInfo currentLevelInfo = LevelManager.GetCurrentLevelInfo();
		RaidUtilities.RaidInfo currentRaid = RaidUtilities.CurrentRaid;
		PlayerStatus playerStatus = PlayerStatus.CreateFromCurrent();
		ReportCustomEvent(EventName.level_evacuated, new EvacuationEventData
		{
			evacuationInfo = evacuationInfo,
			mapID = currentLevelInfo.activeSubSceneID,
			raidInfo = currentRaid,
			playerStatus = playerStatus
		});
	}

	private void OnLevelInitialized()
	{
		ReportCustomEvent(EventName.level_initialized, LevelManager.GetCurrentLevelInfo());
	}

	private void OnSceneLoadingFinish(SceneLoadingContext context)
	{
		ReportCustomEvent(EventName.scene_load_start, context);
	}

	private void OnSceneLoadingStart(SceneLoadingContext context)
	{
		ReportCustomEvent(EventName.scene_load_finish, context);
	}

	private void OnRaidEnd(RaidUtilities.RaidInfo info)
	{
		ReportCustomEvent(EventName.raid_end, info);
	}

	private void OnNewRaid(RaidUtilities.RaidInfo info)
	{
		ReportCustomEvent(EventName.raid_new, info);
	}

	private void OnSaveDeleted()
	{
		ReportCustomEvent(EventName.delete_save_data, StrJson.Create("slot", $"{SavesSystem.CurrentSlot}"));
	}

	private void OnGameStarted()
	{
		int num = PlayerPrefs.GetInt("AppStartCount", 0);
		sessionInfo = new SessionInfo
		{
			startCount = num,
			isFirstTimeStart = (num <= 0),
			session_id = DateTime.Now.ToBinary().GetHashCode()
		};
		sessionStartTime = DateTime.Now;
		ReportCustomEvent(EventName.app_start, sessionInfo);
		PlayerPrefs.SetInt("AppStartCount", ++num);
		PlayerPrefs.Save();
	}

	private void ReportCustomEvent(EventName eventName, StrJson customParameters)
	{
		ReportCustomEvent(eventName, customParameters.ToString());
	}

	private void ReportCustomEvent<T>(EventName eventName, T customParameters)
	{
		string customParameters2 = ((customParameters != null) ? JsonUtility.ToJson(customParameters) : "");
		ReportCustomEvent(eventName, customParameters2);
	}

	private void ReportCustomEvent(EventName eventName, string customParameters = "")
	{
		string strPlayerInfo = PlayerInfo.GetCurrent().ToJson();
		SDK.ReportCustomEvent(eventName.ToString(), strPlayerInfo, "", customParameters);
		try
		{
			OnReportCustomEvent?.Invoke(eventName.ToString(), customParameters);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
