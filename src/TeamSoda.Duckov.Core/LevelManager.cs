using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.MiniMaps;
using Duckov.Rules;
using Duckov.Scenes;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using Saves;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
	[Serializable]
	public struct LevelInfo
	{
		public bool isBaseLevel;

		public string sceneName;

		public string activeSubSceneID;
	}

	private Transform _lootBoxInventoriesParent;

	private Dictionary<int, Inventory> _lootBoxInventories;

	[SerializeField]
	private Transform defaultStartPos;

	private static LevelManager instance;

	[SerializeField]
	private InputManager inputManager;

	[SerializeField]
	private CharacterCreator characterCreator;

	[SerializeField]
	private ExitCreator exitCreator;

	[SerializeField]
	private ExplosionManager explosionManager;

	[SerializeField]
	private CharacterModel characterModel;

	private CharacterMainControl mainCharacter;

	private CharacterMainControl petCharacter;

	[SerializeField]
	private GameCamera gameCamera;

	[SerializeField]
	private FogOfWarManager fowManager;

	[SerializeField]
	private TimeOfDayController timeOfDayController;

	[SerializeField]
	private AIMainBrain aiMainBrain;

	[SerializeField]
	private CharacterRandomPreset matePreset;

	private bool initingLevel;

	private bool isNewRaidLevel;

	private bool afterInit;

	[SerializeField]
	private CharacterRandomPreset petPreset;

	[SerializeField]
	private Sprite characterMapIcon;

	[SerializeField]
	private Color characterMapIconColor;

	[SerializeField]
	private Color characterMapShadowColor;

	[SerializeField]
	private MultiSceneLocation testTeleportTarget;

	[SerializeField]
	public SkillBase defaultSkill;

	[SerializeField]
	private PetProxy petProxy;

	[SerializeField]
	private CustomFaceManager customFaceManager;

	[SerializeField]
	private BulletPool bulletPool;

	private string _levelInitializingComment = "";

	public static int loadLevelBeaconIndex = 0;

	private bool levelInited;

	public const string MainCharacterItemSaveKey = "MainCharacterItemData";

	public const string MainCharacterHealthSaveKey = "MainCharacterHealth";

	private float levelStartTime = -0.1f;

	private static Ruleset rule;

	private static List<object> waitForInitializationList = new List<object>();

	private bool dieTask;

	public static LevelManager Instance
	{
		get
		{
			if (!instance)
			{
				SetInstance();
			}
			return instance;
		}
	}

	public static Transform LootBoxInventoriesParent
	{
		get
		{
			if (Instance._lootBoxInventoriesParent == null)
			{
				GameObject gameObject = new GameObject("Loot Box Inventories");
				gameObject.transform.SetParent(Instance.transform);
				Instance._lootBoxInventoriesParent = gameObject.transform;
				LootBoxInventories.Clear();
			}
			return Instance._lootBoxInventoriesParent;
		}
	}

	public static Dictionary<int, Inventory> LootBoxInventories
	{
		get
		{
			if (Instance._lootBoxInventories == null)
			{
				Instance._lootBoxInventories = new Dictionary<int, Inventory>();
			}
			return Instance._lootBoxInventories;
		}
	}

	public bool IsRaidMap => LevelConfig.IsRaidMap;

	public bool IsBaseLevel => LevelConfig.IsBaseLevel;

	public InputManager InputManager => inputManager;

	public CharacterCreator CharacterCreator => characterCreator;

	public ExitCreator ExitCreator => exitCreator;

	public ExplosionManager ExplosionManager => explosionManager;

	private int characterItemTypeID => GameplayDataSettings.ItemAssets.DefaultCharacterItemTypeID;

	public CharacterMainControl MainCharacter => mainCharacter;

	public CharacterMainControl PetCharacter => petCharacter;

	public GameCamera GameCamera => gameCamera;

	public FogOfWarManager FogOfWarManager => fowManager;

	public TimeOfDayController TimeOfDayController => timeOfDayController;

	public AIMainBrain AIMainBrain => aiMainBrain;

	public static bool LevelInitializing
	{
		get
		{
			if (Instance == null)
			{
				return false;
			}
			return Instance.initingLevel;
		}
	}

	public static bool AfterInit
	{
		get
		{
			if (Instance == null)
			{
				return false;
			}
			return Instance.afterInit;
		}
	}

	public PetProxy PetProxy => petProxy;

	public BulletPool BulletPool => bulletPool;

	public CustomFaceManager CustomFaceManager => customFaceManager;

	public static string LevelInitializingComment
	{
		get
		{
			if (Instance == null)
			{
				return null;
			}
			return Instance._levelInitializingComment;
		}
		set
		{
			if (!(Instance == null))
			{
				Instance._levelInitializingComment = value;
				LevelManager.OnLevelInitializingCommentChanged?.Invoke(value);
				Debug.Log("[Level Initialization] " + value);
			}
		}
	}

	public static bool LevelInited
	{
		get
		{
			if (instance == null)
			{
				return false;
			}
			return instance.levelInited;
		}
	}

	public float LevelTime => Time.time - levelStartTime;

	public static Ruleset Rule => rule;

	public static event Action OnLevelBeginInitializing;

	public static event Action OnLevelInitialized;

	public static event Action OnAfterLevelInitialized;

	public static event Action<string> OnLevelInitializingCommentChanged;

	public static event Action<EvacuationInfo> OnEvacuated;

	public static event Action<DamageInfo> OnMainCharacterDead;

	public static event Action OnNewGameReport;

	public static void RegisterWaitForInitialization<T>(T toWait) where T : class, IInitializedQueryHandler
	{
		if (toWait != null && toWait != null)
		{
			waitForInitializationList.Add(toWait);
		}
	}

	public static bool UnregisterWaitForInitialization<T>(T obj) where T : class
	{
		return waitForInitializationList.Remove(obj);
	}

	private void Start()
	{
		if (!SceneLoader.IsSceneLoading)
		{
			StartInit(default(SceneLoadingContext));
		}
		else
		{
			SceneLoader.onFinishedLoadingScene += StartInit;
		}
		if (!SavesSystem.Load<bool>("NewGameReported"))
		{
			SavesSystem.Save("NewGameReported", value: true);
			LevelManager.OnNewGameReport?.Invoke();
		}
		if (GameManager.newBoot)
		{
			OnNewBoot();
			GameManager.newBoot = false;
		}
	}

	private void OnDestroy()
	{
		SceneLoader.onFinishedLoadingScene -= StartInit;
		mainCharacter?.Health?.OnDeadEvent.RemoveListener(OnMainCharacterDie);
	}

	private void OnNewBoot()
	{
		Debug.Log("New boot");
		GameClock.Instance.StepTimeTil(new TimeSpan(7, 0, 0));
	}

	private void StartInit(SceneLoadingContext context)
	{
		InitLevel(context).Forget();
	}

	private async UniTaskVoid InitLevel(SceneLoadingContext context)
	{
		if (initingLevel)
		{
			return;
		}
		LevelInitializingComment = "Starting up...";
		instance = this;
		_ = GameManager.Instance;
		initingLevel = true;
		LevelInitializingComment = "Invoking Beginning Event...";
		LevelManager.OnLevelBeginInitializing?.Invoke();
		await UniTask.Yield();
		LevelInitializingComment = "Setting up rule...";
		rule = GameRulesManager.Current;
		Debug.Log($"Rule is:{rule.DisplayName},Recoil:{rule.RecoilMultiplier}");
		Vector3 startPos = defaultStartPos.position;
		if (context.useLocation && MultiSceneCore.Instance != null)
		{
			LevelInitializingComment = "Finding location for spawning...";
			MultiSceneLocation location = context.location;
			LevelInitializingComment = "Creating Character...";
			await CreateMainCharacterAsync(startPos, Quaternion.identity);
			LevelInitializingComment = "Teleporting to location...";
			await MultiSceneCore.Instance.LoadAndTeleport(location);
			startPos = location.GetLocationTransform().position;
		}
		else if (MultiSceneCore.Instance != null)
		{
			LevelInitializingComment = "Getting location...";
			(string, SubSceneEntry.Location) playerStartLocation = GetPlayerStartLocation();
			if (playerStartLocation.Item2 != null)
			{
				LevelInitializingComment = "Creating location info...";
				MultiSceneLocation location = new MultiSceneLocation
				{
					SceneID = playerStartLocation.Item1,
					LocationName = playerStartLocation.Item2.path
				};
				LevelInitializingComment = "Setting start position...";
				startPos = playerStartLocation.Item2.position;
				LevelInitializingComment = "Creating character at location...";
				await CreateMainCharacterAsync(playerStartLocation.Item2.position, Quaternion.identity);
				LevelInitializingComment = "Teleporting to location...";
				await MultiSceneCore.Instance.LoadAndTeleport(location);
			}
			else
			{
				LevelInitializingComment = "Setting default start position...";
				startPos = defaultStartPos.position;
				LevelInitializingComment = "Creating character at default position...";
				await CreateMainCharacterAsync(defaultStartPos.position, Quaternion.identity);
			}
		}
		else
		{
			LevelInitializingComment = "Creating character...";
			await CreateMainCharacterAsync(defaultStartPos.position, Quaternion.identity);
		}
		LevelInitializingComment = "Setting up character status...";
		mainCharacter.Health.OnDeadEvent.AddListener(OnMainCharacterDie);
		RefreshMainCharacterFace();
		LevelInitializingComment = "Setting up pet...";
		petCharacter = await petPreset.CreateCharacterAsync(mainCharacter.transform.position + Vector3.one * 99f, Vector3.forward, MultiSceneCore.MainScene.Value.buildIndex, null, isLeader: false);
		if (IsBaseLevel && petProxy != null)
		{
			petProxy.DestroyItemInBase();
		}
		petCharacter.Health.showHealthBar = false;
		petCharacter.Health.SetInvincible(value: true);
		LevelInitializingComment = "Setting character items...";
		SetCharacterItemsInspected();
		LevelInitializingComment = "Waiting for other initialization...";
		await WaitForOtherInitialization();
		mainCharacter.SwitchToFirstAvailableWeapon();
		if (MultiSceneCore.Instance != null)
		{
			while (MultiSceneCore.Instance.IsLoading)
			{
				await UniTask.Yield();
			}
		}
		initingLevel = false;
		levelInited = true;
		levelStartTime = Time.time;
		LevelInitializingComment = "Handling raid initialization...";
		HandleRaidInitialization();
		LevelInitializingComment = "Invoking initialized event...";
		LevelManager.OnLevelInitialized?.Invoke();
		LevelInitializingComment = "Healing...";
		float health = SavesSystem.Load<float>("MainCharacterHealth");
		mainCharacter.Health.SetHealth(health);
		if (IsBaseLevel || isNewRaidLevel)
		{
			mainCharacter.AddHealth(mainCharacter.Health.MaxHealth);
		}
		LevelInitializingComment = "Spawing exits...";
		if (MultiSceneCore.Instance != null)
		{
			exitCreator.Spawn();
		}
		LevelInitializingComment = "Creating map element...";
		CreateMainCharacterMapElement();
		LevelInitializingComment = "Setting character position...";
		await UniTask.WaitForSeconds(0.25f);
		mainCharacter.SetPosition(startPos);
		LevelInitializingComment = "Done!";
		try
		{
			LevelManager.OnAfterLevelInitialized?.Invoke();
			afterInit = true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private async UniTask CreateMate()
	{
		if ((bool)matePreset)
		{
			AICharacterController componentInChildren = (await matePreset.CreateCharacterAsync(mainCharacter.transform.position + Vector3.one, Vector3.forward, MultiSceneCore.MainScene.Value.buildIndex, null, isLeader: false)).GetComponentInChildren<AICharacterController>();
			if ((bool)componentInChildren)
			{
				componentInChildren.leader = mainCharacter;
			}
		}
	}

	private async UniTask WaitForOtherInitialization()
	{
		while (waitForInitializationList.Any(delegate(object e)
		{
			if (e == null)
			{
				return false;
			}
			if (!(e is IInitializedQueryHandler initializedQueryHandler))
			{
				return false;
			}
			if (!initializedQueryHandler.HasInitialized())
			{
				LevelInitializingComment = "Waiting for " + e.GetType().Name + "...";
				return true;
			}
			return false;
		}))
		{
			await UniTask.Yield();
		}
	}

	private void HandleRaidInitialization()
	{
		RaidUtilities.RaidInfo currentRaid = RaidUtilities.CurrentRaid;
		if (IsRaidMap)
		{
			if (currentRaid.ended)
			{
				RaidUtilities.NewRaid();
				isNewRaidLevel = true;
			}
		}
		else if (IsBaseLevel && !currentRaid.ended)
		{
			RaidUtilities.NotifyEnd();
		}
	}

	public void RefreshMainCharacterFace()
	{
		if ((bool)mainCharacter.characterModel.CustomFace)
		{
			CustomFaceSettingData saveData = customFaceManager.LoadMainCharacterSetting();
			mainCharacter.characterModel.CustomFace.LoadFromData(saveData);
		}
	}

	private async UniTask CreateMainCharacterAsync(Vector3 position, Quaternion rotation)
	{
		Item itemInstance = await LoadOrCreateCharacterItemInstance();
		mainCharacter = await characterCreator.CreateCharacter(itemInstance, characterModel, position, rotation);
		if (!(mainCharacter == null))
		{
			if (IsBaseLevel)
			{
				mainCharacter.DestroyItemsThatNeededToBeDestriedInBase();
			}
			mainCharacter.SetTeam(Teams.player);
			mainCharacter.CharacterItem.Inventory.AcceptSticky = true;
			if (defaultSkill != null)
			{
				SkillBase skillBase = UnityEngine.Object.Instantiate(defaultSkill);
				skillBase.transform.SetParent(mainCharacter.transform, worldPositionStays: false);
				mainCharacter.SetSkill(SkillTypes.characterSkill, skillBase, skillBase.gameObject);
			}
			inputManager.characterMainControl = mainCharacter;
			inputManager.SwitchItemAgent(1);
			gameCamera.SetTarget(mainCharacter);
		}
	}

	private void SetCharacterItemsInspected()
	{
		foreach (Slot slot in mainCharacter.CharacterItem.Slots)
		{
			if (slot.Content != null)
			{
				slot.Content.Inspected = true;
			}
		}
		foreach (Item item in mainCharacter.CharacterItem.Inventory)
		{
			if (item != null)
			{
				item.Inspected = true;
			}
		}
		foreach (Item item2 in petProxy.Inventory)
		{
			if (item2 != null)
			{
				item2.Inspected = true;
			}
		}
	}

	private static void SetInstance()
	{
		if (!instance)
		{
			instance = UnityEngine.Object.FindFirstObjectByType<LevelManager>();
			_ = (bool)instance;
		}
	}

	private async UniTask<Item> LoadOrCreateCharacterItemInstance()
	{
		Item item = await ItemSavesUtilities.LoadItem("MainCharacterItemData");
		if (item == null)
		{
			item = await ItemAssetsCollection.InstantiateAsync(characterItemTypeID);
			Debug.LogWarning("Item Loading failed");
		}
		return item;
	}

	public void NotifyEvacuated(EvacuationInfo info)
	{
		mainCharacter.Health.SetInvincible(value: true);
		LevelManager.OnEvacuated?.Invoke(info);
		SaveMainCharacter();
		SavesSystem.CollectSaveData();
		SavesSystem.SaveFile();
	}

	public void NotifySaveBeforeLoadScene(bool saveToFile)
	{
		SaveMainCharacter();
		SavesSystem.CollectSaveData();
		if (saveToFile)
		{
			SavesSystem.SaveFile();
		}
	}

	private void OnMainCharacterDie(DamageInfo dmgInfo)
	{
		if (!dieTask)
		{
			dieTask = true;
			CharacterDieTask(dmgInfo).Forget();
			LevelManager.OnMainCharacterDead?.Invoke(dmgInfo);
		}
	}

	private async UniTaskVoid CharacterDieTask(DamageInfo dmgInfo)
	{
		if (IsRaidMap)
		{
			RaidUtilities.NotifyDead();
			DeadBodyManager.RecordDeath(mainCharacter);
		}
		ItemSavesUtilities.SaveAsLastDeadCharacter(mainCharacter.CharacterItem);
		if (LevelConfig.SpawnTomb)
		{
			InteractableLootbox.CreateFromItem(mainCharacter.CharacterItem, mainCharacter.transform.position, mainCharacter.transform.rotation, moveToMainScene: true, GameplayDataSettings.Prefabs.LootBoxPrefab_Tomb, filterDontDropOnDead: true);
		}
		else
		{
			mainCharacter?.DropAllItems();
		}
		mainCharacter?.DestroyAllItem();
		SaveMainCharacter();
		SavesSystem.CollectSaveData();
		SavesSystem.SaveFile();
		await UniTask.WaitForSeconds(2.5f, ignoreTimeScale: true);
		await ClosureView.ShowAndReturnTask(dmgInfo);
		if (OverrideDeathSceneRouting.Instance != null)
		{
			Debug.Log("死亡后的目标场景已被特殊脚本修改");
			SceneLoader.Instance.LoadScene(OverrideDeathSceneRouting.Instance.GetSceneID(), GameplayDataSettings.SceneManagement.FailLoadingScreenScene).Forget();
		}
		else
		{
			SceneLoader.Instance.LoadBaseScene(GameplayDataSettings.SceneManagement.FailLoadingScreenScene).Forget();
		}
	}

	internal void SaveMainCharacter()
	{
		mainCharacter.CharacterItem.Save("MainCharacterItemData");
		SavesSystem.Save("MainCharacterHealth", MainCharacter.Health.CurrentHealth);
	}

	private (string sceneID, SubSceneEntry.Location locationData) GetPlayerStartLocation()
	{
		List<(string, SubSceneEntry.Location)> list = new List<(string, SubSceneEntry.Location)>();
		string text = "StartPoints";
		if (loadLevelBeaconIndex > 0)
		{
			text = text + "_" + loadLevelBeaconIndex;
			loadLevelBeaconIndex = 0;
		}
		foreach (SubSceneEntry subScene in MultiSceneCore.Instance.SubScenes)
		{
			foreach (SubSceneEntry.Location cachedLocation in subScene.cachedLocations)
			{
				if (IsPathCompatible(cachedLocation, text))
				{
					list.Add((subScene.sceneID, cachedLocation));
				}
			}
		}
		if (list.Count == 0)
		{
			text = "StartPoints";
			foreach (SubSceneEntry subScene2 in MultiSceneCore.Instance.SubScenes)
			{
				foreach (SubSceneEntry.Location cachedLocation2 in subScene2.cachedLocations)
				{
					if (IsPathCompatible(cachedLocation2, text))
					{
						list.Add((subScene2.sceneID, cachedLocation2));
					}
				}
			}
		}
		return list.GetRandom();
	}

	private void CreateMainCharacterMapElement()
	{
		if (MultiSceneCore.Instance != null)
		{
			SimplePointOfInterest simplePointOfInterest = mainCharacter.gameObject.AddComponent<SimplePointOfInterest>();
			simplePointOfInterest.Color = characterMapIconColor;
			simplePointOfInterest.ShadowColor = characterMapShadowColor;
			simplePointOfInterest.ShadowDistance = 0f;
			simplePointOfInterest.Setup(characterMapIcon, "You", followActiveScene: true);
		}
	}

	private void OnSubSceneLoaded()
	{
	}

	private bool IsPathCompatible(SubSceneEntry.Location location, string keyWord)
	{
		string path = location.path;
		int num = path.IndexOf('/');
		if (num != -1 && path.Substring(0, num) == keyWord)
		{
			return true;
		}
		return false;
	}

	public void TestTeleport()
	{
		MultiSceneCore.Instance.LoadAndTeleport(testTeleportTarget).Forget();
	}

	private LevelInfo mGetInfo()
	{
		Scene? activeSubScene = MultiSceneCore.ActiveSubScene;
		string activeSubSceneID = (activeSubScene.HasValue ? activeSubScene.Value.name : "");
		return new LevelInfo
		{
			isBaseLevel = IsBaseLevel,
			sceneName = base.gameObject.scene.name,
			activeSubSceneID = activeSubSceneID
		};
	}

	public static LevelInfo GetCurrentLevelInfo()
	{
		if (Instance == null)
		{
			return default(LevelInfo);
		}
		return Instance.mGetInfo();
	}
}
