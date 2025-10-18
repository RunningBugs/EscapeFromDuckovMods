using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.Utilities;
using ItemStatsSystem.Items;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ItemStatsSystem.Data;

[Serializable]
public class ItemTreeData
{
	[Serializable]
	public class DataEntry
	{
		public int instanceID;

		public int typeID;

		public List<CustomData> variables = new List<CustomData>();

		public List<SlotInstanceIDPair> slotContents = new List<SlotInstanceIDPair>();

		public List<InventoryDataEntry> inventory = new List<InventoryDataEntry>();

		public List<int> inventorySortLocks = new List<int>();

		public string TypeName => $"TYPE_{typeID}";

		public int StackCount
		{
			get
			{
				CustomData customData = variables.Find((CustomData e) => e.Key == "Count");
				if (customData == null)
				{
					return 1;
				}
				if (customData.DataType != CustomDataType.Int)
				{
					return 1;
				}
				return customData.GetInt();
			}
		}
	}

	public class SlotInstanceIDPair
	{
		public string slot;

		public int instanceID;

		public SlotInstanceIDPair(string slot, int instanceID)
		{
			this.slot = slot;
			this.instanceID = instanceID;
		}
	}

	public class InventoryDataEntry
	{
		public int position;

		public int instanceID;

		public InventoryDataEntry(int position, int instanceID)
		{
			this.position = position;
			this.instanceID = instanceID;
		}
	}

	public int rootInstanceID;

	public List<DataEntry> entries = new List<DataEntry>();

	public DataEntry RootData
	{
		get
		{
			DataEntry dataEntry = entries.Find((DataEntry e) => e.instanceID == rootInstanceID);
			if (dataEntry == null)
			{
				return null;
			}
			return dataEntry;
		}
	}

	public int RootTypeID => entries.Find((DataEntry e) => e.instanceID == rootInstanceID)?.typeID ?? 0;

	public static event Action<Item> OnItemLoaded;

	public static ItemTreeData FromItem(Item item)
	{
		ItemTreeData itemTreeData = new ItemTreeData();
		Dictionary<int, DataEntry> dictionary = new Dictionary<int, DataEntry>();
		itemTreeData.rootInstanceID = item.GetInstanceID();
		List<Item> allChildren = item.GetAllChildren();
		foreach (Item item3 in allChildren)
		{
			DataEntry dataEntry = new DataEntry
			{
				instanceID = item3.GetInstanceID(),
				typeID = item3.TypeID
			};
			foreach (CustomData variable in item3.Variables)
			{
				dataEntry.variables.Add(new CustomData(variable));
			}
			if (item3.Inventory != null)
			{
				int lastItemPosition = item3.Inventory.GetLastItemPosition();
				for (int i = 0; i <= lastItemPosition; i++)
				{
					Item item2 = item3.Inventory[i];
					if (item2 != null)
					{
						dataEntry.inventory.Add(new InventoryDataEntry(i, item2.GetInstanceID()));
					}
				}
				dataEntry.inventorySortLocks = new List<int>(item3.Inventory.lockedIndexes);
			}
			dictionary.Add(dataEntry.instanceID, dataEntry);
			itemTreeData.entries.Add(dataEntry);
		}
		foreach (Item item4 in allChildren)
		{
			DataEntry dataEntry2 = dictionary[item4.GetInstanceID()];
			if (item4.Slots == null)
			{
				continue;
			}
			foreach (Slot slot in item4.Slots)
			{
				if (slot.Content != null)
				{
					dataEntry2.slotContents.Add(new SlotInstanceIDPair(slot.Key, slot.Content.GetInstanceID()));
				}
			}
		}
		return itemTreeData;
	}

