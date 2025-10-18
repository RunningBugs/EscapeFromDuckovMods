using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class FXPool : MonoBehaviour
{
	private class Pool
	{
		private ParticleSystem prefab;

		private Transform parent;

		private ObjectPool<ParticleSystem> pool;

		private Action<ParticleSystem> onCreate;

		private Action<ParticleSystem> onGet;

		private Action<ParticleSystem> onRelease;

		private Action<ParticleSystem> onDestroy;

		private List<ParticleSystem> activeEntries = new List<ParticleSystem>();

		public Pool(ParticleSystem prefab, Transform parent, Action<ParticleSystem> onCreate = null, Action<ParticleSystem> onGet = null, Action<ParticleSystem> onRelease = null, Action<ParticleSystem> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 100)
		{
			this.prefab = prefab;
			this.parent = parent;
			pool = new ObjectPool<ParticleSystem>(Create, OnEntryGet, OnEntryRelease, OnEntryDestroy, collectionCheck, defaultCapacity, maxSize);
			this.onCreate = onCreate;
			this.onGet = onGet;
			this.onRelease = onRelease;
			this.onDestroy = onDestroy;
		}

		private ParticleSystem Create()
		{
			ParticleSystem particleSystem = UnityEngine.Object.Instantiate(prefab, parent);
			onCreate?.Invoke(particleSystem);
			return particleSystem;
		}

		public void OnEntryGet(ParticleSystem obj)
		{
			activeEntries.Add(obj);
		}

		public void OnEntryRelease(ParticleSystem obj)
		{
			activeEntries.Remove(obj);
			obj.gameObject.SetActive(value: false);
		}

		public void OnEntryDestroy(ParticleSystem obj)
		{
			onDestroy?.Invoke(obj);
		}

		public ParticleSystem Get()
		{
			return pool.Get();
		}

		public void Release(ParticleSystem obj)
		{
			pool.Release(obj);
		}

		public void Tick()
		{
			List<ParticleSystem> list = new List<ParticleSystem>();
			foreach (ParticleSystem activeEntry in activeEntries)
			{
				if (!activeEntry.isPlaying)
				{
					list.Add(activeEntry);
				}
			}
			foreach (ParticleSystem item in list)
			{
				Release(item);
			}
		}
	}

	private Dictionary<ParticleSystem, Pool> poolsDic;

	public static FXPool Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void FixedUpdate()
	{
		if (poolsDic == null)
		{
			return;
		}
		foreach (Pool value in poolsDic.Values)
		{
			value.Tick();
		}
	}

	private Pool GetOrCreatePool(ParticleSystem prefab)
	{
		if (poolsDic == null)
		{
			poolsDic = new Dictionary<ParticleSystem, Pool>();
		}
		if (poolsDic.TryGetValue(prefab, out var value))
		{
			return value;
		}
		Pool pool = new Pool(prefab, base.transform);
		poolsDic[prefab] = pool;
		return pool;
	}

	private static ParticleSystem Get(ParticleSystem prefab)
	{
		if (Instance == null)
		{
			return null;
		}
		return Instance.GetOrCreatePool(prefab).Get();
	}

	public static ParticleSystem Play(ParticleSystem prefab, Vector3 postion, Quaternion rotation)
	{
		if (Instance == null)
		{
			return null;
		}
		if (prefab == null)
		{
			return null;
		}
		ParticleSystem particleSystem = Get(prefab);
		particleSystem.transform.position = postion;
		particleSystem.transform.rotation = rotation;
		particleSystem.gameObject.SetActive(value: true);
		particleSystem.Play();
		return particleSystem;
	}

	public static ParticleSystem Play(ParticleSystem prefab, Vector3 postion, Quaternion rotation, Color color)
	{
		if (Instance == null)
		{
			return null;
		}
		if (prefab == null)
		{
			return null;
		}
		ParticleSystem particleSystem = Get(prefab);
		particleSystem.transform.position = postion;
		particleSystem.transform.rotation = rotation;
		particleSystem.gameObject.SetActive(value: true);
		ParticleSystem.MainModule main = particleSystem.main;
		main.startColor = color;
		particleSystem.Play();
		return particleSystem;
	}
}
