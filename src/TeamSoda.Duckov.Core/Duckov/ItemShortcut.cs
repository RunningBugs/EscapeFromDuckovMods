using System;
using System.Collections.Generic;
using System.Linq;
using ItemStatsSystem;
using Saves;
using UnityEngine;

namespace Duckov;

public class ItemShortcut : MonoBehaviour
{
	[Serializable]
	private class SaveData
	{
		[SerializeField]
		internal List<int> inventoryIndexes = new List<int>();

		public int Count => inventoryIndexes.Count;

		public void Generate(ItemShortcut shortcut)
		{
			inventoryIndexes.Clear();
			Inventory mainInventory = MainInventory;
			if (!(mainInventory == null))
			{
				for (int i = 0; i < shortcut.items.Count; i++)
				{
					Item item = shortcut.items[i];
					int index = mainInventory.GetIndex(item);
					inventoryIndexes.Add(index);
				}
			}
		}

		public void ApplyTo(ItemShortcut shortcut)
		{
			Inventory mainInventory = MainInventory;
			if (mainInventory == null)
			{
				return;
			}
			for (int i = 0; i < inventoryIndexes.Count; i++)
			{
				int num = inventoryIndexes[i];
				if (num >= 0)
				{
					Item itemAt = mainInventory.GetItemAt(num);
					shortcut.Set_Local(i, itemAt);
				}
			}
		}
	}

	public static ItemShortcut Instance;

	[SerializeField]
	private int maxIndex = 3;

	[SerializeField]
	private List<Item> items = new List<Item>();

	[SerializeField]
	private List<int> itemTypes = new List<int>();

	private const string SaveKey = "ItemShortcut_Data";

	private HashSet<int> dirtyIndexes = new HashSet<int>();

	private static CharacterMainControl Master => CharacterMainControl.Main;

	private static Inventory MainInventory
	{
		get
		{
			if (Master == null)
			{
				return null;
			}
			if (!Master.CharacterItem)
			{
				return null;
			}
			return Master.CharacterItem.Inventory;
		}
	}

	public static int MaxIndex
	{
		get
		{
			if (Instance == null)
			{
				return 0;
			}
			return Instance.maxIndex;
		}
	}

	public static event Action<int> OnSetItem;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("检测到多个ItemShortcut");
		}
		SavesSystem.OnCollectSaveData += OnCollectSaveData;
		SavesSystem.OnSetFile += OnSetSaveFile;
		LevelManager.OnLevelInitialized += OnLevelInitialized;
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= OnCollectSaveData;
		SavesSystem.OnSetFile -= OnSetSaveFile;
		LevelManager.OnLevelInitialized -= OnLevelInitialized;
	}

	private void Start()
	{
		Load();
	}

	private void OnLevelInitialized()
	{
		Load();
	}

	private void OnSetSaveFile()
	{
		Load();
	}

	private void OnCollectSaveData()
	{
		Save();
	}

	private void Load()
	{
		SavesSystem.Load<SaveData>("ItemShortcut_Data")?.ApplyTo(this);
	}

	private void Save()
	{
		SaveData saveData = new SaveData();
		saveData.Generate(this);
		SavesSystem.Save("ItemShortcut_Data", saveData);
	}

	public static bool IsItemValid(Item item)
	{
		if (item == null)
		{
			return false;
		}
		if (MainInventory == null)
		{
			return false;
		}
		if (MainInventory != item.InInventory)
		{
			return false;
		}
		if (item.Tags.Contains("Weapon"))
		{
			return false;
		}
		return true;
	}

	private bool Set_Local(int index, Item item)
	{
		if (Master == null)
		{
			return false;
		}
		if (index < 0 || index > maxIndex)
		{
			return false;
		}
		if (!IsItemValid(item))
		{
			return false;
		}
		while (items.Count <= index)
		{
			items.Add(null);
		}
		while (itemTypes.Count <= index)
		{
			itemTypes.Add(-1);
		}
		items[index] = item;
		itemTypes[index] = item.TypeID;
		ItemShortcut.OnSetItem?.Invoke(index);
		for (int i = 0; i < items.Count; i++)
		{
			if (i != index)
			{
				bool flag = false;
				if (items[i] == item)
				{
					items[i] = null;
					flag = true;
				}
				if (itemTypes[i] == item.TypeID)
				{
					itemTypes[i] = -1;
					items[i] = null;
					flag = true;
				}
				if (flag)
				{
					ItemShortcut.OnSetItem(i);
				}
			}
		}
		return true;
	}

	private Item Get_Local(int index)
	{
		if (index >= items.Count)
		{
			return null;
		}
		Item item = items[index];
		if (item == null)
		{
			item = MainInventory.Find(itemTypes[index]);
			if (item != null)
			{
				items[index] = item;
			}
		}
		if (!IsItemValid(item))
		{
			SetDirty(index);
			return null;
		}
		return item;
	}

	private void SetDirty(int index)
	{
		dirtyIndexes.Add(index);
	}

	private void Update()
	{
		if (dirtyIndexes.Count <= 0)
		{
			return;
		}
		int[] array = dirtyIndexes.ToArray();
		foreach (int num in array)
		{
			if (num < items.Count && !IsItemValid(items[num]))
			{
				items[num] = null;
				ItemShortcut.OnSetItem?.Invoke(num);
			}
		}
		dirtyIndexes.Clear();
	}

	public static Item Get(int index)
	{
		if (Instance == null)
		{
			return null;
		}
		return Instance.Get_Local(index);
	}

	public static bool Set(int index, Item item)
	{
		if (Instance == null)
		{
			return false;
		}
		return Instance.Set_Local(index, item);
	}
}
