using System;
using Saves;
using UnityEngine;

namespace Duckov.Achievements;

public class StatisticsManager : MonoBehaviour
{
	public static event Action<string, long, long> OnStatisticsChanged;

	private static string GetSaveKey(string statisticsKey)
	{
		return "Statistics/" + statisticsKey;
	}

	private static long Get(string key)
	{
		GetSaveKey(key);
		if (!SavesSystem.KeyExisits(key))
		{
			return 0L;
		}
		return SavesSystem.Load<long>(key);
	}

	private static void Set(string key, long value)
	{
		long arg = Get(key);
		GetSaveKey(key);
		SavesSystem.Save(key, value);
		StatisticsManager.OnStatisticsChanged?.Invoke(key, arg, value);
	}

	public static void Add(string key, long value = 1L)
	{
		long num = Get(key);
		try
		{
			num = checked(num + value);
		}
		catch (OverflowException exception)
		{
			Debug.LogException(exception);
			Debug.Log("Failed changing statistics of " + key + ". Overflow detected.");
			return;
		}
		Set(key, num);
	}

	private void Awake()
	{
		RegisterEvents();
	}

	private void OnDestroy()
	{
		UnregisterEvents();
	}

	private void RegisterEvents()
	{
	}

	private void UnregisterEvents()
	{
	}
}
