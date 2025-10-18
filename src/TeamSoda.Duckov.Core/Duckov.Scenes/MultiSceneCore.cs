using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.Economy;
using Duckov.MiniMaps;
using Duckov.UI;
using Duckov.Utilities;
using Eflatun.SceneReference;
using Saves;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Duckov.Scenes;

public class MultiSceneCore : MonoBehaviour
{
	[SerializeField]
	private string levelStateName = "None";

	[SerializeField]
	private string playStinger = "";

	[SerializeField]
	private bool playAfterLevelInit;

	[SerializeField]
	private List<SubSceneEntry> subScenes;

	private Scene activeSubScene;

	[HideInInspector]
	public List<int> usedCreatorIds = new List<int>();

	[HideInInspector]
	public Dictionary<int, object> inLevelData = new Dictionary<int, object>();

	[SerializeField]
	private bool teleportToRandomOnLevelInitialized;

	private Dictionary<int, GameObject> setActiveWithSceneObjects = new Dictionary<int, GameObject>();

	private bool isLoading;

	private SubSceneEntry cachedSubsceneEntry;

	public static MultiSceneCore Instance { get; private set; }

	public List<SubSceneEntry> SubScenes => subScenes;

	public static Scene? MainScene
	{
		get
		{
			if (Instance == null)
			{
				return null;
			}
			return Instance.gameObject.scene;
		}
	}

	public static string ActiveSubSceneID
	{
		get
		{
			if (!ActiveSubScene.HasValue)
			{
				return null;
			}
			if (Instance == null)
			{
				return null;
			}
			return Instance.SubScenes.Find((SubSceneEntry e) => e != null && ActiveSubScene.Value.buildIndex == e.Info.BuildIndex)?.sceneID;
		}
	}

	public static Scene? ActiveSubScene
	{
		get
		{
			if (Instance == null)
			{
				return null;
			}
			if (Instance.isLoading)
			{
				return null;
			}
			return Instance.activeSubScene;
		}
	}

	public SceneInfoEntry SceneInfo => SceneInfoCollection.GetSceneInfo(base.gameObject.scene.buildIndex);

	public string DisplayName
	{
		get
		{
			SceneInfoEntry sceneInfo = SceneInfoCollection.GetSceneInfo(base.gameObject.scene.buildIndex);
			if (sceneInfo == null)
			{
				return "?";
			}
			return sceneInfo.DisplayName;
		}
	}

	public string DisplaynameRaw
	{
		get
		{
			SceneInfoEntry sceneInfo = SceneInfoCollection.GetSceneInfo(base.gameObject.scene.buildIndex);
			if (sceneInfo == null)
			{
				return "?";
			}
			return sceneInfo.DisplayNameRaw;
		}
	}

	public bool IsLoading => isLoading;

	public static string MainSceneID => SceneInfoCollection.GetSceneID(MainScene.Value.buildIndex);

	public static event Action<MultiSceneCore, Scene> OnSubSceneWillBeUnloaded;

	public static event Action<MultiSceneCore, Scene> OnSubSceneLoaded;

	public static event Action<MultiSceneCore> OnInstanceAwake;

	public static event Action<MultiSceneCore> OnInstanceDestroy;

	public static event Action<string> OnSetSceneVisited;

	public static void MoveToActiveWithScene(GameObject go, int sceneBuildIndex)
	{
		if (!(Instance == null))
		{
			Transform setActiveWithSceneParent = Instance.GetSetActiveWithSceneParent(sceneBuildIndex);
			go.transform.SetParent(setActiveWithSceneParent);
		}
	}

	public static void MoveToActiveWithScene(GameObject go)
	{
		int buildIndex = go.scene.buildIndex;
		MoveToActiveWithScene(go, buildIndex);
	}

