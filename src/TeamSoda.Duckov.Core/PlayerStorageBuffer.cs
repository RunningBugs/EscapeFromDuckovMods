using System.Collections.Generic;
using ItemStatsSystem.Data;
using Saves;
using UnityEngine;

public class PlayerStorageBuffer : MonoBehaviour
{
	private const string bufferSaveKey = "PlayerStorage_Buffer";

	private static List<ItemTreeData> incomingItemBuffer = new List<ItemTreeData>();

	public static PlayerStorageBuffer Instance { get; private set; }

	public static List<ItemTreeData> Buffer => incomingItemBuffer;

	private void Awake()
	{
		Instance = this;
		LoadBuffer();
		SavesSystem.OnCollectSaveData += OnCollectSaveData;
	}

	private void OnCollectSaveData()
	{
		SaveBuffer();
	}

	public static void SaveBuffer()
	{
		List<ItemTreeData> list = new List<ItemTreeData>();
		foreach (ItemTreeData item in incomingItemBuffer)
		{
			if (item != null)
			{
				list.Add(item);
			}
		}
		SavesSystem.Save("PlayerStorage_Buffer", list);
	}

	public static void LoadBuffer()
	{
		incomingItemBuffer.Clear();
		List<ItemTreeData> list = SavesSystem.Load<List<ItemTreeData>>("PlayerStorage_Buffer");
		if (list != null)
		{
			if (list.Count <= 0)
			{
				Debug.Log("tree data is empty");
			}
			{
				foreach (ItemTreeData item in list)
				{
					incomingItemBuffer.Add(item);
				}
				return;
			}
		}
		Debug.Log("Tree Data is null");
	}
}
