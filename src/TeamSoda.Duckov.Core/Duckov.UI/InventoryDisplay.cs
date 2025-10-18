using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using Duckov.Utilities;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI;

public class InventoryDisplay : MonoBehaviour, IPoolable
{
	[SerializeField]
	private InventoryEntry entryPrefab;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private TextMeshProUGUI capacityText;

	[SerializeField]
	private string capacityTextFormat = "({1}/{0})";

	[SerializeField]
	private FadeGroup loadingIndcator;

	[SerializeField]
	private FadeGroup contentFadeGroup;

	[SerializeField]
	private GridLayoutGroup contentLayout;

	[SerializeField]
	private LayoutElement gridLayoutElement;

	[SerializeField]
	private GameObject placeHolder;

	[SerializeField]
	private Transform entriesParent;

	[SerializeField]
	private Button sortButton;

	[SerializeField]
	private Vector2Int shortcutsRange = new Vector2Int(0, 3);

	[SerializeField]
	private bool editable = true;

	[SerializeField]
	private bool showOperationButtons = true;

	[SerializeField]
	private bool showSortButton;

	[SerializeField]
	private bool usePages;

	[SerializeField]
	private int itemsEachPage = 30;

	public Func<Item, bool> filter;

	[SerializeField]
	private List<InventoryEntry> entries = new List<InventoryEntry>();

	private PrefabPool<InventoryEntry> _entryPool;

	private Func<Item, bool> _func_ShouldHighlight;

	private Func<Item, bool> _func_CanOperate;

	private int cachedCapacity = -1;

	private int activeTaskToken;

	private int cachedMaxPage = 1;

	private int cachedSelectedPage;

	private List<int> cachedIndexesToDisplay = new List<int>();

	private bool shortcuts => false;

	public bool UsePages => usePages;

	public bool Editable
	{
		get
		{
			return editable;
		}
		internal set
		{
			editable = value;
		}
	}

	public bool ShowOperationButtons
	{
		get
		{
			return showOperationButtons;
		}
		internal set
		{
			showOperationButtons = value;
		}
	}

	public bool Movable { get; private set; }

	public Inventory Target { get; private set; }

	private PrefabPool<InventoryEntry> EntryPool
	{
		get
		{
			if (_entryPool == null && entryPrefab != null)
			{
				_entryPool = new PrefabPool<InventoryEntry>(entryPrefab, contentLayout.transform);
			}
			return _entryPool;
		}
	}

	public Func<Item, bool> Func_ShouldHighlight => _func_ShouldHighlight;

	public Func<Item, bool> Func_CanOperate => _func_CanOperate;

	public bool ShowSortButton
	{
		get
		{
			return showSortButton;
		}
		internal set
		{
			showSortButton = value;
		}
	}

	public int MaxPage => cachedMaxPage;

	public int SelectedPage => cachedSelectedPage;

	public event Action<InventoryDisplay, InventoryEntry, PointerEventData> onDisplayDoubleClicked;

	public event Action onPageInfoRefreshed;

	private void RegisterEvents()
	{
		if (!(Target == null))
		{
			UnregisterEvents();
			Target.onContentChanged += OnTargetContentChanged;
			Target.onInventorySorted += OnTargetSorted;
			Target.onSetIndexLock += OnTargetSetIndexLock;
		}
	}

	private void UnregisterEvents()
	{
		if (!(Target == null))
		{
			Target.onContentChanged -= OnTargetContentChanged;
			Target.onInventorySorted -= OnTargetSorted;
			Target.onSetIndexLock -= OnTargetSetIndexLock;
		}
	}

	private void OnTargetSetIndexLock(Inventory inventory, int index)
	{
		foreach (InventoryEntry entry in entries)
		{
			if (!(entry == null) && entry.isActiveAndEnabled && entry.Index == index)
			{
				entry.Refresh();
			}
		}
	}

	private void OnTargetSorted(Inventory inventory)
	{
		if (filter == null)
		{
			foreach (InventoryEntry entry in entries)
			{
				entry.Refresh();
			}
			return;
		}
		LoadEntriesTask().Forget();
	}

