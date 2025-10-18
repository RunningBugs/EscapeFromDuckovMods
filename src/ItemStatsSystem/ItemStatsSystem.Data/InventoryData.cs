using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ItemStatsSystem.Data;

[Serializable]
public class InventoryData
{
	[Serializable]
	public class Entry
	{
		public int inventoryPosition;

		public ItemTreeData itemTreeData;
	}

	public int capacity = 16;

	public List<Entry> entries = new List<Entry>();

	public List<int> lockedIndexes = new List<int>();

	public static InventoryData FromInventory(Inventory inventory)
	{
		InventoryData inventoryData = new InventoryData();
		inventoryData.capacity = inventory.Capacity;
		int lastItemPosition = inventory.GetLastItemPosition();
		for (int i = 0; i <= lastItemPosition; i++)
		{
			Item itemAt = inventory.GetItemAt(i);
			if (!(itemAt == null))
			{
				Entry entry = new Entry();
				entry.inventoryPosition = i;
				entry.itemTreeData = ItemTreeData.FromItem(itemAt);
				inventoryData.entries.Add(entry);
			}
		}
		inventoryData.lockedIndexes = new List<int>(inventory.lockedIndexes);
		return inventoryData;
	}

	public static async UniTask LoadIntoInventory(InventoryData data, Inventory inventoryInstance)
	{
		if (data == null)
		{
			return;
		}
		foreach (Entry entry in data.entries)
		{
			int position = entry.inventoryPosition;
			Item item = await ItemTreeData.InstantiateAsync(entry.itemTreeData);
			if (item == null)
			{
				Debug.LogError("物品加载失败");
			}
			else if (!inventoryInstance.AddAt(item, position))
			{
				Debug.LogError("向 Inventory " + inventoryInstance.name + " 中添加物品失败。");
			}
		}
		if (data.lockedIndexes != null)
		{
			inventoryInstance.lockedIndexes.Clear();
			inventoryInstance.lockedIndexes.AddRange(data.lockedIndexes);
		}
	}
}
