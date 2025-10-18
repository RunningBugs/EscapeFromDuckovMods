using System.Collections.Generic;
using Duckov.Quests.Tasks;
using Duckov.Scenes;
using Duckov.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Duckov.Quests;

public class SpawnPrefabForTask : MonoBehaviour
{
	[SerializeField]
	private string componentID = "SpawnPrefabForTask";

	private Task _taskCache;

	[SerializeField]
	private List<MultiSceneLocation> locations;

	[SerializeField]
	private GameObject prefab;

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
		LevelManager.LevelInitializingComment = "Spawning prefabs for task";
		SpawnIfNeeded();
	}

	private void OnFinishedLoadingScene(SceneLoadingContext context)
	{
		SpawnIfNeeded();
	}

	private void SpawnIfNeeded()
	{
		if (!(prefab == null))
		{
			if (task == null)
			{
				Debug.LogWarning("未配置Task");
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
		if (!locations.GetRandom().TryGetLocationPosition(out var result))
		{
			return;
		}
		GameObject gameObject = Object.Instantiate(prefab, result, Quaternion.identity);
		QuestTask_TaskEvent questTask_TaskEvent = task as QuestTask_TaskEvent;
		if ((bool)questTask_TaskEvent)
		{
			TaskEventEmitter component = gameObject.GetComponent<TaskEventEmitter>();
			if ((bool)component)
			{
				component.SetKey(questTask_TaskEvent.EventKey);
			}
		}
		if ((bool)MultiSceneCore.Instance)
		{
			MultiSceneCore.MoveToActiveWithScene(gameObject, base.transform.gameObject.scene.buildIndex);
			MultiSceneCore.Instance.inLevelData[SpawnKey] = true;
		}
		spawned = true;
	}
}
