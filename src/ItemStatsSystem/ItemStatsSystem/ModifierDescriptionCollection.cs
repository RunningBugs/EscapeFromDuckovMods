using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemStatsSystem;

public class ModifierDescriptionCollection : ItemComponent, ICollection<ModifierDescription>, IEnumerable<ModifierDescription>, IEnumerable
{
	private bool _modifierEnableCache = true;

	[SerializeField]
	private List<ModifierDescription> list;

	public int Count => list.Count;

	public bool ModifierEnable
	{
		get
		{
			return _modifierEnableCache;
		}
		set
		{
			_modifierEnableCache = value;
			ReapplyModifiers();
		}
	}

	public bool IsReadOnly => false;

	internal override void OnInitialize()
	{
		base.Master.onItemTreeChanged += OnItemTreeChange;
		base.Master.onDurabilityChanged += OnDurabilityChange;
	}

	private void OnDurabilityChange(Item item)
	{
		ReapplyModifiers();
	}

	private void OnDestroy()
	{
		if ((bool)base.Master)
		{
			base.Master.onItemTreeChanged -= OnItemTreeChange;
			base.Master.onDurabilityChanged -= OnDurabilityChange;
		}
	}

	private void OnItemTreeChange(Item item)
	{
		ReapplyModifiers();
	}

	public void ReapplyModifiers()
	{
		if (base.Master == null)
		{
			return;
		}
		bool flag = ModifierEnable;
		if (base.Master.UseDurability && base.Master.Durability <= 0f)
		{
			flag = false;
		}
		if (!flag)
		{
			foreach (ModifierDescription item in list)
			{
				item.Release();
			}
			return;
		}
		foreach (ModifierDescription item2 in list)
		{
			item2.ReapplyModifier(this);
		}
	}

	public void Add(ModifierDescription item)
	{
		list.Add(item);
	}

	public void Clear()
	{
		if (list == null)
		{
			list = new List<ModifierDescription>();
		}
		foreach (ModifierDescription item in list)
		{
			item.Release();
		}
		list.Clear();
	}

	public bool Contains(ModifierDescription item)
	{
		return list.Contains(item);
	}

	public void CopyTo(ModifierDescription[] array, int arrayIndex)
	{
		list.CopyTo(array, arrayIndex);
	}

	public bool Remove(ModifierDescription item)
	{
		if (item != null && list.Contains(item))
		{
			item.Release();
		}
		return list.Remove(item);
	}

	public IEnumerator<ModifierDescription> GetEnumerator()
	{
		return list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return list.GetEnumerator();
	}

	public ModifierDescription Find(Predicate<ModifierDescription> predicate)
	{
		return list.Find(predicate);
	}
}
