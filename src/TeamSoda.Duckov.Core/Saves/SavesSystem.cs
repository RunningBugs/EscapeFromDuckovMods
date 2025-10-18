using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Saves;

public class SavesSystem
{
	public struct BackupInfo
	{
		public int slot;

		public int index;

		public string path;

		public bool exists;

		public long time_raw;

		public bool TimeValid => time_raw > 0;

		public DateTime Time => DateTime.FromBinary(time_raw);
	}

	private static int? _currentSlot = null;

	private static bool saving;

	private static ES3Settings settings = ES3Settings.defaultSettings;

	private static bool cached;

	private const int BackupListCount = 10;

	private static DateTime _lastSavedTime = DateTime.MinValue;

	private static DateTime _lastIndexedBackupTime = DateTime.MinValue;

	private static bool globalCached;

	private static ES3Settings GlobalFileSetting = new ES3Settings
	{
		location = ES3.Location.File
	};

	public static int CurrentSlot
	{
		get
		{
			if (!_currentSlot.HasValue)
			{
				_currentSlot = PlayerPrefs.GetInt("CurrentSlot", 1);
				if (_currentSlot < 1)
				{
					_currentSlot = 1;
				}
			}
			return _currentSlot.Value;
		}
		private set
		{
			_currentSlot = value;
			PlayerPrefs.SetInt("CurrentSlot", value);
			CacheFile();
		}
	}

	public static string CurrentFilePath => GetFilePath(CurrentSlot);

	public static bool IsSaving => saving;

	public static string SavesFolder => "Saves";

	public static bool RestoreFailureMarker { get; private set; }

	private static DateTime LastSavedTime
	{
		get
		{
			if (_lastSavedTime > DateTime.UtcNow)
			{
				_lastSavedTime = DateTime.UtcNow;
				GameManager.TimeTravelDetected();
			}
			return _lastSavedTime;
		}
		set
		{
			_lastSavedTime = value;
		}
	}

	private static TimeSpan TimeSinceLastSave => DateTime.UtcNow - LastSavedTime;

	private static DateTime LastIndexedBackupTime
	{
		get
		{
			if (_lastIndexedBackupTime > DateTime.UtcNow)
			{
				_lastIndexedBackupTime = DateTime.UtcNow;
				GameManager.TimeTravelDetected();
			}
			return _lastIndexedBackupTime;
		}
		set
		{
			_lastIndexedBackupTime = value;
		}
	}

	private static TimeSpan TimeSinceLastIndexedBackup => DateTime.UtcNow - LastIndexedBackupTime;

	public static string GlobalSaveDataFilePath => Path.Combine(SavesFolder, GlobalSaveDataFileName);

	public static string GlobalSaveDataFileName => "Global.json";

	public static event Action OnSetFile;

	public static event Action OnSaveDeleted;

	public static event Action OnCollectSaveData;

	public static event Action OnRestoreFailureDetected;

	public static string GetFullPathToSavesFolder()
	{
		return Path.Combine(Application.persistentDataPath, SavesFolder);
	}

	public static string GetFilePath(int slot)
	{
		return Path.Combine(SavesFolder, GetSaveFileName(slot));
	}

	public static string GetSaveFileName(int slot)
	{
		return $"Save_{slot}.sav";
	}

	public static bool IsOldSave(int index)
	{
		return !KeyExisits("CreatedWithVersion", index);
	}

	public static void SetFile(int index)
	{
		cached = false;
		CurrentSlot = index;
		SavesSystem.OnSetFile?.Invoke();
	}

	public static BackupInfo[] GetBackupList()
	{
		return GetBackupList(CurrentSlot);
	}

	public static BackupInfo[] GetBackupList(int slot)
	{
		return GetBackupList(GetFilePath(slot), slot);
	}

	public static BackupInfo[] GetBackupList(string mainPath, int slot = -1)
	{
		BackupInfo[] array = new BackupInfo[10];
		for (int i = 0; i < 10; i++)
		{
			try
			{
				string backupPathByIndex = GetBackupPathByIndex(mainPath, i);
				ES3Settings eS3Settings = new ES3Settings(backupPathByIndex);
				eS3Settings.location = ES3.Location.File;
				bool flag = ES3.FileExists(backupPathByIndex, eS3Settings);
				long num = 0L;
				if (flag && ES3.KeyExists("SaveTime", backupPathByIndex, eS3Settings))
				{
					num = ES3.Load<long>("SaveTime", backupPathByIndex, eS3Settings);
				}
				DateTime.FromBinary(num);
				BackupInfo backupInfo = new BackupInfo
				{
					slot = slot,
					index = i,
					path = backupPathByIndex,
					exists = flag,
					time_raw = num
				};
				array[i] = backupInfo;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				array[i] = default(BackupInfo);
			}
		}
		return array;
	}

