using Duckov.Utilities;
using UnityEngine;

public class LevelConfig : MonoBehaviour
{
	private static LevelConfig instance;

	[SerializeField]
	private bool isBaseLevel;

	[SerializeField]
	private bool isRaidMap = true;

	[SerializeField]
	private bool spawnTomb = true;

	[SerializeField]
	private int minExitCount;

	[SerializeField]
	private int maxExitCount;

	public TimeOfDayConfig timeOfDayConfig;

	[SerializeField]
	[Min(1f)]
	private float lootBoxHighQualityChanceMultiplier = 1f;

	[SerializeField]
	[Range(0.1f, 10f)]
	private float lootboxItemCountMultiplier = 1f;

	public static LevelConfig Instance
	{
		get
		{
			if (!instance)
			{
				SetInstance();
			}
			return instance;
		}
	}

	public float LootBoxQualityLowPercent => 1f - 1f / lootBoxHighQualityChanceMultiplier;

	public float LootboxItemCountMultiplier => lootboxItemCountMultiplier;

	public static bool IsBaseLevel
	{
		get
		{
			if (!Instance)
			{
				return false;
			}
			return Instance.isBaseLevel;
		}
	}

	public static bool IsRaidMap
	{
		get
		{
			if (!Instance)
			{
				return false;
			}
			return Instance.isRaidMap;
		}
	}

	public static int MinExitCount
	{
		get
		{
			if (!Instance)
			{
				return 0;
			}
			return Instance.minExitCount;
		}
	}

	public static bool SpawnTomb
	{
		get
		{
			if (!Instance)
			{
				return true;
			}
			return Instance.spawnTomb;
		}
	}

	public static int MaxExitCount
	{
		get
		{
			if (!Instance)
			{
				return 0;
			}
			return Instance.maxExitCount;
		}
	}

	private void Awake()
	{
		Object.Instantiate(GameplayDataSettings.Prefabs.LevelManagerPrefab).transform.SetParent(base.transform);
	}

	private static void SetInstance()
	{
		if (!instance)
		{
			instance = Object.FindFirstObjectByType<LevelConfig>();
			_ = (bool)instance;
		}
	}
}
