using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using UnityEngine;

namespace Duckov.Economy;

[Serializable]
public struct Cost
{
	[Serializable]
	public struct ItemEntry
	{
		[ItemTypeID]
		public int id;

		public long amount;
	}

	public long money;

	public ItemEntry[] items;

	private static List<object> ReturnTaskLocks = new List<object>();

	public bool Enough => EconomyManager.IsEnough(this);

	public bool IsFree
	{
		get
		{
			if (money > 0)
			{
				return false;
			}
			if (items != null && items.Length != 0)
			{
				return false;
			}
			return true;
		}
	}

	public static bool TaskPending => ReturnTaskLocks.Count > 0;

	public bool Pay(bool accountAvaliable = true, bool cashAvaliable = true)
	{
		return EconomyManager.Pay(this, accountAvaliable, cashAvaliable);
	}

	public static Cost FromString(string costDescription)
	{
		int num = 0;
		List<ItemEntry> list = new List<ItemEntry>();
		string[] array = costDescription.Split(',');
		foreach (string text in array)
		{
			string[] array2 = text.Split(":");
			if (array2.Length != 2)
			{
				Debug.LogError("Invalid cost description: " + text + "\n" + costDescription);
				continue;
			}
			string text2 = array2[0].Trim();
			if (!int.TryParse(array2[1].Trim(), out var result))
			{
				Debug.LogError("Invalid cost description: " + text);
				continue;
			}
			if (text2 == "money")
			{
				num = result;
				continue;
			}
			int num2 = ItemAssetsCollection.TryGetIDByName(text2);
			if (num2 <= 0)
			{
				Debug.LogError("Invalid item name " + text2);
				continue;
			}
			list.Add(new ItemEntry
			{
				id = num2,
				amount = result
			});
		}
		return new Cost
		{
			money = num,
			items = list.ToArray()
		};
	}

	internal async UniTask Return(bool directToBuffer = false, bool toPlayerInventory = false, int amountFactor = 1, List<Item> generatedItemsBuffer = null)
	{
		object taskLock = new object();
		ReturnTaskLocks.Add(taskLock);
		List<Item> generatedItems = new List<Item>();
		ItemEntry[] array = items;
		for (int i = 0; i < array.Length; i++)
		{
			ItemEntry item = array[i];
			long count = item.amount * amountFactor;
			while (count > 0)
			{
				Item item2 = await ItemAssetsCollection.InstantiateAsync(item.id);
				if (item2.Stackable)
				{
					if (count > item2.MaxStackCount)
					{
						item2.StackCount = item2.MaxStackCount;
					}
					else
					{
						item2.StackCount = (int)count;
					}
					if (item2.StackCount <= 0)
					{
						Debug.LogError($"物品{item2.DisplayName}({item2.TypeID})的StackCount为{item2.StackCount},请检查");
						count--;
					}
					else
					{
						count -= item2.StackCount;
					}
				}
				else
				{
					count--;
				}
				generatedItems.Add(item2);
			}
		}
		foreach (Item item3 in generatedItems)
		{
			if (!toPlayerInventory || !ItemUtilities.SendToPlayerCharacterInventory(item3))
			{
				ItemUtilities.SendToPlayerStorage(item3, directToBuffer);
			}
		}
		generatedItemsBuffer?.AddRange(generatedItems);
		EconomyManager.Add(money * amountFactor);
		ReturnTaskLocks.Remove(taskLock);
	}

	public Cost(long money, (int id, long amount)[] items)
	{
		this.money = money;
		this.items = new ItemEntry[items.Length];
		for (int i = 0; i < items.Length; i++)
		{
			(int, long) tuple = items[i];
			ItemEntry[] array = this.items;
			int num = i;
			ItemEntry itemEntry = default(ItemEntry);
			(itemEntry.id, itemEntry.amount) = tuple;
			array[num] = itemEntry;
		}
	}

	public Cost(long money)
	{
		this.money = money;
		items = new ItemEntry[0];
	}

	public Cost(params (int id, long amount)[] items)
	{
		money = 0L;
		this.items = new ItemEntry[items.Length];
		for (int i = 0; i < items.Length; i++)
		{
			(int, long) tuple = items[i];
			ItemEntry[] array = this.items;
			int num = i;
			ItemEntry itemEntry = default(ItemEntry);
			(itemEntry.id, itemEntry.amount) = tuple;
			array[num] = itemEntry;
		}
	}
}