	public Transform GetSetActiveWithSceneParent(int sceneBuildIndex)
	{
		if (setActiveWithSceneObjects.TryGetValue(sceneBuildIndex, out var value))
		{
			return value.transform;
		}
		SceneInfoEntry sceneInfoEntry = SceneInfoCollection.GetSceneInfo(sceneBuildIndex);
		if (sceneInfoEntry == null)
		{
			sceneInfoEntry = new SceneInfoEntry();
			Debug.LogWarning($"BuildIndex {sceneBuildIndex} 的sceneInfo不存在");
		}
		GameObject gameObject = new GameObject(sceneInfoEntry.ID);
		gameObject.transform.SetParent(base.transform);
		setActiveWithSceneObjects.Add(sceneBuildIndex, gameObject);
		gameObject.SetActive(sceneInfoEntry.IsLoaded);
		return gameObject.transform;
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Multiple Multi Scene Core detected!");
		}
		MultiSceneCore.OnInstanceAwake?.Invoke(this);
		if (playAfterLevelInit)
		{
			if (LevelManager.AfterInit)
			{
				PlayStinger();
			}
			else
			{
				LevelManager.OnAfterLevelInitialized += OnAfterLevelInitialized;
			}
		}
	}

	private void OnDestroy()
	{
		MultiSceneCore.OnInstanceDestroy?.Invoke(this);
		LevelManager.OnAfterLevelInitialized -= OnAfterLevelInitialized;
	}

	private void OnAfterLevelInitialized()
	{
		if (playAfterLevelInit)
		{
			PlayStinger();
		}
	}

	public void PlayStinger()
	{
		if (!string.IsNullOrWhiteSpace(playStinger))
		{
			AudioManager.PlayStringer(playStinger);
		}
	}

	private void Start()
	{
		CreatePointsOfInterestsForLocations();
		AudioManager.StopBGM();
		AudioManager.SetState("Level", levelStateName);
		if (SceneInfo != null && !string.IsNullOrEmpty(SceneInfo.ID))
		{
			SetVisited(SceneInfo.ID);
		}
	}

	public static void SetVisited(string sceneID)
	{
		SavesSystem.Save("MultiSceneCore_Visited_" + sceneID, value: true);
		MultiSceneCore.OnSetSceneVisited?.Invoke(sceneID);
	}

	public static bool GetVisited(string sceneID)
	{
		return SavesSystem.Load<bool>("MultiSceneCore_Visited_" + sceneID);
	}

	private void CreatePointsOfInterestsForLocations()
	{
		foreach (SubSceneEntry subScene in SubScenes)
		{
			foreach (SubSceneEntry.Location cachedLocation in subScene.cachedLocations)
			{
				if (cachedLocation.showInMap)
				{
					SimplePointOfInterest.Create(cachedLocation.position, subScene.sceneID, cachedLocation.DisplayNameRaw, null, hideIcon: true);
				}
			}
		}
	}

	private void CreatePointsOfInterestsForTeleporters()
	{
		foreach (SubSceneEntry subScene in SubScenes)
		{
			foreach (SubSceneEntry.TeleporterInfo cachedTeleporter in subScene.cachedTeleporters)
			{
				SimplePointOfInterest.Create(cachedTeleporter.position, subScene.sceneID, "", GameplayDataSettings.UIStyle.DefaultTeleporterIcon).ScaleFactor = GameplayDataSettings.UIStyle.TeleporterIconScale;
			}
		}
	}

	public void BeginLoadSubScene(SceneReference reference)
	{
		LoadSubScene(reference).Forget();
	}

	private SceneReference GetSubSceneReference(string sceneID)
	{
		return subScenes.Find((SubSceneEntry e) => e.sceneID == sceneID)?.SceneReference;
	}

	private async UniTask<bool> LoadSubScene(SceneReference targetScene, bool withBlackScreen = true)
	{
		if (SceneLoader.IsSceneLoading)
		{
			Debug.LogWarning("已经在加载子场景了");
			return false;
		}
		if (isLoading)
		{
			Debug.LogWarning("已经在加载子场景了");
			return false;
		}
		if (targetScene == null)
		{
			Debug.LogWarning("目标场景为空");
			return false;
		}
		isLoading = true;
		if (withBlackScreen)
		{
			await BlackScreen.ShowAndReturnTask();
		}
		if (Cost.TaskPending)
		{
			Debug.LogError("MultiSceneCore: 检测到正在返还物品");
		}
		_ = Time.unscaledTime;
		Scene currentMainScene = base.gameObject.scene;
		SceneManager.SetActiveScene(base.gameObject.scene);
		List<UniTask> list = new List<UniTask>();
		if (activeSubScene.isLoaded)
		{
			MultiSceneCore.OnSubSceneWillBeUnloaded?.Invoke(this, activeSubScene);
			LocalOnSubSceneWillBeUnloaded(activeSubScene);
			UniTask item = SceneManager.UnloadSceneAsync(activeSubScene).ToUniTask();
			list.Add(item);
		}
		UniTask item2 = SceneManager.LoadSceneAsync(targetScene.BuildIndex, LoadSceneMode.Additive).ToUniTask();
		list.Add(item2);
		await UniTask.WhenAll(list);
		if (currentMainScene != SceneManager.GetActiveScene())
		{
			Debug.LogError("Sub-scene loading failed because the Active Scene has Changed during this process!");
			await SceneManager.UnloadSceneAsync(targetScene.BuildIndex);
			return false;
		}
		activeSubScene = targetScene.LoadedScene;
		cachedSubsceneEntry = GetSubSceneInfo(activeSubScene);
		AudioManager.SetState("GameStatus", "Playing");
		string sceneID = SceneInfoCollection.GetSceneID(targetScene.BuildIndex);
		if (!string.IsNullOrEmpty(sceneID))
		{
			SetVisited(sceneID);
		}
		LocalOnSubSceneLoaded(activeSubScene);
		SceneManager.SetActiveScene(activeSubScene);
		await UniTask.NextFrame();
		_ = AudioManager.PlayingBGM;
		BlackScreen.HideAndReturnTask().Forget();
		isLoading = false;
		MultiSceneCore.OnSubSceneLoaded?.Invoke(this, activeSubScene);
		return true;
	}

	private void LocalOnSubSceneWillBeUnloaded(Scene scene)
	{
		subScenes.Find((SubSceneEntry e) => e != null && e.Info.BuildIndex == scene.buildIndex);
		Transform setActiveWithSceneParent = GetSetActiveWithSceneParent(scene.buildIndex);
		Debug.Log($"Setting Active False {setActiveWithSceneParent.name}  {scene.buildIndex}");
		setActiveWithSceneParent.gameObject.SetActive(value: false);
	}

	private void LocalOnSubSceneLoaded(Scene scene)
	{
		subScenes.Find((SubSceneEntry e) => e != null && e.Info.BuildIndex == scene.buildIndex);
		GetSetActiveWithSceneParent(scene.buildIndex).gameObject.SetActive(value: true);
	}

	public async UniTask<bool> LoadAndTeleport(MultiSceneLocation location)
	{
		if (!SubScenes.Any((SubSceneEntry e) => e.sceneID == location.SceneID))
		{
			return false;
		}
		if (!(await LoadSubScene(location.Scene)))
		{
			return false;
		}
		Transform locationTransform = location.GetLocationTransform();
		if (locationTransform == null)
		{
			Debug.LogError("Location Not Found: " + location.Scene.Name + "/" + location.LocationName);
		}
		LevelManager.Instance.MainCharacter.SetPosition(locationTransform.position);
		return true;
	}

	public async UniTask<bool> LoadAndTeleport(string sceneID, Vector3 position, bool subSceneLocation = false)
	{
		if (!SubScenes.Any((SubSceneEntry e) => e.sceneID == sceneID))
		{
			return false;
		}
		SceneReference subSceneReference = GetSubSceneReference(sceneID);
		if (!(await LoadSubScene(subSceneReference)))
		{
			return false;
		}
		CharacterMainControl mainCharacter = LevelManager.Instance.MainCharacter;
		Vector3 result = position;
		if (subSceneLocation && !MiniMapSettings.TryGetWorldPosition(position, sceneID, out result))
		{
			return false;
		}
		mainCharacter.SetPosition(result);
		return true;
	}

	public static void MoveToMainScene(GameObject gameObject)
	{
		if (Instance == null)
		{
			Debug.LogError("移动到主场景失败，因为MultiSceneCore不存在");
		}
		else
		{
			SceneManager.MoveGameObjectToScene(gameObject, MainScene.Value);
		}
	}

	public void CacheLocations()
	{
	}

	public void CacheTeleporters()
	{
	}

	private Vector3 GetClosestTeleporterPosition(Vector3 pos)
	{
		float num = float.MaxValue;
		Vector3 result = pos;
		foreach (SubSceneEntry subScene in subScenes)
		{
			foreach (SubSceneEntry.TeleporterInfo cachedTeleporter in subScene.cachedTeleporters)
			{
				float magnitude = (cachedTeleporter.position - pos).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					result = cachedTeleporter.position;
				}
			}
		}
		return result;
	}

	internal bool TryGetCachedPosition(MultiSceneLocation location, out Vector3 result)
	{
		return TryGetCachedPosition(location.SceneID, location.LocationName, out result);
	}

	internal bool TryGetCachedPosition(string sceneID, string locationName, out Vector3 result)
	{
		result = default(Vector3);
		SubSceneEntry subSceneEntry = subScenes.Find((SubSceneEntry e) => e != null && e.sceneID == sceneID);
		if (subSceneEntry == null)
		{
			return false;
		}
		if (subSceneEntry.TryGetCachedPosition(locationName, out result))
		{
			return true;
		}
		return false;
	}

	internal SubSceneEntry GetSubSceneInfo(Scene scene)
	{
		return subScenes.Find((SubSceneEntry e) => e != null && e.Info != null && e.Info.BuildIndex == scene.buildIndex);
	}

	public SubSceneEntry GetSubSceneInfo()
	{
		return cachedSubsceneEntry;
	}
}
