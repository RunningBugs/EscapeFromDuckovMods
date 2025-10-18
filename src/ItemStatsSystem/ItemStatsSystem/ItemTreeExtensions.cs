using System.Collections.Generic;
using ItemStatsSystem.Items;
using UnityEngine;

namespace ItemStatsSystem;

public static class ItemTreeExtensions
{
	public static List<Item> GetAllChildren(this Item item, bool includingGrandChildren = true, bool excludeSelf = false)
	{
		if (item == null)
		{
			return new List<Item>();
		}
		List<Item> children = new List<Item>();
		Stack<Item> pendingItems = new Stack<Item>();
		PushAllInSlots(item);
		PushAllInInventory(item);
		if (includingGrandChildren)
		{
			while (pendingItems.Count > 0)
			{
				Item item2 = pendingItems.Pop();
				PushAllInSlots(item2);
				PushAllInInventory(item2);
			}
		}
		if (!excludeSelf)
		{
			children.Add(item);
		}
		return children;
		void Push(Item pendingItem)
		{
			if (!(pendingItem == null))
			{
				if (pendingItem == item)
				{
					Debug.LogWarning("Item Loop Detected! Aborting!");
				}
				else
				{
					children.Add(pendingItem);
					pendingItems.Push(pendingItem);
				}
			}
		}
		void PushAllInInventory(Item item3)
		{
			if (item3 == null || item3.Inventory == null)
			{
				return;
			}
			foreach (Item item3 in item3.Inventory)
			{
				Push(item3);
			}
		}
		void PushAllInSlots(Item item3)
		{
			if (item3 == null || item3.Slots == null)
			{
				return;
			}
			foreach (Slot slot in item3.Slots)
			{
				Push(slot.Content);
			}
		}
	}

	public static List<Item> GetAllParents(this Item item, bool excludeSelf = false)
	{
		List<Item> list = new List<Item>();
		if (item == null)
		{
			return list;
		}
		Item parentItem = item.ParentItem;
		while (parentItem != null)
		{
			if (list.Contains(parentItem))
			{
				Debug.LogError("Item parenting loop detected!");
				break;
			}
			list.Add(parentItem);
			parentItem = parentItem.ParentItem;
		}
		if (!excludeSelf)
		{
			list.Insert(0, item);
		}
		return list;
	}

	public static Item GetRoot(this Item item)
	{
		if (item == null)
		{
			return null;
		}
		int num = 0;
		while (item.ParentItem != null)
		{
			item = item.ParentItem;
			num++;
			if (num >= 32)
			{
				Debug.LogError("Too much layers in Item. Check if item reference loop occurred!");
				break;
			}
		}
		return item;
	}

	public static void DestroyTree(this Item item)
	{
		if (!(item == null))
		{
			item.MarkDestroyed();
			if (Application.isPlaying)
			{
				Object.Destroy(item.gameObject);
			}
			else
			{
				Object.DestroyImmediate(item.gameObject);
			}
		}
	}

	public static void DestroyTreeImmediate(this Item item)
	{
		if (!(item == null))
		{
			Object.DestroyImmediate(item.gameObject);
		}
	}

	public static List<Item> GetAllConnected(this Item item)
	{
		if (item == null)
		{
			return null;
		}
		return item.GetRoot().GetAllChildren();
	}
}
