using System;
using System.Collections;
using System.Collections.Generic;

namespace ItemStatsSystem.Items;

public class SlotCollection : ItemComponent, ICollection<Slot>, IEnumerable<Slot>, IEnumerable
{
	public Action<Slot> OnSlotContentChanged;

	public List<Slot> list;

	private Dictionary<int, Slot> _cachedSlotsDictionary;

	private Dictionary<int, Slot> slotsDictionary
	{
		get
		{
			if (_cachedSlotsDictionary == null)
			{
				BuildDictionary();
			}
			return _cachedSlotsDictionary;
		}
	}

	public int Count
	{
		get
		{
			if (list != null)
			{
				return list.Count;
			}
			return 0;
		}
	}

	public bool IsReadOnly => false;

	public Slot this[string key] => GetSlot(key);

	public Slot this[int index] => GetSlotByIndex(index);

	public Slot GetSlotByIndex(int index)
	{
		return list[index];
	}

	public Slot GetSlot(int hash)
	{
		if (slotsDictionary.TryGetValue(hash, out var value))
		{
			return value;
		}
		return null;
	}

	public Slot GetSlot(string key)
	{
		int hashCode = key.GetHashCode();
		Slot slot = GetSlot(hashCode);
		if (slot == null)
		{
			slot = list.Find((Slot e) => e.Key == key);
		}
		return slot;
	}

	private void BuildDictionary()
	{
		if (_cachedSlotsDictionary == null)
		{
			_cachedSlotsDictionary = new Dictionary<int, Slot>();
		}
		_cachedSlotsDictionary.Clear();
		foreach (Slot item in list)
		{
			int hashCode = item.Key.GetHashCode();
			_cachedSlotsDictionary[hashCode] = item;
		}
	}

	internal override void OnInitialize()
	{
		foreach (Slot item in list)
		{
			item.Initialize(this);
		}
	}

	public IEnumerator<Slot> GetEnumerator()
	{
		return list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return list.GetEnumerator();
	}

	public void Add(Slot item)
	{
		list.Add(item);
	}

	public void Clear()
	{
		list.Clear();
	}

	public bool Contains(Slot item)
	{
		return list.Contains(item);
	}

	public void CopyTo(Slot[] array, int arrayIndex)
	{
		list.CopyTo(array, arrayIndex);
	}

	public bool Remove(Slot item)
	{
		return list.Remove(item);
	}
}
