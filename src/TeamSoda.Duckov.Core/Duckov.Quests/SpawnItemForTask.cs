using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.Scenes;
using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Duckov.Quests;

public class SpawnItemForTask : MonoBehaviour
{
	[SerializeField]
	private string componentID = "SpawnItemForTask";

	private Task _taskCache;

	[SerializeField]
	private List<MultiSceneLocation> locations;

	[ItemTypeID]
	[SerializeField]
	private int itemID = -1;

	[SerializeField]
	private MapElementForTask mapElement;

	private bool spawned;

	private Task task
	{
		get
		{
			if (_taskCache == null)
			{
				_taskCache = GetComponent<Task>();
			}
			return _taskCache;
		}
	}

	private int SpawnKey => string.Format("{0}/{1}/{2}/{3}", "SpawnPrefabForTask", task.Master.ID, task.ID, componentID).GetHashCode();

	private void Awake()
	{
		SceneLoader.onFinishedLoadingScene += OnFinishedLoadingScene;
		MultiSceneCore.OnSubSceneLoaded += OnSubSceneLoaded;
	}

	private void Start()
	{
		SpawnIfNeeded();
	}

	private void OnDestroy()
	{
		SceneLoader.onFinishedLoadingScene -= OnFinishedLoadingScene;
		MultiSceneCore.OnSubSceneLoaded -= OnSubSceneLoaded;
	}

	private void OnSubSceneLoaded(MultiSceneCore core, Scene scene)
	{
		LevelManager.LevelInitializingComment = "Spawning item for task";
		SpawnIfNeeded();
	}

	private void OnFinishedLoadingScene(SceneLoadingContext context)
	{
		SpawnIfNeeded();
	}

	private void SpawnIfNeeded()
	{
		if (itemID >= 0)
		{
			if (task == null)
			{
				Debug.Log("spawn item task is null");
			}
			else if (!task.IsFinished() && !IsSpawned())
			{
				Spawn();
			}
		}
	}

	private bool IsSpawned()
	{
		if (spawned)
		{
			return true;
		}
		if (MultiSceneCore.Instance == null)
		{
			return false;
		}
		if (!MultiSceneCore.Instance.inLevelData.TryGetValue(SpawnKey, out var value))
		{
			return false;
		}
		if (value is bool)
		{
			return (bool)value;
		}
		return false;
	}

	private void Spawn()
	{
		MultiSceneLocation random = locations.GetRandom();
		if (random.TryGetLocationPosition(out var result))
		{
			if ((bool)MultiSceneCore.Instance)
			{
				MultiSceneCore.Instance.inLevelData[SpawnKey] = true;
			}
			spawned = true;
			SpawnItem(result, base.transform.gameObject.scene, random).Forget();
		}
	}

	private async UniTaskVoid SpawnItem(Vector3 pos, Scene scene, MultiSceneLocation location)
	{
		Item item = await ItemAssetsCollection.InstantiateAsync(itemID);
		if (item == null)
		{
			return;
		}
		item.Drop(pos, createRigidbody: false, Vector3.zero, 0f);
		if ((bool)mapElement)
		{
			mapElement.SetVisibility(_visable: false);
			mapElement.locations.Clear();
			mapElement.locations.Add(location);
			if ((bool)task)
			{
				mapElement.name = task.Master.DisplayName;
			}
			mapElement.SetVisibility(_visable: true);
			item.onItemTreeChanged += OnItemTreeChanged;
		}
	}

	private void OnItemTreeChanged(Item selfItem)
	{
		if ((bool)mapElement && (bool)selfItem.ParentItem)
		{
			mapElement.SetVisibility(_visable: false);
			selfItem.onItemTreeChanged -= OnItemTreeChanged;
		}
	}
}
