using System;
using System.IO;
using Saves;
using UnityEngine;

namespace Duckov.Options;

public class OptionsManager : MonoBehaviour
{
	public const string FileName = "Options.ES3";

	private static ES3Settings _saveSettings;

	private static string Folder => SavesSystem.SavesFolder;

	public static string FilePath => Path.Combine(Folder, "Options.ES3");

	private static ES3Settings SaveSettings
	{
		get
		{
			if (_saveSettings == null)
			{
				_saveSettings = new ES3Settings(applyDefaults: true);
				_saveSettings.path = FilePath;
				_saveSettings.location = ES3.Location.File;
			}
			return _saveSettings;
		}
	}

	public static float MouseSensitivity
	{
		get
		{
			return Load("MouseSensitivity", 10f);
		}
		set
		{
			Save("MouseSensitivity", value);
		}
	}

	public static event Action<string> OnOptionsChanged;

	public static void Save<T>(string key, T obj)
	{
		if (string.IsNullOrEmpty(key))
		{
			return;
		}
		try
		{
			ES3.Save(key, obj, SaveSettings);
			OptionsManager.OnOptionsChanged?.Invoke(key);
			ES3.CreateBackup(SaveSettings);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.LogError("Error: Failed saving options: " + key);
		}
	}

	public static T Load<T>(string key, T defaultValue = default(T))
	{
		if (string.IsNullOrEmpty(key))
		{
			return default(T);
		}
		try
		{
			if (ES3.KeyExists(key, SaveSettings))
			{
				return ES3.Load<T>(key, SaveSettings);
			}
			ES3.Save(key, defaultValue, SaveSettings);
			return defaultValue;
		}
		catch
		{
			if (ES3.RestoreBackup(SaveSettings))
			{
				try
				{
					if (ES3.KeyExists(key, SaveSettings))
					{
						return ES3.Load<T>(key, SaveSettings);
					}
					ES3.Save(key, defaultValue, SaveSettings);
					return defaultValue;
				}
				catch
				{
					Debug.LogError("[OPTIONS MANAGER] Failed restoring backup");
				}
			}
			ES3.DeleteFile(SaveSettings);
			return defaultValue;
		}
	}
}