	private static int GetEmptyOrOldestBackupIndex()
	{
		BackupInfo[] backupList = GetBackupList();
		int result = -1;
		DateTime dateTime = DateTime.MaxValue;
		BackupInfo[] array = backupList;
		for (int i = 0; i < array.Length; i++)
		{
			BackupInfo backupInfo = array[i];
			if (!backupInfo.exists)
			{
				return backupInfo.index;
			}
			if (backupInfo.Time < dateTime)
			{
				result = backupInfo.index;
				dateTime = backupInfo.Time;
			}
		}
		return result;
	}

	private static int GetOldestBackupIndex()
	{
		BackupInfo[] backupList = GetBackupList();
		int result = -1;
		DateTime dateTime = DateTime.MaxValue;
		BackupInfo[] array = backupList;
		for (int i = 0; i < array.Length; i++)
		{
			BackupInfo backupInfo = array[i];
			if (backupInfo.exists && backupInfo.Time < dateTime)
			{
				result = backupInfo.index;
				dateTime = backupInfo.Time;
			}
		}
		return result;
	}

	private static int GetNewestBackupIndex()
	{
		BackupInfo[] backupList = GetBackupList();
		int result = -1;
		DateTime dateTime = DateTime.MinValue;
		BackupInfo[] array = backupList;
		for (int i = 0; i < array.Length; i++)
		{
			BackupInfo backupInfo = array[i];
			if (backupInfo.exists && backupInfo.Time > dateTime)
			{
				result = backupInfo.index;
				dateTime = backupInfo.Time;
			}
		}
		return result;
	}

	private static string GetBackupPathByIndex(int index)
	{
		return GetBackupPathByIndex(CurrentSlot, index);
	}

	private static string GetBackupPathByIndex(int slot, int index)
	{
		return GetBackupPathByIndex(GetFilePath(slot), index);
	}

	private static string GetBackupPathByIndex(string path, int index)
	{
		return $"{path}.bac.{index + 1:00}";
	}

	private static void CreateIndexedBackup(int index = -1)
	{
		LastIndexedBackupTime = DateTime.UtcNow;
		try
		{
			if (index < 0)
			{
				index = GetEmptyOrOldestBackupIndex();
			}
			string backupPathByIndex = GetBackupPathByIndex(index);
			ES3.DeleteFile(backupPathByIndex, settings);
			ES3.CopyFile(CurrentFilePath, backupPathByIndex);
			ES3.StoreCachedFile(backupPathByIndex);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.Log("[Saves] Failed creating indexed backup");
		}
	}

	private static void CreateBackup()
	{
		try
		{
			CreateBackup(CurrentFilePath);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.Log("[Saves] Failed creating backup");
		}
	}

	private static void CreateBackup(string path)
	{
		try
		{
			string filePath = path + ".bac";
			ES3.DeleteFile(filePath, settings);
			ES3.CreateBackup(path);
			ES3.StoreCachedFile(filePath);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.Log("[Saves] Failed creating backup for path " + path);
		}
	}

	public static void UpgradeSaveFileAssemblyInfo(string path)
	{
		if (!File.Exists(path))
		{
			Debug.Log("没有找到存档文件：" + path);
			return;
		}
		string text;
		using (StreamReader streamReader = File.OpenText(path))
		{
			text = streamReader.ReadToEnd();
			if (text.Contains("TeamSoda.Duckov.Core"))
			{
				streamReader.Close();
				return;
			}
			text = text.Replace("Assembly-CSharp", "TeamSoda.Duckov.Core");
			streamReader.Close();
		}
		File.Delete(path);
		using (FileStream fileStream = File.OpenWrite(path))
		{
			StreamWriter streamWriter = new StreamWriter(fileStream);
			streamWriter.Write(text);
			streamWriter.Close();
			fileStream.Close();
		}
		Debug.Log("存档格式已更新：" + path);
	}

