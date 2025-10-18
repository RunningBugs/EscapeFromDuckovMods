using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using ItemStatsSystem.Data;
using UnityEngine;

namespace Saves;

public static class ItemSavesUtilities
{
	private const string InventoryPrefix = "Inventory/";

	private const string ItemPrefix = "Item/";

	public static void SaveAsLastDeadCharacter(Item item)
	{
		uint num = SavesSystem.Load<uint>("DeadCharacterToken");
		uint num2 = num;
		do
		{
			num2++;
		}
		while (num2 == num);
		SavesSystem.Save("DeadCharacterToken", num2);
		item.Save("LastDeadCharacter");
	}

	public static async UniTask<Item> LoadLastDeadCharacterItem()
	{
		return await LoadItem("LastDeadCharacter");
	}

	public static void Save(this Item item, string key)
	{
		ItemTreeData value = ItemTreeData.FromItem(item);
		SavesSystem.Save("Item/", key, value);
	}

	public static void Save(this Inventory inventory, string key)
	{
		InventoryData value = InventoryData.FromInventory(inventory);
		SavesSystem.Save("Inventory/", key, value);
	}

	public static async UniTask<Item> LoadItem(string key)
	{
		return await ItemTreeData.InstantiateAsync(SavesSystem.Load<ItemTreeData>("Item/", key));
	}

	public static async UniTask LoadInventory(string key, Inventory inventoryInstance)
	{
		if (!(inventoryInstance == null))
		{
			inventoryInstance.Loading = true;
			InventoryData inventoryData = SavesSystem.Load<InventoryData>("Inventory/", key);
			if (inventoryData == null)
			{
				Debug.LogWarning("Key Doesn't exist " + key + ", aborting operation");
				inventoryInstance.Loading = false;
			}
			else
			{
				inventoryInstance.DestroyAllContent();
				await InventoryData.LoadIntoInventory(inventoryData, inventoryInstance);
				inventoryInstance.Loading = false;
			}
		}
	}
}
