using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Pool;

namespace Duckov.Utilities;

public class PrefabPool<T> where T : Component
{
	public readonly T Prefab;

	public Transform poolParent;

	private Action<T> onGet;

	private Action<T> onRelease;

	private Action<T> onDestroy;

	private Action<T> onCreate;

	public readonly bool CollectionCheck;

	public readonly int DefaultCapacity;

	public readonly int MaxSize;

	private readonly ObjectPool<T> pool;

	private List<T> activeObjects;

	public ReadOnlyCollection<T> ActiveEntries => activeObjects.AsReadOnly();

	public PrefabPool(T prefab, Transform poolParent = null, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000, Action<T> onCreate = null)
	{
		Prefab = prefab;
		prefab.gameObject.SetActive(value: false);
		if (poolParent == null)
		{
			poolParent = prefab.transform.parent;
		}
		this.poolParent = poolParent;
		this.onGet = onGet;
		this.onRelease = onRelease;
		this.onDestroy = onDestroy;
		CollectionCheck = collectionCheck;
		DefaultCapacity = defaultCapacity;
		MaxSize = maxSize;
		this.onCreate = onCreate;
		pool = new ObjectPool<T>(CreateInstance, OnGet, OnRelease, OnDestroy, collectionCheck, defaultCapacity, maxSize);
		activeObjects = new List<T>();
	}

	public T Get(Transform setParent = null)
	{
		if (setParent == null)
		{
			setParent = poolParent;
		}
		T val = pool.Get();
		if ((bool)setParent)
		{
			val.transform.SetParent(setParent, worldPositionStays: false);
			val.transform.SetAsLastSibling();
		}
		return val;
	}

	public void Release(T item)
	{
		pool.Release(item);
		if (item is IPoolable poolable)
		{
			poolable.NotifyReleased();
		}
	}

	private T CreateInstance()
	{
		T val = UnityEngine.Object.Instantiate(Prefab);
		onCreate?.Invoke(val);
		return val;
	}

	private void OnGet(T item)
	{
		activeObjects.Add(item);
		item.gameObject.SetActive(value: true);
		if (item is IPoolable poolable)
		{
			poolable.NotifyPooled();
		}
		onGet?.Invoke(item);
	}

	private void OnRelease(T item)
	{
		activeObjects.Remove(item);
		onRelease?.Invoke(item);
		if (item != null)
		{
			item.gameObject.SetActive(value: false);
			item.transform.SetParent(poolParent);
		}
	}

	private void OnDestroy(T item)
	{
		onDestroy?.Invoke(item);
		UnityEngine.Object.Destroy(item.gameObject);
	}

	public void ReleaseAll()
	{
		activeObjects.RemoveAll((T e) => e == null);
		T[] array = activeObjects.ToArray();
		foreach (T item in array)
		{
			Release(item);
		}
	}

	public T Find(Predicate<T> predicate)
	{
		foreach (T activeObject in activeObjects)
		{
			if (predicate(activeObject))
			{
				return activeObject;
			}
		}
		return null;
	}

	public int ReleaseAll(Predicate<T> predicate)
	{
		List<T> list = new List<T>();
		foreach (T activeObject in activeObjects)
		{
			if (predicate(activeObject))
			{
				list.Add(activeObject);
			}
		}
		foreach (T item in list)
		{
			Release(item);
		}
		return list.Count;
	}
}
