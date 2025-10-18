using System.Linq;
using Duckov.Utilities;
using UnityEngine;

namespace Duckov.UI;

public class HealthBarManager : MonoBehaviour
{
	private static HealthBarManager _instance;

	[SerializeField]
	public HealthBar healthBarPrefab;

	private PrefabPool<HealthBar> _prefabPool;

	public static HealthBarManager Instance => _instance;

	private PrefabPool<HealthBar> PrefabPool
	{
		get
		{
			if (_prefabPool == null)
			{
				_prefabPool = new PrefabPool<HealthBar>(healthBarPrefab, base.transform);
			}
			return _prefabPool;
		}
	}

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
		}
		UnregisterStaticEvents();
		RegisterStaticEvents();
	}

	private void OnDestroy()
	{
		UnregisterStaticEvents();
	}

	private void RegisterStaticEvents()
	{
		Health.OnRequestHealthBar += Health_OnRequestHealthBar;
	}

	private void UnregisterStaticEvents()
	{
		Health.OnRequestHealthBar -= Health_OnRequestHealthBar;
	}

	private HealthBar GetActiveHealthBar(Health health)
	{
		if (health == null)
		{
			return null;
		}
		return PrefabPool.ActiveEntries.FirstOrDefault((HealthBar e) => e.target == health);
	}

	private HealthBar CreateHealthBarFor(Health health, DamageInfo? damage = null)
	{
		if (health == null)
		{
			return null;
		}
		if ((bool)PrefabPool.ActiveEntries.FirstOrDefault((HealthBar e) => e.target == health))
		{
			Debug.Log("Health bar for " + health.name + " already exists");
			return null;
		}
		HealthBar newBar = PrefabPool.Get();
		newBar.Setup(health, damage, delegate
		{
			PrefabPool.Release(newBar);
		});
		return newBar;
	}

	private void Health_OnRequestHealthBar(Health health)
	{
		HealthBar activeHealthBar = GetActiveHealthBar(health);
		if (activeHealthBar != null)
		{
			activeHealthBar.RefreshOffset();
		}
		else
		{
			CreateHealthBarFor(health);
		}
	}

	public static void RequestHealthBar(Health health, DamageInfo? damage = null)
	{
		if (!(Instance == null))
		{
			Instance.CreateHealthBarFor(health, damage);
		}
	}
}
