using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using ItemStatsSystem;
using ItemStatsSystem.Data;
using Saves;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

public class PlayerStorage : MonoBehaviour, IInitializedQueryHandler
{
	public class StorageCapacityCalculationHolder
	{
		public int capacity;
	}

	[SerializeField]
	private Inventory inventory;

	[SerializeField]
	private InteractableLootbox interactable;

	[SerializeField]
	private int defaultCapacity = 32;

	private static bool needRecalculateCapacity;

	private const string inventorySaveKey = "PlayerStorage";

	private bool initialized;

	public static PlayerStorage Instance { get; private set; }

	public static Inventory Inventory
	{
		get
		{
			if (Instance == null)
			{
				return null;
			}
			return Instance.inventory;
		}
	}

	public static List<ItemTreeData> IncomingItemBuffer => PlayerStorageBuffer.Buffer;

	public InteractableLootbox InteractableLootBox => interactable;

	public int DefaultCapacity => defaultCapacity;

	public static bool Loading { get; private set; }

	public static bool TakingItem { get; private set; }

	public static event Action<PlayerStorage, Inventory, int> OnPlayerStorageChange;

	public static event Action<StorageCapacityCalculationHolder> OnRecalculateStorageCapacity;

	public static event Action OnTakeBufferItem;

	public static event Action<Item> OnItemAddedToBuffer;

	public static event Action OnLoadingFinished;

	public static bool IsAccessableAndNotFull()
	{
		if (Instance == null)
		{
			return false;
		}
		if (Inventory == null)
		{
			return false;
		}
		return Inventory.GetFirstEmptyPosition() >= 0;
	}

	public static void NotifyCapacityDirty()
	{
		needRecalculateCapacity = true;
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		if (Instance != this)
		{
			Debug.LogError("发现了多个Player Storage!");
			return;
		}
		if (interactable == null)
		{
			interactable = GetComponent<InteractableLootbox>();
		}
		inventory.onContentChanged += OnInventoryContentChanged;
		SavesSystem.OnCollectSaveData += SavesSystem_OnCollectSaveData;
		LevelManager.RegisterWaitForInitialization(this);
	}

	private void Start()
	{
		Load().Forget();
	}

	private void OnDestroy()
	{
		inventory.onContentChanged -= OnInventoryContentChanged;
		SavesSystem.OnCollectSaveData -= SavesSystem_OnCollectSaveData;
		LevelManager.UnregisterWaitForInitialization(this);
	}

	private void SavesSystem_OnSetFile()
	{
		Load().Forget();
	}

	private void SavesSystem_OnCollectSaveData()
	{
		Save();
	}

	private void OnInventoryContentChanged(Inventory inventory, int index)
	{
		PlayerStorage.OnPlayerStorageChange?.Invoke(this, inventory, index);
	}

	public static void Push(Item item, bool toBufferDirectly = false)
	{
		if (item == null)
		{
			return;
		}
		if (!toBufferDirectly && Inventory != null)
		{
			if (item.Stackable)
			{
				while (item.StackCount > 0)
				{
					Item item2 = Inventory.FirstOrDefault((Item e) => e.TypeID == item.TypeID && e.MaxStackCount > e.StackCount);
					if (item2 == null)
					{
						break;
					}
					item2.Combine(item);
				}
			}
			if (item != null && item.StackCount > 0)
			{
				int firstEmptyPosition = Inventory.GetFirstEmptyPosition();
				if (firstEmptyPosition >= 0)
				{
					Inventory.AddAt(item, firstEmptyPosition);
					return;
				}
			}
		}
		NotificationText.Push("PlayerStorage_Notification_ItemAddedToBuffer".ToPlainText().Format(new
		{
			displayName = item.DisplayName
		}));
		IncomingItemBuffer.Add(ItemTreeData.FromItem(item));
		item.Detach();
		item.DestroyTree();
		PlayerStorage.OnItemAddedToBuffer?.Invoke(item);
	}

	private void Save()
	{
		if (!Loading)
		{
			inventory.Save("PlayerStorage");
		}
	}

	private async UniTask Load()
	{
		Loading = true;
		inventory.SetCapacity(16384);
		await ItemSavesUtilities.LoadInventory("PlayerStorage", inventory);
		RecalculateStorageCapacity();
		Loading = false;
		PlayerStorage.OnLoadingFinished?.Invoke();
		initialized = true;
	}

	private void Update()
	{
		if (needRecalculateCapacity)
		{
			RecalculateStorageCapacity();
		}
	}

	public static int RecalculateStorageCapacity()
	{
		if (Instance == null)
		{
			return 0;
		}
		StorageCapacityCalculationHolder storageCapacityCalculationHolder = new StorageCapacityCalculationHolder();
		storageCapacityCalculationHolder.capacity = Instance.DefaultCapacity;
		PlayerStorage.OnRecalculateStorageCapacity?.Invoke(storageCapacityCalculationHolder);
		int capacity = storageCapacityCalculationHolder.capacity;
		Instance.SetCapacity(capacity);
		needRecalculateCapacity = false;
		return capacity;
	}

	private void SetCapacity(int capacity)
	{
		inventory.SetCapacity(capacity);
	}

	public static async UniTask TakeBufferItem(int index)
	{
		if (!Loading && !TakingItem && index >= 0 && index <= IncomingItemBuffer.Count && IsAccessableAndNotFull())
		{
			ItemTreeData itemTreeData = IncomingItemBuffer[index];
			if (itemTreeData != null)
			{
				TakingItem = true;
				Item item = await ItemTreeData.InstantiateAsync(itemTreeData);
				Inventory.AddAndMerge(item);
				IncomingItemBuffer.RemoveAt(index);
				PlayerStorage.OnTakeBufferItem?.Invoke();
				TakingItem = false;
			}
		}
	}

	public bool HasInitialized()
	{
		return initialized;
	}
}
