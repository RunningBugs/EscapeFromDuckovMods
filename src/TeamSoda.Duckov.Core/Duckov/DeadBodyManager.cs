using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.Rules;
using Duckov.Scenes;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Data;
using Saves;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Duckov;

public class DeadBodyManager : MonoBehaviour
{
	[Serializable]
	public class DeathInfo
	{
		public bool valid;

		public uint raidID;

		public string subSceneID;

		public Vector3 worldPosition;

		public ItemTreeData itemTreeData;

		public bool spawned;

		public bool touched;
	}

	private List<DeathInfo> deaths = new List<DeathInfo>();

	public static DeadBodyManager Instance { get; private set; }

	private void AppendDeathInfo(DeathInfo deathInfo)
	{
		while (deaths.Count >= GameRulesManager.Current.SaveDeadbodyCount)
		{
			deaths.RemoveAt(0);
		}
		deaths.Add(deathInfo);
		Save();
	}

	private static List<DeathInfo> LoadDeathInfos()
	{
		return SavesSystem.Load<List<DeathInfo>>("DeathList");
	}

	internal static void RecordDeath(CharacterMainControl mainCharacter)
	{
		if (Instance == null)
		{
			Debug.LogError("DeadBodyManager Instance is null");
			return;
		}
		DeathInfo deathInfo = new DeathInfo();
		deathInfo.valid = true;
		deathInfo.raidID = RaidUtilities.CurrentRaid.ID;
		deathInfo.subSceneID = MultiSceneCore.ActiveSubSceneID;
		deathInfo.worldPosition = mainCharacter.transform.position;
		deathInfo.itemTreeData = ItemTreeData.FromItem(mainCharacter.CharacterItem);
		Instance.AppendDeathInfo(deathInfo);
	}

	private void Awake()
	{
		Instance = this;
		MultiSceneCore.OnSubSceneLoaded += OnSubSceneLoaded;
		deaths.Clear();
		List<DeathInfo> list = LoadDeathInfos();
		if (list != null)
		{
			deaths.AddRange(list);
		}
		SavesSystem.OnCollectSaveData += Save;
	}

	private void OnDestroy()
	{
		MultiSceneCore.OnSubSceneLoaded -= OnSubSceneLoaded;
		SavesSystem.OnCollectSaveData -= Save;
	}

	private void Save()
	{
		SavesSystem.Save("DeathList", deaths);
	}

	private void OnSubSceneLoaded(MultiSceneCore core, Scene scene)
	{
		LevelManager.LevelInitializingComment = "Spawning bodies";
		if (!LevelConfig.SpawnTomb)
		{
			return;
		}
		foreach (DeathInfo death in deaths)
		{
			if (ShouldSpawnDeadBody(death))
			{
				SpawnDeadBody(death).Forget();
			}
		}
	}

	private async UniTask SpawnDeadBody(DeathInfo info)
	{
		Item item = await ItemTreeData.InstantiateAsync(info.itemTreeData);
		if (!(item == null))
		{
			Vector3 worldPosition = info.worldPosition;
			_ = info.subSceneID;
			InteractableLootbox.CreateFromItem(item, worldPosition, Quaternion.identity, moveToMainScene: true, GameplayDataSettings.Prefabs.LootBoxPrefab_Tomb, filterDontDropOnDead: true).OnInteractStartEvent.AddListener(delegate
			{
				NotifyDeadbodyTouched(info);
			});
			info.spawned = true;
		}
	}

	private static void NotifyDeadbodyTouched(DeathInfo info)
	{
		if (!(Instance == null))
		{
			Instance.OnDeadbodyTouched(info);
		}
	}

	private void OnDeadbodyTouched(DeathInfo info)
	{
		DeathInfo deathInfo = deaths.Find((DeathInfo e) => e.raidID == info.raidID);
		if (deathInfo != null)
		{
			deathInfo.touched = true;
		}
	}

	private bool ShouldSpawnDeadBody(DeathInfo info)
	{
		if (info == null)
		{
			return false;
		}
		if (!GameRulesManager.Current.SpawnDeadBody)
		{
			return false;
		}
		if (!LevelManager.Instance)
		{
			return false;
		}
		if (!LevelManager.Instance.IsRaidMap)
		{
			return false;
		}
		if (info == null)
		{
			return false;
		}
		if (!info.valid)
		{
			return false;
		}
		if (info.touched)
		{
			return false;
		}
		if (MultiSceneCore.ActiveSubSceneID != info.subSceneID)
		{
			return false;
		}
		return true;
	}
}