	public static void RestoreIndexedBackup(int slot, int index)
	{
		string backupPathByIndex = GetBackupPathByIndex(slot, index);
		UpgradeSaveFileAssemblyInfo(Path.Combine(Application.persistentDataPath, backupPathByIndex));
		string filePath = GetFilePath(slot);
		string text = filePath + ".bac";
		try
		{
			ES3.CacheFile(backupPathByIndex);
			ES3.DeleteFile(text, settings);
			ES3.CopyFile(backupPathByIndex, text);
			ES3.DeleteFile(filePath, settings);
			ES3.RestoreBackup(filePath, settings);
			ES3.StoreCachedFile(filePath);
			ES3.CacheFile(filePath);
			SavesSystem.OnSetFile?.Invoke();
		}
		catch
		{
			RestoreFailureMarker = true;
			Debug.LogError("文件损坏，且无法修复。");
			ES3.DeleteFile(filePath);
			File.Delete(filePath);
			ES3.Save("Created", value: true, filePath);
			ES3.StoreCachedFile(filePath);
			ES3.CacheFile(filePath);
			SavesSystem.OnRestoreFailureDetected?.Invoke();
		}
	}

	private static bool RestoreBackup(string path)
	{
		bool flag = false;
		try
		{
			string text = path + ".bac";
			UpgradeSaveFileAssemblyInfo(Path.Combine(Application.persistentDataPath, text));
			ES3.CacheFile(text);
			ES3.DeleteFile(path, settings);
			ES3.RestoreBackup(path, settings);
			ES3.StoreCachedFile(path);
			ES3.CacheFile(path);
			ES3.CacheFile(path);
			flag = true;
		}
		catch
		{
			Debug.Log("默认备份损坏。");
		}
		if (!flag)
		{
			RestoreFailureMarker = true;
			Debug.LogError("恢复默认备份失败");
			ES3.DeleteFile(path);
			ES3.Save("Created", value: true, path);
			ES3.StoreCachedFile(path);
			ES3.CacheFile(path);
			SavesSystem.OnRestoreFailureDetected?.Invoke();
		}
		return flag;
	}

	public DateTime GetSaveTimeUTC(int slot = -1)
	{
		if (slot < 0)
		{
			slot = CurrentSlot;
		}
		if (!KeyExisits("SaveTime", slot))
		{
			return default(DateTime);
		}
		return DateTime.FromBinary(Load<long>("SaveTime", slot));
	}

	public DateTime GetSaveTimeLocal(int slot = -1)
	{
		if (slot < 0)
		{
			slot = CurrentSlot;
		}
		DateTime saveTimeUTC = GetSaveTimeUTC(slot);
		if (saveTimeUTC == default(DateTime))
		{
			return default(DateTime);
		}
		return saveTimeUTC.ToLocalTime();
	}

	public static void SaveFile(bool writeSaveTime = true)
	{
		TimeSpan timeSinceLastIndexedBackup = TimeSinceLastIndexedBackup;
		LastSavedTime = DateTime.UtcNow;
		if (writeSaveTime)
		{
			Save("SaveTime", DateTime.UtcNow.ToBinary());
		}
		saving = true;
		CreateBackup();
		if (timeSinceLastIndexedBackup > TimeSpan.FromMinutes(5.0))
		{
			CreateIndexedBackup();
		}
		SetAsOldGame();
		ES3.StoreCachedFile(CurrentFilePath);
		saving = false;
	}

	private static void CacheFile()
	{
		CacheFile(CurrentSlot);
		cached = true;
	}

	private static void CacheFile(int slot)
	{
		if (slot == CurrentSlot && cached)
		{
			return;
		}
		string filePath = GetFilePath(slot);
		if (!CacheFile(filePath))
		{
			Debug.Log("尝试恢复 indexed backups");
			List<BackupInfo> list = (from e in GetBackupList(filePath, slot)
				where e.exists
				select e).ToList();
			list.Sort((BackupInfo a, BackupInfo b) => (!(a.Time > b.Time)) ? 1 : (-1));
			bool flag = false;
			if (list.Count > 0)
			{
				for (int num = 0; num < list.Count; num++)
				{
					BackupInfo backupInfo = list[num];
					try
					{
						Debug.Log($"Restoreing {slot}.bac.{backupInfo.index} \t" + backupInfo.Time.ToString("MM/dd HH:mm:ss"));
						RestoreIndexedBackup(slot, backupInfo.index);
						flag = true;
					}
					catch
					{
						Debug.LogError($"slot:{slot} backup_index:{backupInfo.index} 恢复失败。");
						continue;
					}
					break;
				}
			}
		}
		if (!ES3.FileExists(filePath))
		{
			ES3.Save("Created", value: true, filePath);
			ES3.StoreCachedFile(filePath);
			ES3.CacheFile(filePath);
		}
	}

