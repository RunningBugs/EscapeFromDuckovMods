using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.Scenes;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractableLootbox : InteractableBase
{
	public enum LootBoxStates
	{
		closed,
		openning,
		looting
	}

	public bool useDefaultInteractName;

	[SerializeField]
	private bool showSortButton;

	[SerializeField]
	private bool usePages;

	public bool needInspect = true;

	public bool showPickAllButton = true;

	public Transform hideIfEmpty;

	[LocalizationKey("Default")]
	[SerializeField]
	private string displayNameKey;

	[SerializeField]
	private Inventory inventoryReference;

	private Item inspectingItem;

	private float inspectTime = 1f;

	private float inspectTimer;

	private LootBoxStates lootState;

	public bool ShowSortButton => showSortButton;

	public bool UsePages => usePages;

	public static Transform LootBoxInventoriesParent => LevelManager.LootBoxInventoriesParent;

	public static Dictionary<int, Inventory> Inventories => LevelManager.LootBoxInventories;

	public Inventory Inventory
	{
		get
		{
			Inventory inventory = null;
			if ((bool)inventoryReference)
			{
				inventory = inventoryReference;
			}
			else
			{
				inventory = GetOrCreateInventory(this);
				if (inventory == null)
				{
					if (LevelManager.Instance == null)
					{
						Debug.Log("LevelManager.Instance 不存在，取消创建i nventory");
						return null;
					}
					LevelManager.Instance.MainCharacter.PopText("空的Inventory");
					Debug.LogError("未能成功创建Inventory," + base.gameObject.name, this);
				}
				inventoryReference = inventory;
			}
			if ((bool)inventoryReference && inventoryReference.hasBeenInspectedInLootBox)
			{
				SetMarkerUsed();
			}
			inventory.DisplayNameKey = displayNameKey;
			return inventory;
		}
	}

	public bool Looted => LootView.HasInventoryEverBeenLooted(Inventory);

	public static InteractableLootbox Prefab => GameplayDataSettings.Prefabs?.LootBoxPrefab;

	public static event Action<InteractableLootbox> OnStartLoot;

	public static event Action<InteractableLootbox> OnStopLoot;

	public static Inventory GetOrCreateInventory(InteractableLootbox lootBox)
	{
		if (lootBox == null)
		{
			if (CharacterMainControl.Main != null)
			{
				CharacterMainControl.Main.PopText("ERROR:尝试创建Inventory, 但lootbox是null");
			}
			Debug.LogError("尝试创建Inventory, 但lootbox是null");
			return null;
		}
		int key = lootBox.GetKey();
		if (Inventories.TryGetValue(key, out var value))
		{
			if (!(value == null))
			{
				return value;
			}
			CharacterMainControl.Main.PopText($"Inventory缓存字典里有Key: {key}, 但其对应值为null.重新创建Inventory。");
			Debug.LogError($"Inventory缓存字典里有Key: {key}, 但其对应值为null.重新创建Inventory。");
		}
		GameObject obj = new GameObject($"Inventory_{key}");
		obj.transform.SetParent(LootBoxInventoriesParent);
		obj.transform.position = lootBox.transform.position;
		value = obj.AddComponent<Inventory>();
		value.NeedInspection = lootBox.needInspect;
		Inventories.Add(key, value);
		LootBoxLoader component = lootBox.GetComponent<LootBoxLoader>();
		if ((bool)component && component.autoSetup)
		{
			component.Setup().Forget();
		}
		return value;
	}

	private int GetKey()
	{
		Vector3 vector = base.transform.position * 10f;
		int x = Mathf.RoundToInt(vector.x);
		int y = Mathf.RoundToInt(vector.y);
		int z = Mathf.RoundToInt(vector.z);
		return new Vector3Int(x, y, z).GetHashCode();
	}

	protected override void Start()
	{
		base.Start();
		if (inventoryReference == null)
		{
			GetOrCreateInventory(this);
		}
		if ((bool)Inventory && Inventory.hasBeenInspectedInLootBox)
		{
			SetMarkerUsed();
		}
		overrideInteractName = true;
		base.InteractName = displayNameKey;
	}

	protected override bool IsInteractable()
	{
		if (Inventory == null)
		{
			if ((bool)CharacterMainControl.Main)
			{
				CharacterMainControl.Main.PopText("ERROR :( 存在不包含Inventory的Lootbox。");
			}
			return false;
		}
		if (lootState != LootBoxStates.closed)
		{
			return false;
		}
		return true;
	}

	protected override void OnUpdate(CharacterMainControl interactCharacter, float deltaTime)
	{
		if (Inventory == null)
		{
			StopInteract();
			if ((bool)LootView.Instance && LootView.Instance.open)
			{
				LootView.Instance.Close();
			}
			return;
		}
		switch (lootState)
		{
		case LootBoxStates.closed:
			StopInteract();
			break;
		case LootBoxStates.openning:
			if (interactCharacter.CurrentAction.ActionTimer >= base.InteractTime && !Inventory.Loading)
			{
				if (StartLoot())
				{
					lootState = LootBoxStates.looting;
					break;
				}
				CharacterMainControl.Main.PopText("ERROR :Start loot失败，终止交互。");
				StopInteract();
				lootState = LootBoxStates.closed;
			}
			break;
		case LootBoxStates.looting:
			if (!LootView.Instance || !LootView.Instance.open)
			{
				CharacterMainControl.Main.PopText("ERROR :打开Loot界面失败，终止交互。");
				StopInteract();
			}
			else if (inspectingItem != null)
			{
				inspectTimer += deltaTime;
				if (inspectTimer >= inspectTime)
				{
					inspectingItem.Inspected = true;
					inspectingItem.Inspecting = false;
				}
				if (!inspectingItem.Inspecting)
				{
					inspectingItem = null;
				}
			}
			else
			{
				Item item = FindFistUninspectedItem();
				if (!item)
				{
					StopInteract();
				}
				else
				{
					StartInspectItem(item);
				}
			}
			break;
		}
	}

	private void StartInspectItem(Item item)
	{
		if (!(item == null))
		{
			if (inspectingItem != null)
			{
				inspectingItem.Inspecting = false;
			}
			inspectingItem = item;
			inspectingItem.Inspecting = true;
			inspectTimer = 0f;
			inspectTime = GameplayDataSettings.LootingData.GetInspectingTime(item);
		}
	}

	private void UpdateInspect()
	{
	}

	private Item FindFistUninspectedItem()
	{
		if (!Inventory)
		{
			return null;
		}
		if (!Inventory.NeedInspection)
		{
			return null;
		}
		return Inventory.FirstOrDefault((Item e) => !e.Inspected);
	}

	protected override void OnInteractStart(CharacterMainControl interactCharacter)
	{
		lootState = LootBoxStates.openning;
	}

	protected override void OnInteractStop()
	{
		lootState = LootBoxStates.closed;
		InteractableLootbox.OnStopLoot?.Invoke(this);
		if (inspectingItem != null)
		{
			inspectingItem.Inspecting = false;
		}
		if ((bool)Inventory)
		{
			Inventory.hasBeenInspectedInLootBox = true;
		}
		SetMarkerUsed();
		CheckHideIfEmpty();
	}

	protected override void OnInteractFinished()
	{
		base.OnInteractFinished();
		if (inspectingItem != null)
		{
			inspectingItem.Inspecting = false;
		}
		CheckHideIfEmpty();
	}

	public void CheckHideIfEmpty()
	{
		if ((bool)hideIfEmpty && Inventory.IsEmpty())
		{
			hideIfEmpty.gameObject.SetActive(value: false);
		}
	}

	private bool StartLoot()
	{
		if (Inventory == null)
		{
			StopInteract();
			Debug.LogError("开始loot失败，缺少inventory。");
			return false;
		}
		InteractableLootbox.OnStartLoot?.Invoke(this);
		return true;
	}

	private void CreateLocalInventory()
	{
		Inventory inventory = base.gameObject.AddComponent<Inventory>();
		inventoryReference = inventory;
	}

	public static InteractableLootbox CreateFromItem(Item item, Vector3 position, Quaternion rotation, bool moveToMainScene = true, InteractableLootbox prefab = null, bool filterDontDropOnDead = false)
	{
		if (item == null)
		{
			Debug.LogError("正在尝试给一个不存在的Item创建LootBox，已取消。");
			return null;
		}
		if (prefab == null)
		{
			prefab = Prefab;
		}
		if (prefab == null)
		{
			Debug.LogError("未配置LootBox的Prefab");
			return null;
		}
		InteractableLootbox interactableLootbox = UnityEngine.Object.Instantiate(prefab, position, rotation);
		interactableLootbox.CreateLocalInventory();
		if (moveToMainScene)
		{
			MultiSceneCore.MoveToActiveWithScene(interactableLootbox.gameObject, SceneManager.GetActiveScene().buildIndex);
		}
		Inventory inventory = interactableLootbox.Inventory;
		if (inventory == null)
		{
			Debug.LogError("LootBox未配置Inventory");
			return interactableLootbox;
		}
		inventory.SetCapacity(512);
		List<Item> list = new List<Item>();
		if (item.Slots != null)
		{
			foreach (Slot slot in item.Slots)
			{
				Item content = slot.Content;
				if (!(content == null))
				{
					content.Inspected = true;
					if (content.Tags.Contains(GameplayDataSettings.Tags.DestroyOnLootBox))
					{
						content.DestroyTree();
					}
					if (!filterDontDropOnDead || (!content.Tags.Contains(GameplayDataSettings.Tags.DontDropOnDeadInSlot) && !content.Sticky))
					{
						list.Add(content);
					}
				}
			}
		}
		if (item.Inventory != null)
		{
			foreach (Item item2 in item.Inventory)
			{
				if (!(item2 == null) && !item2.Tags.Contains(GameplayDataSettings.Tags.DestroyOnLootBox))
				{
					list.Add(item2);
				}
			}
		}
		foreach (Item item3 in list)
		{
			item3.Detach();
			inventory.AddAndMerge(item3);
		}
		int capacity = Mathf.Max(8, inventory.GetLastItemPosition() + 1);
		inventory.SetCapacity(capacity);
		inventory.NeedInspection = prefab.needInspect;
		return interactableLootbox;
	}
}
