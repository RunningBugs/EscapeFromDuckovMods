using System.Collections.Generic;
using Duckov.UI;
using Duckov.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryFilterDisplay : MonoBehaviour, ISingleSelectionMenu<InventoryFilterDisplayEntry>
{
	[SerializeField]
	private InventoryFilterDisplayEntry template;

	private PrefabPool<InventoryFilterDisplayEntry> _pool;

	private InventoryDisplay targetDisplay;

	private InventoryFilterProvider provider;

	private List<InventoryFilterDisplayEntry> entries = new List<InventoryFilterDisplayEntry>();

	private InventoryFilterDisplayEntry selection;

	private PrefabPool<InventoryFilterDisplayEntry> Pool
	{
		get
		{
			if (_pool == null)
			{
				_pool = new PrefabPool<InventoryFilterDisplayEntry>(template);
			}
			return _pool;
		}
	}

	private void Awake()
	{
		template.gameObject.SetActive(value: false);
	}

	public void Setup(InventoryDisplay target)
	{
		Pool.ReleaseAll();
		entries.Clear();
		if (target == null)
		{
			return;
		}
		targetDisplay = target;
		provider = target.Target.GetComponent<InventoryFilterProvider>();
		if (!(provider == null))
		{
			InventoryFilterProvider.FilterEntry[] array = provider.entries;
			foreach (InventoryFilterProvider.FilterEntry filter in array)
			{
				InventoryFilterDisplayEntry inventoryFilterDisplayEntry = Pool.Get();
				inventoryFilterDisplayEntry.Setup(OnEntryClicked, filter);
				entries.Add(inventoryFilterDisplayEntry);
			}
			selection = null;
		}
	}

	private void OnEntryClicked(InventoryFilterDisplayEntry entry, PointerEventData data)
	{
		SetSelection(entry);
	}

	internal void Select(int i)
	{
		if (i >= 0 && i < entries.Count)
		{
			SetSelection(entries[i]);
		}
	}

	public InventoryFilterDisplayEntry GetSelection()
	{
		return selection;
	}

	public bool SetSelection(InventoryFilterDisplayEntry selection)
	{
		if (selection == null)
		{
			return false;
		}
		this.selection = selection;
		InventoryFilterProvider.FilterEntry filter = selection.Filter;
		targetDisplay.SetFilter(filter.GetFunction());
		foreach (InventoryFilterDisplayEntry entry in entries)
		{
			entry.NotifySelectionChanged(entry == selection);
		}
		return true;
	}
}