	private static bool CacheFile(string path)
	{
		try
		{
			ES3.CacheFile(path);
			return true;
		}
		catch
		{
			return RestoreBackup(path);
		}
	}

	public static void Save<T>(string prefix, string key, T value)
	{
		Save(prefix + key, value);
	}

	public static void Save<T>(string realKey, T value)
	{
		if (!cached)
		{
			CacheFile();
		}
		if (string.IsNullOrWhiteSpace(CurrentFilePath))
		{
			Debug.Log("Save failed " + realKey);
		}
		else
		{
			ES3.Save(realKey, value, CurrentFilePath);
		}
	}

	public static T Load<T>(string prefix, string key)
	{
		return Load<T>(prefix + key);
	}

	public static T Load<T>(string realKey)
	{
		if (!cached)
		{
			CacheFile();
		}
		string.IsNullOrWhiteSpace(realKey);
		if (ES3.KeyExists(realKey, CurrentFilePath))
		{
			return ES3.Load<T>(realKey, CurrentFilePath);
		}
		return default(T);
	}

	public static bool KeyExisits(string prefix, string key)
	{
		return ES3.KeyExists(prefix + key);
	}

	public static bool KeyExisits(string realKey)
	{
		if (!cached)
		{
			CacheFile();
		}
		return ES3.KeyExists(realKey, CurrentFilePath);
	}

	public static bool KeyExisits(string realKey, int slotIndex)
	{
		if (slotIndex == CurrentSlot)
		{
			return KeyExisits(realKey);
		}
		string filePath = GetFilePath(slotIndex);
		CacheFile(slotIndex);
		return ES3.KeyExists(realKey, filePath);
	}

	public static T Load<T>(string realKey, int slotIndex)
	{
		if (slotIndex == CurrentSlot)
		{
			return Load<T>(realKey);
		}
		string filePath = GetFilePath(slotIndex);
		CacheFile(slotIndex);
		if (ES3.KeyExists(realKey, filePath))
		{
			return ES3.Load<T>(realKey, filePath);
		}
		return default(T);
	}

	public static void SaveGlobal<T>(string key, T value)
	{
		if (!globalCached)
		{
			CacheFile(GlobalSaveDataFilePath);
			globalCached = true;
		}
		ES3.Save(key, value, GlobalSaveDataFilePath);
		CreateBackup(GlobalSaveDataFilePath);
		ES3.StoreCachedFile(GlobalSaveDataFilePath);
	}

	public static T LoadGlobal<T>(string key, T defaultValue = default(T))
	{
		if (!globalCached)
		{
			CacheFile(GlobalSaveDataFilePath);
			globalCached = true;
		}
		if (ES3.KeyExists(key, GlobalSaveDataFilePath))
		{
			return ES3.Load<T>(key, GlobalSaveDataFilePath);
		}
		return defaultValue;
	}

	public static void CollectSaveData()
	{
		SavesSystem.OnCollectSaveData?.Invoke();
	}

	public static bool IsOldGame()
	{
		return Load<bool>("IsOldGame");
	}

	public static bool IsOldGame(int index)
	{
		return Load<bool>("IsOldGame", index);
	}

	private static void SetAsOldGame()
	{
		Save("IsOldGame", value: true);
	}

	public static void DeleteCurrentSave()
	{
		ES3.CacheFile(CurrentFilePath);
		ES3.DeleteFile(CurrentFilePath);
		ES3.Save("Created", value: false, CurrentFilePath);
		ES3.StoreCachedFile(CurrentFilePath);
		Debug.Log($"已删除存档{CurrentSlot}");
		SavesSystem.OnSaveDeleted?.Invoke();
	}
}
