using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.Economy;
using Duckov.MasterKeys;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;

public static class ItemUtilities
{
	private static Item CharacterItem => LevelManager.Instance?.MainCharacter?.CharacterItem;

	private static Inventory CharacterInventory => CharacterItem?.Inventory;

	private static Inventory PlayerStorageInventory => PlayerStorage.Inventory;

	public static event Action OnPlayerItemOperation;

	public static event Action<Item> OnItemSentToPlayerInventory;

	public static event Action<Item> OnItemSentToPlayerStorage;

	public static async UniTask<bool> Decompose(Item item, int count)
	{
		return await DecomposeDatabase.Decompose(item, count);
	}

	public static async UniTask<Item> GenerateBullet(Item gunItem)
	{
		Debug.Log("Trying to generate bullet for " + gunItem.DisplayName);
		if ((bool)gunItem.GetComponent<ItemSetting_Gun>())
		{
			string text = gunItem.Constants.GetString("Caliber");
			if (!string.IsNullOrEmpty(text))
			{
				ItemFilter filter = new ItemFilter
				{
					caliber = text,
					minQuality = 0,
					maxQuality = 1,
					requireTags = new Tag[1] { GameplayDataSettings.Tags.Get("Bullet") }
				};
				filter.caliber = text;
				int[] array = ItemAssetsCollection.Search(filter);
				if (array.Length != 0)
				{
					Item obj = await ItemAssetsCollection.InstantiateAsync(array.GetRandom());
					if (obj == null)
					{
						Debug.Log("Bullet generation failed for " + gunItem.DisplayName);
					}
					return obj;
				}
			}
		}
		Debug.Log("Bullet generation failed for " + gunItem.DisplayName);
		return null;
	}

	public static List<Item> FindAllBelongsToPlayer(Predicate<Item> predicate)
	{
		List<Item> list = new List<Item>();
		Inventory playerStorageInventory = PlayerStorageInventory;
		if (playerStorageInventory != null)
		{
			List<Item> collection = playerStorageInventory.FindAll(predicate);
			list.AddRange(collection);
		}
		Inventory characterInventory = CharacterInventory;
		if (characterInventory != null)
		{
			List<Item> collection2 = characterInventory.FindAll(predicate);
			list.AddRange(collection2);
		}
		Inventory inventory = LevelManager.Instance?.PetProxy?.Inventory;
		if (inventory != null)
		{
			List<Item> collection3 = inventory.FindAll(predicate);
			list.AddRange(collection3);
		}
		return list;
	}

	public static int GetItemCount(int typeID)
	{
		List<Item> list = FindAllBelongsToPlayer((Item e) => e != null && e.TypeID == typeID);
		int num = 0;
		foreach (Item item in list)
		{
			num += item.StackCount;
		}
		return num;
	}

	public static bool AddAndMerge(this Inventory inventory, Item item, int preferedFirstPosition = 0)
	{
		if (inventory == null)
		{
			return false;
		}
		if (item.Stackable)
		{
			while (item.StackCount > 0)
			{
				Item item2 = inventory.FirstOrDefault((Item e) => e.TypeID == item.TypeID && e.MaxStackCount > e.StackCount);
				if (item2 == null)
				{
					break;
				}
				item2.Combine(item);
			}
			if (item.StackCount <= 0)
			{
				return true;
			}
		}
		int firstEmptyPosition = inventory.GetFirstEmptyPosition(preferedFirstPosition);
		if (firstEmptyPosition < 0)
		{
			return false;
		}
		item.Detach();
		inventory.AddAt(item, firstEmptyPosition);
		return true;
	}

	public static bool SendToPlayerCharacter(Item item, bool dontMerge = false)
	{
		if (item == null)
		{
			return false;
		}
		Item item2 = LevelManager.Instance?.MainCharacter?.CharacterItem;
		if (item2 == null)
		{
			return false;
		}
		if (item2.TryPlug(item, emptyOnly: true))
		{
			ItemUtilities.OnPlayerItemOperation?.Invoke();
			return true;
		}
		return SendToPlayerCharacterInventory(item, dontMerge);
	}

	public static void SendToPlayer(Item item, bool dontMerge = false, bool sendToStorage = true)
	{
		if (!SendToPlayerCharacter(item, dontMerge))
		{
			if (sendToStorage)
			{
				SendToPlayerStorage(item);
			}
			else
			{
				item.Drop(CharacterMainControl.Main, createRigidbody: true);
			}
		}
	}

