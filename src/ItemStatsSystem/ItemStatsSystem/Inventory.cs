using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Duckov.Utilities;
using Sirenix.OdinInspector;
using SodaCraft.Localizations;
using UnityEngine;

namespace ItemStatsSystem;

public class Inventory : MonoBehaviour, ISelfValidator, IEnumerable<Item>, IEnumerable
{
	private bool loading;

	[LocalizationKey("Default")]
	[SerializeField]
	private string displayNameKey = "";

	private const string defaultDisplayNameKey = "UI_InventoryDefault";

	[SerializeField]
	private int defaultCapacity = 64;

	[SerializeField]
	private Item attachedToItem;

	[SerializeField]
	private List<Item> content = new List<Item>();

	[SerializeField]
	private bool needInspection;

	[SerializeField]
	private bool acceptSticky;

	private const bool TrimListWhenRemovingItem = true;

	public bool hasBeenInspectedInLootBox;

	[SerializeField]
	public List<int> lockedIndexes = new List<int>();

	private float? cachedWeight;

	public bool Loading
	{
		get
		{
			return loading;
		}
		set
		{
			loading = value;
		}
	}

	public string DisplayNameKey
	{
		get
		{
			if (string.IsNullOrWhiteSpace(displayNameKey))
			{
				return "UI_InventoryDefault";
			}
			return displayNameKey;
		}
		set
		{
			displayNameKey = value;
		}
	}

	public string DisplayName => DisplayNameKey.ToPlainText();

	public List<Item> Content => content;

	public bool AcceptSticky
	{
		get
		{
			return acceptSticky;
		}
		set
		{
			acceptSticky = value;
		}
	}

	public bool NeedInspection
	{
		get
		{
			return needInspection;
		}
		set
		{
			needInspection = value;
		}
	}

	public int Capacity => defaultCapacity;

	public Item AttachedToItem
	{
		get
		{
			return attachedToItem;
		}
		internal set
		{
			attachedToItem = value;
		}
	}

	public Item this[int index] => GetItemAt(index);

	public float CachedWeight
	{
		get
		{
			if (!cachedWeight.HasValue)
			{
				RecalculateWeight();
			}
			return cachedWeight.Value;
		}
	}

	public event Action<Inventory, int> onContentChanged;

	public event Action<Inventory> onInventorySorted;

	public event Action<Inventory> onCapacityChanged;

	public event Action<Inventory, int> onSetIndexLock;

	public void LockIndex(int index)
	{
		if (!lockedIndexes.Contains(index))
		{
			lockedIndexes.Add(index);
			this.onSetIndexLock?.Invoke(this, index);
		}
	}

	public void UnlockIndex(int index)
	{
		lockedIndexes.RemoveAll((int e) => e == index);
		this.onSetIndexLock?.Invoke(this, index);
	}

	public bool IsIndexLocked(int index)
	{
		return lockedIndexes.Contains(index);
	}

	public void ToggleLockIndex(int index)
	{
		if (IsIndexLocked(index))
		{
			UnlockIndex(index);
		}
		else
		{
			LockIndex(index);
		}
	}