	public static async UniTask<Item> InstantiateAsync(ItemTreeData data)
	{
		if (data == null)
		{
			return null;
		}
		Dictionary<int, Item> instanceDic = new Dictionary<int, Item>();
		Scene beginningScene = SceneManager.GetActiveScene();
		bool beginningSceneLoaded = beginningScene.isLoaded;
		bool playing = Application.isPlaying;
		bool abort = false;
		foreach (DataEntry curData in data.entries)
		{
			if ((beginningSceneLoaded && !beginningScene.isLoaded) || Application.isPlaying != playing)
			{
				abort = true;
				break;
			}
			Item item = await ItemAssetsCollection.InstantiateAsync(curData.typeID);
			if (item == null)
			{
				Debug.LogError($"Failed to create item {data.rootInstanceID}, type:{curData.typeID}");
				continue;
			}
			instanceDic.Add(curData.instanceID, item);
			foreach (CustomData variable in curData.variables)
			{
				item.Variables.SetRaw(variable.Key, variable.DataType, variable.GetRawCopied());
			}
			ItemTreeData.OnItemLoaded?.Invoke(item);
		}
		if (abort)
		{
			Debug.LogWarning("Item Instantiate Aborted");
			Item[] array = instanceDic.Values.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				UnityEngine.Object.Destroy(array[i].gameObject);
			}
			return null;
		}
		foreach (DataEntry entry in data.entries)
		{
			if (!instanceDic.TryGetValue(entry.instanceID, out var value))
			{
				continue;
			}
			foreach (SlotInstanceIDPair slotContent in entry.slotContents)
			{
				if (value.Slots == null)
				{
					Debug.LogError($"Trying to plug item to slot {value.name}({value.TypeID}-{value.DisplayName})/{slotContent.slot}, but the slot doesn't exist.");
					break;
				}
				Slot slot = value.Slots[slotContent.slot];
				instanceDic.TryGetValue(slotContent.instanceID, out var value2);
				if (slot != null && !(value2 == null))
				{
					slot.Plug(value2, out var unpluggedItem);
					if (unpluggedItem != null)
					{
						Debug.LogError("Found Unplugged Item while Loading Item Tree!");
					}
				}
			}
			if (entry.inventory.Count > 0)
			{
				if (value.Inventory == null)
				{
					Debug.LogError("尝试加载Inventory数据，但物品的Inventory不存在。");
				}
				else
				{
					foreach (InventoryDataEntry item2 in entry.inventory)
					{
						if (instanceDic.TryGetValue(item2.instanceID, out var value3))
						{
							value.Inventory.AddAt(value3, item2.position);
						}
						else
						{
							Debug.LogError($"加载Inventory时找不到物品实例 {item2.instanceID}");
						}
					}
				}
			}
			if ((bool)value.Inventory && entry.inventorySortLocks != null)
			{
				value.Inventory.lockedIndexes.Clear();
				value.Inventory.lockedIndexes.AddRange(entry.inventorySortLocks);
			}
		}
		if (instanceDic.TryGetValue(data.rootInstanceID, out var value4))
		{
			return value4;
		}
		Debug.LogError($"Missing Item {data.rootInstanceID} \n {data.ToString()}");
		return null;
	}

	public DataEntry GetEntry(int instanceID)
	{
		return entries.Find((DataEntry e) => e.instanceID == instanceID);
	}

	public override string ToString()
	{
		DataEntry dataEntry = entries.Find((DataEntry e) => e.instanceID == rootInstanceID);
		if (dataEntry == null)
		{
			Debug.LogError("No Root Entry in Tree");
			return "Invalid Item Tree";
		}
		int indent = 0;
		string result = "";
		PrintEntry(dataEntry);
		return result;
		void MakeIndent()
		{
			result += new string('\t', indent);
		}
		void Print(string content)
		{
			result += content;
		}
		void PrintEntry(DataEntry dataEntry2)
		{
			MakeIndent();
			PrintLine($"{dataEntry2.typeID}-{dataEntry2.TypeName} ({dataEntry2.instanceID})");
			indent++;
			if (dataEntry2.slotContents.Count > 0)
			{
				Print("[Slots]\n");
			}
			foreach (SlotInstanceIDPair slotContent in dataEntry2.slotContents)
			{
				DataEntry entry = GetEntry(slotContent.instanceID);
				MakeIndent();
				Print(slotContent.slot + ":\n");
				PrintEntry(entry);
			}
			if (dataEntry2.inventory.Count > 0)
			{
				Print("[Inventory]\n");
			}
			foreach (InventoryDataEntry item in dataEntry2.inventory)
			{
				DataEntry entry2 = GetEntry(item.instanceID);
				MakeIndent();
				PrintEntry(entry2);
			}
			indent--;
		}
		void PrintLine(string content)
		{
			result = result + content + "\n";
		}
	}
}