	private void OnTargetContentChanged(Inventory inventory, int position)
	{
		if (Target.Loading)
		{
			return;
		}
		if (filter != null)
		{
			RefreshCapacityText();
			LoadEntriesTask().Forget();
			return;
		}
		RefreshCapacityText();
		InventoryEntry inventoryEntry = entries.Find((InventoryEntry e) => e != null && e.Index == position);
		if ((bool)inventoryEntry)
		{
			inventoryEntry.Refresh();
			inventoryEntry.Punch();
		}
	}

	private void RefreshCapacityText()
	{
		if (!(Target == null) && (bool)capacityText)
		{
			capacityText.text = string.Format(capacityTextFormat, Target.Capacity, Target.GetItemCount());
		}
	}

	public void Setup(Inventory target, Func<Item, bool> funcShouldHighLight = null, Func<Item, bool> funcCanOperate = null, bool movable = false, Func<Item, bool> filter = null)
	{
		UnregisterEvents();
		Target = target;
		Clear();
		if (Target == null || Target.Loading)
		{
			return;
		}
		if (funcShouldHighLight == null)
		{
			_func_ShouldHighlight = (Item e) => false;
		}
		else
		{
			_func_ShouldHighlight = funcShouldHighLight;
		}
		if (funcCanOperate == null)
		{
			_func_CanOperate = (Item e) => true;
		}
		else
		{
			_func_CanOperate = funcCanOperate;
		}
		displayNameText.text = target.DisplayName;
		Movable = movable;
		cachedCapacity = target.Capacity;
		this.filter = filter;
		RefreshCapacityText();
		RegisterEvents();
		sortButton.gameObject.SetActive(editable && showSortButton);
		LoadEntriesTask().Forget();
	}

	private void RefreshGridLayoutPreferredHeight()
	{
		if (Target == null)
		{
			placeHolder.gameObject.SetActive(value: true);
			return;
		}
		int num = cachedIndexesToDisplay.Count;
		if (usePages && num > 0)
		{
			int num2 = cachedSelectedPage * itemsEachPage;
			int num3 = Mathf.Min(num2 + itemsEachPage, cachedIndexesToDisplay.Count);
			num = Mathf.Max(0, num3 - num2);
		}
		float preferredHeight = (float)Mathf.CeilToInt((float)num / (float)contentLayout.constraintCount) * contentLayout.cellSize.y + (float)contentLayout.padding.top + (float)contentLayout.padding.bottom;
		gridLayoutElement.preferredHeight = preferredHeight;
		placeHolder.gameObject.SetActive(num <= 0);
	}

	public void SetPage(int page)
	{
		cachedSelectedPage = page;
		this.onPageInfoRefreshed?.Invoke();
		LoadEntriesTask().Forget();
	}

	public void NextPage()
	{
		int num = cachedSelectedPage + 1;
		if (num >= cachedMaxPage)
		{
			num = 0;
		}
		SetPage(num);
	}

	public void PreviousPage()
	{
		int num = cachedSelectedPage - 1;
		if (num < 0)
		{
			num = cachedMaxPage - 1;
		}
		SetPage(num);
	}

	private void CacheIndexesToDisplay()
	{
		cachedIndexesToDisplay.Clear();
		for (int i = 0; i < Target.Capacity; i++)
		{
			if (filter != null)
			{
				Item itemAt = Target.GetItemAt(i);
				if (!filter(itemAt))
				{
					continue;
				}
			}
			cachedIndexesToDisplay.Add(i);
		}
		int count = cachedIndexesToDisplay.Count;
		cachedMaxPage = count / itemsEachPage + ((count % itemsEachPage > 0) ? 1 : 0);
		if (cachedSelectedPage >= cachedMaxPage)
		{
			cachedSelectedPage = Mathf.Max(0, cachedMaxPage - 1);
		}
		this.onPageInfoRefreshed?.Invoke();
	}