	private void Start()
	{
		using IEnumerator<Item> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Item current = enumerator.Current;
			if (!(current == null) && current.ParentItem != this)
			{
				current.NotifyAddedToInventory(this);
			}
		}
	}

	public bool IsEmpty()
	{
		foreach (Item item in content)
		{
			if (item != null)
			{
				return false;
			}
		}
		return true;
	}

	public void Sort(Comparison<Item> comparison)
	{
		content.Sort(comparison);
	}

	[ContextMenu("Sort")]
	public void Sort()
	{
		if (Loading)
		{
			return;
		}
		Loading = true;
		List<Item> list = new List<Item>();
		for (int i = 0; i < content.Count; i++)
		{
			if (!IsIndexLocked(i))
			{
				Item item = content[i];
				if (!(item == null))
				{
					item.Detach();
					list.Add(item);
				}
			}
		}
		List<IGrouping<Tag, Item>> list2 = (from item2 in list
			where item2 != null
			group item2 by GetFirstTag(item2)).ToList();
		list2.Sort(delegate(IGrouping<Tag, Item> g1, IGrouping<Tag, Item> g2)
		{
			Tag key = g1.Key;
			Tag key2 = g2.Key;
			int num = ((key != null) ? key.Priority : (-1));
			int num2 = ((key2 != null) ? key2.Priority : (-1));
			return (num != num2) ? (num - num2) : string.Compare(key.name, key2.name, StringComparison.OrdinalIgnoreCase);
		});
		List<Item> list3 = new List<Item>();
		foreach (IGrouping<Tag, Item> item4 in list2)
		{
			List<IGrouping<int, Item>> list4 = (from item2 in item4
				group item2 by item2.TypeID).ToList();
			list4.Sort(delegate(IGrouping<int, Item> a, IGrouping<int, Item> b)
			{
				Item item2 = a.First();
				Item item3 = b.First();
				return (item2.Order == item3.Order) ? (a.Key - b.Key) : (item2.Order - item3.Order);
			});
			foreach (IGrouping<int, Item> item5 in list4)
			{
				if (item5.First().Stackable && TryMerge(item5, out var result))
				{
					list3.AddRange(result);
				}
				else
				{
					list3.AddRange(item5);
				}
			}
		}
		_ = content.Count;
		foreach (Item item6 in list3)
		{
			AddItem(item6);
		}
		Loading = false;
		this.onInventorySorted?.Invoke(this);
		static Tag GetFirstTag(Item item2)
		{
			if (item2 == null)
			{
				return null;
			}
			if (item2.Tags == null || item2.Tags.Count == 0)
			{
				return null;
			}
			return item2.Tags.Get(0);
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
			item.Detach();
			Item item2 = null;
			while (item.StackCount < item.MaxStackCount && stack.Count > 0)
			{
				item2 = stack.Pop();
				item2.Detach();
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

	public int GetFirstEmptyPosition(int preferedFirstPosition = 0)
	{
		if (preferedFirstPosition < 0)
		{
			preferedFirstPosition = 0;
		}
		if (content.Count <= preferedFirstPosition)
		{
			return preferedFirstPosition;
		}
		for (int i = preferedFirstPosition; i < content.Count; i++)
		{
			if (content[i] == null)
			{
				return i;
			}
		}
		if (content.Count < Capacity)
		{
			return content.Count;
		}
		for (int j = 0; j < preferedFirstPosition; j++)
		{
			if (content[j] == null)
			{
				return j;
			}
		}
		return -1;
	}

	public int GetLastItemPosition()
	{
		for (int num = content.Count - 1; num >= 0; num--)
		{
			if (content[num] != null)
			{
				return num;
			}
		}
		return -1;
	}

	public bool AddAt(Item item, int atPosition)
	{
		if (item == null)
		{
			Debug.LogError("尝试添加的物体为空");
			return false;
		}
		if (Capacity <= atPosition)
		{
			Debug.LogError($"向 Inventory {base.name} 加入物品时位置 {atPosition} 超出最大容量 {Capacity}。");
			return false;
		}
		if (item.ParentObject != null)
		{
			Debug.Log($"{item.name} \nParent: {item.ParentItem} \nInventory: {item.InInventory?.name} \nPlug: {item.PluggedIntoSlot?.DisplayName}");
			Debug.LogError("正在尝试将一个有父物体的物品 " + item.DisplayName + " 放入Inventory。请先使其脱离其父物体 " + item.ParentObject.name + " 再进行此操作。");
			return false;
		}
		Item itemAt = GetItemAt(atPosition);
		if (itemAt != null)
		{
			Debug.LogError($"正在尝试将物品 {item.DisplayName} 放入 Inventory {base.name} 的 {atPosition} 位置。但此位置已经存在另一物体 {itemAt.DisplayName}。");
		}
		while (content.Count <= atPosition)
		{
			content.Add(null);
		}
		content[atPosition] = item;
		item.transform.SetParent(base.transform);
		item.NotifyAddedToInventory(this);
		item.InitiateNotifyItemTreeChanged();
		RecalculateWeight();
		this.onContentChanged?.Invoke(this, atPosition);
		return true;
	}

	public bool RemoveAt(int position, out Item removedItem)
	{
		removedItem = null;
		if (Capacity <= position && position >= content.Count)
		{
			Debug.LogError("位置超出Inventory容量。");
			return false;
		}
		Item itemAt = GetItemAt(position);
		if (itemAt != null)
		{
			content[position] = null;
			removedItem = itemAt;
			removedItem.NotifyRemovedFromInventory(this);
			removedItem.InitiateNotifyItemTreeChanged();
			AttachedToItem?.InitiateNotifyItemTreeChanged();
			int num = content.Count - 1;
			while (num >= 0 && content[num] == null)
			{
				content.RemoveAt(num);
				num--;
			}
			RecalculateWeight();
			this.onContentChanged?.Invoke(this, position);
			return true;
		}
		return false;
	}

	public bool AddItem(Item item)
	{
		int firstEmptyPosition = GetFirstEmptyPosition();
		if (firstEmptyPosition < 0)
		{
			Debug.Log("添加物品失败，Inventory " + base.name + " 已满。");
			return false;
		}
		return AddAt(item, firstEmptyPosition);
	}

	public bool RemoveItem(Item item)
	{
		int num = content.IndexOf(item);
		if (num < 0)
		{
			Debug.LogError("正在尝试从Inventory " + base.name + " 中删除 " + item.DisplayName + "，但它并不在这个Inventory中。");
			return false;
		}
		Item removedItem;
		return RemoveAt(num, out removedItem);
	}

	public Item GetItemAt(int position)
	{
		if (position >= Capacity && position >= content.Count)
		{
			Debug.LogError("访问的位置超出Inventory容量。");
			return null;
		}
		if (content.Count <= position)
		{
			return null;
		}
		return content[position];
	}

	public void Validate(SelfValidationResult result)
	{
		if (AttachedToItem != null)
		{
			if (AttachedToItem.gameObject != base.gameObject)
			{
				result.AddError("AttachedItem引用了另一个Game Object上的Item。").WithFix("引用本物体上的Item。", delegate
				{
					attachedToItem = GetComponent<Item>();
				});
			}
			if (AttachedToItem.Inventory != this)
			{
				if (AttachedToItem.Inventory != null)
				{
					result.AddError("AttachedItem引用了其他的Inventory。请检查Item内的配置。");
				}
				else
				{
					result.AddError("AttachedItem没有引用此Inventory。").WithFix("使AttachedItem引用此Inventory。", delegate
					{
						AttachedToItem.Inventory = this;
					});
				}
			}
		}
		if (!(AttachedToItem == null))
		{
			return;
		}
		Item gotItem = GetComponent<Item>();
		if (gotItem != null)
		{
			result.AddError("同一GameObject上存在Item，但AttachedToItem变量留空。").WithFix("设为本物体上的Item。", delegate
			{
				attachedToItem = gotItem;
			});
		}
	}

	public IEnumerator<Item> GetEnumerator()
	{
		foreach (Item item in content)
		{
			if (!(item == null))
			{
				yield return item;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void DestroyAllContent()
	{
		for (int i = 0; i < content.Count; i++)
		{
			Item item = content[i];
			if (!(item == null))
			{
				RemoveItem(item);
				item.DestroyTree();
			}
		}
	}

	public List<Item> FindAll(Predicate<Item> match)
	{
		return content.FindAll(match);
	}

	public void RecalculateWeight()
	{
		float num = 0f;
		foreach (Item item in content)
		{
			if (!(item == null))
			{
				float num2 = item.RecalculateTotalWeight();
				num += num2;
			}
		}
		cachedWeight = num;
	}

	public void SetCapacity(int capacity)
	{
		defaultCapacity = capacity;
		this.onCapacityChanged?.Invoke(this);
	}

	public int GetItemCount()
	{
		int num = 0;
		foreach (Item item in content)
		{
			if (!(item == null))
			{
				num++;
			}
		}
		return num;
	}

	internal void NotifyContentChanged(Item item)
	{
		if ((bool)item)
		{
			this.onContentChanged?.Invoke(this, content.IndexOf(item));
		}
	}

	public int GetIndex(Item item)
	{
		if (item == null)
		{
			return -1;
		}
		return content.IndexOf(item);
	}

	public Item Find(int typeID)
	{
		return content.Find((Item e) => e != null && e.TypeID == typeID);
	}
}
