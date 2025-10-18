using Saves;
using UnityEngine;

namespace Duckov.Utilities;

public class CommonVariables : MonoBehaviour
{
	private static CommonVariables instance;

	[SerializeField]
	private CustomDataCollection data;

	private const string saves_prefix = "CommonVariables";

	private const string saves_key = "Data";

	public CustomDataCollection Data => data;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Debug.LogWarning("检测到多个Common Variables");
		}
		SavesSystem.OnCollectSaveData += OnCollectSaveData;
		SavesSystem.OnSetFile += OnSetSaveFile;
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= OnCollectSaveData;
		SavesSystem.OnSetFile -= OnSetSaveFile;
	}

	private void OnSetSaveFile()
	{
		Load();
	}

	private void OnCollectSaveData()
	{
		Save();
	}

	private void Start()
	{
		Load();
	}

	private void Save()
	{
		SavesSystem.Save("CommonVariables", "Data", data);
	}

	private void Load()
	{
		data = SavesSystem.Load<CustomDataCollection>("CommonVariables", "Data");
		if (data == null)
		{
			data = new CustomDataCollection();
		}
	}

	public static void SetFloat(string key, float value)
	{
		if ((bool)instance)
		{
			instance.Data.SetFloat(key, value);
		}
	}

	public static void SetInt(string key, int value)
	{
		if ((bool)instance)
		{
			instance.Data.SetInt(key, value);
		}
	}

	public static void SetBool(string key, bool value)
	{
		if ((bool)instance)
		{
			instance.Data.SetBool(key, value);
		}
	}

	public static void SetString(string key, string value)
	{
		if ((bool)instance)
		{
			instance.Data.SetString(key, value);
		}
	}

	public static float GetFloat(string key, float defaultValue = 0f)
	{
		if ((bool)instance)
		{
			return instance.Data.GetFloat(key, defaultValue);
		}
		return defaultValue;
	}

	public static int GetInt(string key, int defaultValue = 0)
	{
		if ((bool)instance)
		{
			return instance.Data.GetInt(key, defaultValue);
		}
		return defaultValue;
	}

	public static bool GetBool(string key, bool defaultValue = false)
	{
		if ((bool)instance)
		{
			return instance.Data.GetBool(key, defaultValue);
		}
		return defaultValue;
	}

	public static string GetString(string key, string defaultValue = "")
	{
		if ((bool)instance)
		{
			return instance.Data.GetString(key, defaultValue);
		}
		return defaultValue;
	}

	public static float GetFloat(int hash, float defaultValue = 0f)
	{
		if ((bool)instance)
		{
			return instance.Data.GetFloat(hash, defaultValue);
		}
		return defaultValue;
	}

	public static int GetInt(int hash, int defaultValue = 0)
	{
		if ((bool)instance)
		{
			return instance.Data.GetInt(hash, defaultValue);
		}
		return defaultValue;
	}

	public static bool GetBool(int hash, bool defaultValue = false)
	{
		if ((bool)instance)
		{
			return instance.Data.GetBool(hash, defaultValue);
		}
		return defaultValue;
	}

	public static string GetString(int hash, string defaultValue = "")
	{
		if ((bool)instance)
		{
			return instance.Data.GetString(hash, defaultValue);
		}
		return defaultValue;
	}
}