	public static bool SendToPlayerCharacterInventory(Item item, bool dontMerge = false)
	{
		if (item == null)
		{
			return false;
		}
		Inventory inventory = LevelManager.Instance?.MainCharacter?.CharacterItem?.Inventory;
		if (inventory == null)
		{
			return false;
		}
		int preferedFirstPosition = 0;
		bool flag = false;
		if (!((!dontMerge) ? inventory.AddAndMerge(item, preferedFirstPosition) : inventory.AddItem(item)))
		{
			return false;
		}
		ItemUtilities.OnPlayerItemOperation?.Invoke();
		ItemUtilities.OnItemSentToPlayerInventory?.Invoke(item);
		return true;
	}

	public static void SendToPlayerStorage(Item item, bool directToBuffer = false)
	{
		item.Detach();
		PlayerStorage.Push(item, directToBuffer);
		ItemUtilities.OnItemSentToPlayerStorage?.Invoke(item);
	}

	public static bool IsInPlayerCharacter(this Item item)
	{
		Item characterItem = LevelManager.Instance?.MainCharacter?.CharacterItem;
		if (characterItem == null)
		{
			return false;
		}
		return item.GetAllParents().Any((Item e) => e == characterItem);
	}

	public static bool IsInPlayerStorage(this Item item)
	{
		Inventory playerStorageInventory = PlayerStorage.Inventory;
		if (playerStorageInventory == null)
		{
			return false;
		}
		return item.GetAllParents().Any((Item e) => e.InInventory == playerStorageInventory);
	}

	public static bool IsRegistered(this Item item)
	{
		if (item == null)
		{
			return false;
		}
		if (MasterKeysManager.IsActive(item.TypeID))
		{
			return true;
		}
		if (CraftingManager.IsFormulaUnlocked(FormulasRegisterView.GetFormulaID(item)))
		{
			return true;
		}
		return false;
	}

	public static bool TryPlug(this Item main, Item part, bool emptyOnly = false, Inventory backupInventory = null, int preferredFirstIndex = 0)
	{
		if (main == null)
		{
			return false;
		}
		if (part == null)
		{
			return false;
		}
		if (main.Slots == null)
		{
			return false;
		}
		bool num = main.IsInPlayerCharacter();
		bool flag = part.IsInPlayerCharacter();
		bool flag2 = main.IsInPlayerStorage();
		bool flag3 = part.IsInPlayerStorage();
		bool flag4 = num || flag || flag2 || flag3;
		Slot slot = null;
		Slot pluggedIntoSlot = part.PluggedIntoSlot;
		if (backupInventory == null)
		{
			if ((bool)part.InInventory)
			{
				backupInventory = part.InInventory;
			}
			else if ((bool)main.InInventory)
			{
				backupInventory = main.InInventory;
			}
			else if (part.PluggedIntoSlot != null)
			{
				Item characterItem = part.GetCharacterItem();
				if (characterItem != null)
				{
					backupInventory = characterItem.Inventory;
				}
			}
			if (backupInventory == null)
			{
				Item characterItem2 = main.GetCharacterItem();
				if (characterItem2 != null)
				{
					backupInventory = characterItem2.Inventory;
				}
			}
		}
		IEnumerable<Slot> enumerable = main.Slots.Where((Slot e) => e?.CanPlug(part) ?? false);
		if (part.PluggedIntoSlot != null)
		{
			foreach (Slot item in enumerable)
			{
				if (part.PluggedIntoSlot == item)
				{
					Debug.Log("什么也没做，因为已经在这个物体上了。");
					return false;
				}
			}
		}
		if (part.Stackable)
		{
			foreach (Slot item2 in enumerable)
			{
				Item content = item2.Content;
				if (!(content == null) && content.TypeID == part.TypeID)
				{
					content.Combine(part);
					if (part.StackCount <= 0)
					{
						return true;
					}
				}
			}
		}
		Slot slot2 = enumerable.FirstOrDefault((Slot e) => e.Content == null);
		if (slot2 != null)
		{
			slot = slot2;
		}
		else if (!emptyOnly)
		{
			slot = enumerable.FirstOrDefault();
		}
		if (slot == null)
		{
			return false;
		}
		slot.Plug(part, out var unpluggedItem);
		if (unpluggedItem != null)
		{
			bool flag5 = false;
			if (pluggedIntoSlot != null && pluggedIntoSlot.Content == null)
			{
				flag5 = pluggedIntoSlot.Plug(unpluggedItem, out var _);
			}
			if (!flag5 && backupInventory != null)
			{
				flag5 = backupInventory.AddAndMerge(unpluggedItem, preferredFirstIndex);
			}
			if (!flag5)
			{
				if (flag4)
				{
					unpluggedItem.Drop(CharacterMainControl.Main, createRigidbody: true);
				}
				else
				{
					unpluggedItem.Drop(Vector3.down * 1000f, createRigidbody: false, Vector3.up, 0f);
				}
			}
		}
		return true;
	}