	private async UniTask LoadEntriesTask()
	{
		placeHolder.gameObject.SetActive(value: false);
		int token = ++activeTaskToken;
		EntryPool.ReleaseAll();
		entries.Clear();
		int batchCount = 5;
		int num = 0;
		CacheIndexesToDisplay();
		RefreshGridLayoutPreferredHeight();
		contentFadeGroup.SkipHide();
		loadingIndcator.Show();
		List<int> indexes;
		if (usePages)
		{
			int num2 = cachedSelectedPage * itemsEachPage;
			int num3 = Mathf.Min(num2 + itemsEachPage, cachedIndexesToDisplay.Count);
			if (num2 >= cachedIndexesToDisplay.Count || num2 >= num3)
			{
				indexes = new List<int>();
			}
			else
			{
				indexes = GetRange(num2, num3, cachedIndexesToDisplay);
			}
		}
		else
		{
			indexes = cachedIndexesToDisplay;
		}
		foreach (int index in indexes)
		{
			if (num >= batchCount)
			{
				await UniTask.Yield();
				if (!TaskValid())
				{
					return;
				}
				num = 0;
			}
			InventoryEntry newInventoryEntry = GetNewInventoryEntry();
			newInventoryEntry.gameObject.SetActive(value: true);
			newInventoryEntry.Setup(this, index);
			entries.Add(newInventoryEntry);
			newInventoryEntry.transform.SetParent(entriesParent, worldPositionStays: false);
			num++;
		}
		loadingIndcator.Hide();
		contentFadeGroup.Show();
		List<int> GetRange(int begin, int end_exclusive, List<int> list)
		{
			if (begin < 0)
			{
				begin = 0;
			}
			if (end_exclusive < 0)
			{
				end_exclusive = 0;
			}
			indexes = new List<int>();
			if (end_exclusive > list.Count)
			{
				end_exclusive = list.Count;
			}
			if (begin >= end_exclusive)
			{
				return indexes;
			}
			for (int i = begin; i < end_exclusive; i++)
			{
				indexes.Add(list[i]);
			}
			return indexes;
		}
		bool TaskValid()
		{
			if (!Application.isPlaying)
			{
				return false;
			}
			return token == activeTaskToken;
		}
	}

	public void SetFilter(Func<Item, bool> filter)
	{
		this.filter = filter;
		cachedSelectedPage = 0;
		LoadEntriesTask().Forget();
	}

	private void Clear()
	{
		EntryPool.ReleaseAll();
		entries.Clear();
	}

	private void Awake()
	{
		sortButton.onClick.AddListener(OnSortButtonClicked);
	}

	private void OnSortButtonClicked()
	{
		if (Editable && (bool)Target && !Target.Loading)
		{
			Target.Sort();
		}
	}

	private void OnEnable()
	{
		RegisterEvents();
	}

	private void OnDisable()
	{
		UnregisterEvents();
		activeTaskToken++;
	}

	private void Update()
	{
		if ((bool)Target && cachedCapacity != Target.Capacity)
		{
			OnCapacityChanged();
		}
	}

	private void OnCapacityChanged()
	{
		if (!(Target == null))
		{
			cachedCapacity = Target.Capacity;
			RefreshCapacityText();
			LoadEntriesTask().Forget();
		}
	}

	public bool IsShortcut(int index)
	{
		if (!shortcuts)
		{
			return false;
		}
		if (index >= shortcutsRange.x)
		{
			return index <= shortcutsRange.y;
		}
		return false;
	}

	private InventoryEntry GetNewInventoryEntry()
	{
		return EntryPool.Get();
	}

	internal void NotifyItemDoubleClicked(InventoryEntry inventoryEntry, PointerEventData data)
	{
		this.onDisplayDoubleClicked?.Invoke(this, inventoryEntry, data);
	}

	public void NotifyPooled()
	{
	}

	public void NotifyReleased()
	{
	}

	public void DisableItem(Item item)
	{
		foreach (InventoryEntry item2 in entries.Where((InventoryEntry e) => e.Content == item))
		{
			item2.Disabled = true;
		}
	}

	internal bool EvaluateShouldHighlight(Item content)
	{
		if (Func_ShouldHighlight != null && Func_ShouldHighlight(content))
		{
			return true;
		}
		_ = content == null;
		return false;
	}
}
