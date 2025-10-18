using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.Scenes;
using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ItemExtensions
{
	public static readonly int PickupHash = "Pickup".GetHashCode();

	public static readonly int HandheldHash = "Handheld".GetHashCode();

	private static ItemAgent CreatePickupAgent(this Item itemInstance, Vector3 pos)
	{
		if (itemInstance.ActiveAgent != null)
		{
			Debug.LogError("创建pickup agent失败,已有agent:" + itemInstance.ActiveAgent.name);
			return null;
		}
		ItemAgent itemAgent = itemInstance.AgentUtilities.GetPrefab(PickupHash);
		if (itemAgent == null)
		{
			itemAgent = GameplayDataSettings.Prefabs.PickupAgentPrefab;
		}
		ItemAgent itemAgent2 = itemInstance.AgentUtilities.CreateAgent(itemAgent, ItemAgent.AgentTypes.pickUp);
		itemAgent2.transform.position = pos;
		return itemAgent2;
	}

	public static ItemAgent CreateHandheldAgent(this Item itemInstance)
	{
		if (itemInstance.ActiveAgent != null)
		{
			Debug.LogError("创建pickup agent失败,已有agent");
			return null;
		}
		ItemAgent itemAgent = itemInstance.AgentUtilities.GetPrefab(HandheldHash);
		if (itemAgent == null)
		{
			itemAgent = GameplayDataSettings.Prefabs.HandheldAgentPrefab;
		}
		return itemInstance.AgentUtilities.CreateAgent(itemAgent, ItemAgent.AgentTypes.handheld);
	}

	public static DuckovItemAgent Drop(this Item item, Vector3 pos, bool createRigidbody, Vector3 dropDirection, float randomAngle)
	{
		if (item == null)
		{
			Debug.Log("尝试丢弃不存在的物体");
			return null;
		}
		item.Detach();
		if (MultiSceneCore.MainScene.HasValue)
		{
			item.gameObject.transform.SetParent(null);
			SceneManager.MoveGameObjectToScene(item.gameObject, MultiSceneCore.MainScene.Value);
		}
		ItemAgent itemAgent = item.CreatePickupAgent(pos);
		if ((bool)MultiSceneCore.Instance)
		{
			if (itemAgent == null)
			{
				Debug.Log("创建的agent是null");
			}
			MultiSceneCore.MoveToActiveWithScene(itemAgent.gameObject, SceneManager.GetActiveScene().buildIndex);
		}
		InteractablePickup component = itemAgent.GetComponent<InteractablePickup>();
		if (createRigidbody && component != null)
		{
			component.Throw(dropDirection, randomAngle);
		}
		else
		{
			component.transform.rotation = Quaternion.Euler(0f, Random.Range(0f - randomAngle, randomAngle) * 0.5f, 0f);
		}
		return itemAgent as DuckovItemAgent;
	}

	public static void Drop(this Item item, CharacterMainControl character, bool createRigidbody)
	{
		if (!(item == null))
		{
			Vector3 vector = Random.insideUnitSphere * 1f;
			vector.y = 0f;
			item.Drop(character.transform.position, createRigidbody, character.CurrentAimDirection, 45f);
			if (character.IsMainCharacter && LevelManager.LevelInited)
			{
				AudioManager.Post("SFX/Item/put_" + item.SoundKey, character.gameObject);
			}
		}
	}

	public static async UniTask<List<Item>> GetItemsOfAmount(this Inventory inventory, int itemTypeID, int amount)
	{
		List<Item> list = inventory.FindAll((Item e) => e != null && e.TypeID == itemTypeID);
		List<Item> result = new List<Item>();
		int count = 0;
		foreach (Item item in list)
		{
			int remainingCount = amount - count;
			if (item.StackCount > remainingCount)
			{
				result.Add(await item.Split(remainingCount));
				count += remainingCount;
			}
			else
			{
				item.Detach();
				result.Add(item);
				count += item.StackCount;
			}
			if (count >= amount)
			{
				break;
			}
		}
		return result;
	}

	public static bool TryFindItemsOfAmount(this IEnumerable<Inventory> inventories, int itemTypeID, int requiredAmount, out List<Item> result)
	{
		result = new List<Item>();
		int num = 0;
		foreach (Inventory inventory in inventories)
		{
			foreach (Item item in inventory)
			{
				if (item.TypeID == itemTypeID)
				{
					result.Add(item);
					num += item.StackCount;
					if (num >= requiredAmount)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static void ConsumeItemsOfAmount(this IEnumerable<Item> itemsToBeConsumed, int amount)
	{
		List<Item> list = new List<Item>();
		int num = 0;
		foreach (Item item2 in itemsToBeConsumed)
		{
			list.Add(item2);
			num += item2.StackCount;
			if (num >= amount)
			{
				break;
			}
		}
		num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			Item item = list[i];
			int num2 = amount - num;
			if (num2 >= item.StackCount)
			{
				item.Detach();
				item.DestroyTree();
				continue;
			}
			item.StackCount -= num2;
			break;
		}
	}

	private static bool TryMerge(IEnumerable<Item> itemsOfSameTypeID, out List<Item> result)
	{
		result = null;
		List<Item> list = itemsOfSameTypeID.ToList();
		list.RemoveAll((Item e) => e == null);
		if (list.Count <= 0)
		{
			return false;
		}
		int typeID = list[0].TypeID;
		foreach (Item item3 in list)
		{
			if (typeID != item3.TypeID)
			{
				Debug.LogError("尝试融合的Item具有不同的TypeID,已取消");
				return false;
			}
		}
		if (!list[0].Stackable)
		{
			Debug.LogError("此类物品不可堆叠，已取消");
			return false;
		}
		result = new List<Item>();
		Stack<Item> stack = new Stack<Item>(list);
		Item item = null;
		while (stack.Count > 0)
		{
			if (item == null)
			{
				item = stack.Pop();
			}
			if (stack.Count <= 0)
			{
				result.Add(item);
				break;
			}
			Item item2 = null;
			while (item.StackCount < item.MaxStackCount && stack.Count > 0)
			{
				item2 = stack.Pop();
				item.Combine(item2);
			}
			result.Add(item);
			if (item2 != null && item2.StackCount > 0)
			{
				if (stack.Count <= 0)
				{
					result.Add(item2);
					break;
				}
				item = item2;
			}
			else
			{
				item = null;
			}
		}
		return true;
	}
}
