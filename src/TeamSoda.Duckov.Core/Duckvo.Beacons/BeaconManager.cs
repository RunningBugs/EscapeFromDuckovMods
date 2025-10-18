using System;
using System.Collections.Generic;
using System.Linq;
using Saves;
using UnityEngine;

namespace Duckvo.Beacons;

public class BeaconManager : MonoBehaviour
{
	[Serializable]
	public struct BeaconStatus
	{
		public string beaconID;

		public int beaconIndex;
	}

	[Serializable]
	public struct Data
	{
		public List<BeaconStatus> entries;
	}

	private Data data;

	public static Action<string, int> OnBeaconUnlocked;

	private const string SaveKey = "BeaconManager";

	public static BeaconManager Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		Load();
		SavesSystem.OnCollectSaveData += Save;
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= Save;
	}

	public void Load()
	{
		if (SavesSystem.KeyExisits("BeaconManager"))
		{
			data = SavesSystem.Load<Data>("BeaconManager");
		}
		if (data.entries == null)
		{
			data.entries = new List<BeaconStatus>();
		}
	}

	public void Save()
	{
		SavesSystem.Save("BeaconManager", data);
	}

	public static void UnlockBeacon(string id, int index)
	{
		if (!(Instance == null) && !GetBeaconUnlocked(id, index))
		{
			Instance.data.entries.Add(new BeaconStatus
			{
				beaconID = id,
				beaconIndex = index
			});
			OnBeaconUnlocked?.Invoke(id, index);
		}
	}

	public static bool GetBeaconUnlocked(string id, int index)
	{
		if (Instance == null)
		{
			return false;
		}
		return Instance.data.entries.Any((BeaconStatus e) => e.beaconID == id && e.beaconIndex == index);
	}
}