	public static CharacterMainControl GetCharacterMainControl(this Item item)
	{
		return item.GetRoot()?.GetComponentInParent<CharacterMainControl>();
	}

	internal static IEnumerable<Inventory> GetPlayerInventories()
	{
		HashSet<Inventory> hashSet = new HashSet<Inventory>();
		Inventory inventory = LevelManager.Instance?.MainCharacter?.CharacterItem?.Inventory;
		if ((bool)inventory)
		{
			hashSet.Add(inventory);
		}
		if (PlayerStorage.Inventory != null)
		{
			hashSet.Add(PlayerStorage.Inventory);
		}
		return hashSet;
	}

	internal static bool ConsumeItems(Cost cost)
	{
		List<Action> list = new List<Action>();
		List<Item> detachedItems = new List<Item>();
		if (cost.items != null)
		{
			Cost.ItemEntry[] items = cost.items;
			for (int i = 0; i < items.Length; i++)
			{
				Cost.ItemEntry cur = items[i];
				List<Item> items2 = FindAllBelongsToPlayer((Item e) => e != null && e.TypeID == cur.id);
				int count = Count(items2);
				if (count < cur.amount)
				{
					return false;
				}
				list.Add(delegate
				{
					long num = cur.amount;
					for (int j = 0; j < count; j++)
					{
						Item item = items2[j];
						if (!(item == null))
						{
							if (item.Slots != null)
							{
								foreach (Slot slot in item.Slots)
								{
									if (slot != null)
									{
										Item content = slot.Content;
										if (!(content == null))
										{
											content.Detach();
											detachedItems.Add(content);
										}
									}
								}
							}
							if (item.StackCount <= num)
							{
								num -= item.StackCount;
								item.Detach();
								item.DestroyTree();
							}
							else
							{
								item.StackCount -= (int)num;
								num = 0L;
							}
							if (num <= 0)
							{
								break;
							}
						}
					}
				});
			}
		}
		foreach (Action item2 in list)
		{
			item2();
		}
		foreach (Item item3 in detachedItems)
		{
			if (!(item3 == null))
			{
				SendToPlayer(item3, dontMerge: false, PlayerStorage.Inventory != null);
			}
		}
		ItemUtilities.OnPlayerItemOperation?.Invoke();
		return true;
	}

	internal static bool ConsumeItems(int itemTypeID, long amount)
	{
		List<Item> list = FindAllBelongsToPlayer((Item e) => e != null && e.TypeID == itemTypeID);
		if (Count(list) < amount)
		{
			return false;
		}
		List<Item> list2 = new List<Item>();
		long num = amount;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			Item item = list[num2];
			if (item == null)
			{
				continue;
			}
			if (item.Slots != null)
			{
				foreach (Slot slot in item.Slots)
				{
					if (slot != null)
					{
						Item content = slot.Content;
						if (!(content == null))
						{
							content.Detach();
							list2.Add(content);
						}
					}
				}
			}
			if (item.StackCount <= num)
			{
				num -= item.StackCount;
				item.Detach();
				item.DestroyTree();
			}
			else
			{
				item.StackCount -= (int)num;
				num = 0L;
			}
			if (num <= 0)
			{
				break;
			}
		}
		foreach (Item item2 in list2)
		{
			if (!(item2 == null))
			{
				SendToPlayer(item2, dontMerge: false, PlayerStorage.Inventory != null);
			}
		}
		return true;
	}

	internal static int Count(IEnumerable<Item> items)
	{
		int num = 0;
		foreach (Item item in items)
		{
			num = ((!item.Stackable) ? (num + 1) : (num + item.StackCount));
		}
		return num;
	}

	public static float GetRepairLossRatio(this Item item)
	{
		if (item == null)
		{
			return 0f;
		}
		float defaultResult = 0.14f;
		float num = item.Constants.GetFloat("RepairLossRatio", defaultResult);
		if (item.Tags.Contains("Weapon"))
		{
			float num2 = CharacterMainControl.WeaponRepairLossFactor();
			return num * num2;
		}
		float num3 = CharacterMainControl.EquipmentRepairLossFactor();
		return num * num3;
	}

	internal static void NotifyPlayerItemOperation()
	{
		ItemUtilities.OnPlayerItemOperation?.Invoke();
	}
}
