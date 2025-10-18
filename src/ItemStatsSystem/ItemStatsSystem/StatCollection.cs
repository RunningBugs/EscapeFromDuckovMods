using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemStatsSystem;

public class StatCollection : ItemComponent, ICollection<Stat>, IEnumerable<Stat>, IEnumerable
{
	[SerializeField]
	private List<Stat> list;

	private Dictionary<int, Stat> _cachedStatsDictionary;

	private Dictionary<int, Stat> statsDictionary
	{
		get
		{
			if (_cachedStatsDictionary == null)
			{
				BuildDictionary();
			}
			return _cachedStatsDictionary;
		}
	}

	public int Count => list.Count;

	public bool IsReadOnly => false;

	public Stat this[int hash] => GetStat(hash);

	public Stat this[string key] => GetStat(key);

	public Stat GetStat(int hash)
	{
		if (statsDictionary.TryGetValue(hash, out var value))
		{
			return value;
		}
		return null;
	}

	public Stat GetStat(string key)
	{
		int hashCode = key.GetHashCode();
		Stat stat = GetStat(hashCode);
		if (stat == null)
		{
			stat = list.Find((Stat e) => e.Key == key);
		}
		return stat;
	}

	private void BuildDictionary()
	{
		if (_cachedStatsDictionary == null)
		{
			_cachedStatsDictionary = new Dictionary<int, Stat>();
		}
		_cachedStatsDictionary.Clear();
		foreach (Stat item in list)
		{
			int hashCode = item.Key.GetHashCode();
			_cachedStatsDictionary[hashCode] = item;
		}
	}

	internal override void OnInitialize()
	{
		foreach (Stat item in list)
		{
			item.Initialize(this);
		}
	}

	public IEnumerator<Stat> GetEnumerator()
	{
		return list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return list.GetEnumerator();
	}

	public void Add(Stat item)
	{
		list.Add(item);
	}

	public void Clear()
	{
		list.Clear();
	}

	public bool Contains(Stat item)
	{
		return list.Contains(item);
	}

	public void CopyTo(Stat[] array, int arrayIndex)
	{
		list.CopyTo(array, arrayIndex);
	}

	public bool Remove(Stat item)
	{
		return list.Remove(item);
	}
}
