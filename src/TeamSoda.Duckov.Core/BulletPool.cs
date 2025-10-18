using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletPool : MonoBehaviour
{
	public Dictionary<Projectile, ObjectPool<Projectile>> pools = new Dictionary<Projectile, ObjectPool<Projectile>>();

	private void Awake()
	{
	}

	public Projectile GetABullet(Projectile bulletPrefab)
	{
		return GetAPool(bulletPrefab).Get();
	}

	private ObjectPool<Projectile> GetAPool(Projectile pfb)
	{
		if (pools.TryGetValue(pfb, out var value))
		{
			return value;
		}
		ObjectPool<Projectile> objectPool = new ObjectPool<Projectile>(() => CreateABulletInPool(pfb), OnGetABulletInPool, OnBulletRelease);
		pools.Add(pfb, objectPool);
		return objectPool;
	}

	private Projectile CreateABulletInPool(Projectile pfb)
	{
		Projectile projectile = Object.Instantiate(pfb);
		projectile.transform.SetParent(base.transform);
		ObjectPool<Projectile> aPool = GetAPool(pfb);
		projectile.SetPool(aPool);
		return projectile;
	}

	private void OnGetABulletInPool(Projectile bulletToGet)
	{
		bulletToGet.gameObject.SetActive(value: true);
	}

	private void OnBulletRelease(Projectile bulletToGet)
	{
		bulletToGet.transform.SetParent(base.transform);
		bulletToGet.gameObject.SetActive(value: false);
	}

	public bool Release(Projectile instance, Projectile prefab)
	{
		if (pools.TryGetValue(prefab, out var value))
		{
			value.Release(prefab);
			return true;
		}
		return false;
	}
}
