using System;
using System.Collections.Generic;
using Duckov.UI;
using Saves;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov;

public class EXPManager : MonoBehaviour, ISaveDataProvider
{
	private static EXPManager instance;

	[SerializeField]
	private string levelChangeNotificationFormatKey = "UI_LevelChangeNotification";

	[SerializeField]
	private List<long> levelExpDefinition;

	[SerializeField]
	private long point;

	public static Action<long> onExpChanged;

	public static Action<int, int> onLevelChanged;

	private long cachedExp;

	private const string prefixKey = "EXP";

	private const string key = "Value";

	public static EXPManager Instance => instance;

	private string LevelChangeNotificationFormat => levelChangeNotificationFormatKey.ToPlainText();

	public static long EXP
	{
		get
		{
			if (instance == null)
			{
				return 0L;
			}
			return instance.point;
		}
		private set
		{
			if (!(instance == null))
			{
				int level = Level;
				instance.point = value;
				onExpChanged?.Invoke(value);
				int level2 = Level;
				if (level != level2)
				{
					OnLevelChanged(level, level2);
				}
			}
		}
	}

	public static int Level
	{
		get
		{
			if (instance == null)
			{
				return 0;
			}
			return instance.LevelFromExp(EXP);
		}
	}

	public static long CachedExp
	{
		get
		{
			if (instance == null)
			{
				return 0L;
			}
			return instance.cachedExp;
		}
	}

	private string realKey => "EXP_Value";

	private static void OnLevelChanged(int oldLevel, int newLevel)
	{
		onLevelChanged?.Invoke(oldLevel, newLevel);
		if (!(Instance == null))
		{
			NotificationText.Push(Instance.LevelChangeNotificationFormat.Format(new
			{
				level = newLevel
			}));
		}
	}

	public static bool AddExp(int amount)
	{
		if (instance == null)
		{
			return false;
		}
		EXP += amount;
		return true;
	}

	private void CacheExp()
	{
		cachedExp = point;
	}

	public object GenerateSaveData()
	{
		return point;
	}

	public void SetupSaveData(object data)
	{
		if (data is long num)
		{
			point = num;
		}
	}

	private void Load()
	{
		if (SavesSystem.KeyExisits(realKey))
		{
			long num = SavesSystem.Load<long>(realKey);
			SetupSaveData(num);
		}
	}

	private void Save()
	{
		object obj = GenerateSaveData();
		SavesSystem.Save(realKey, (long)obj);
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Debug.LogWarning("检测到多个ExpManager");
		}
		SavesSystem.OnSetFile += Load;
		SavesSystem.OnCollectSaveData += Save;
		LevelManager.OnLevelInitialized += CacheExp;
	}

	private void Start()
	{
		Load();
		CacheExp();
	}

	private void OnDestroy()
	{
		SavesSystem.OnSetFile -= Load;
		SavesSystem.OnCollectSaveData -= Save;
		LevelManager.OnLevelInitialized -= CacheExp;
	}

	public int LevelFromExp(long exp)
	{
		for (int i = 0; i < levelExpDefinition.Count; i++)
		{
			long num = levelExpDefinition[i];
			if (exp < num)
			{
				return i - 1;
			}
		}
		return levelExpDefinition.Count - 1;
	}

	public (long from, long to) GetLevelExpRange(int level)
	{
		int num = levelExpDefinition.Count - 1;
		if (level >= num)
		{
			List<long> list = levelExpDefinition;
			return (from: list[list.Count - 1], to: long.MaxValue);
		}
		long item = levelExpDefinition[level];
		long item2 = levelExpDefinition[level + 1];
		return (from: item, to: item2);
	}
}
