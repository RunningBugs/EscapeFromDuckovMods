using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Saves;
using Sirenix.Utilities;
using UnityEngine;

namespace Duckov.Modding;

public class ModManager : MonoBehaviour
{
	[SerializeField]
	private Transform modParent;

	public static Action<string, string> OnModLoadingFailed;

	public static List<ModInfo> modInfos = new List<ModInfo>();

	private Dictionary<string, ModBehaviour> activeMods = new Dictionary<string, ModBehaviour>();

	public static bool AllowActivatingMod
	{
		get
		{
			return SavesSystem.LoadGlobal("AllowLoadingMod", defaultValue: false);
		}
		set
		{
			SavesSystem.SaveGlobal("AllowLoadingMod", value);
			if (Instance != null)
			{
				Instance.ScanAndActivateMods();
			}
		}
	}

	private static string DefaultModFolderPath => Path.Combine(Application.dataPath, "Mods");

	public static ModManager Instance => GameManager.ModManager;

	public static event Action<List<ModInfo>> OnScan;

	public static event Action<ModInfo, ModBehaviour> OnModActivated;

	public static event Action<ModInfo, ModBehaviour> OnModWillBeDeactivated;

	public static event Action OnModStatusChanged;

	private void Awake()
	{
		if (modParent == null)
		{
			modParent = base.transform;
		}
	}

	private void Start()
	{
		ScanAndActivateMods();
	}

	public void ScanAndActivateMods()
	{
		if (!AllowActivatingMod)
		{
			return;
		}
		Rescan();
		foreach (ModInfo modInfo in modInfos)
		{
			if (!activeMods.ContainsKey(modInfo.name))
			{
				bool flag = ShouldActivateMod(modInfo);
				Debug.Log($"ModActive_{modInfo.name}: {flag}");
				if (flag && ActivateMod(modInfo) == null)
				{
					SetShouldActivateMod(modInfo, value: false);
				}
			}
		}
	}

	private void SetShouldActivateMod(ModInfo info, bool value)
	{
		SavesSystem.SaveGlobal("ModActive_" + info.name, value);
	}

	private bool ShouldActivateMod(ModInfo info)
	{
		return SavesSystem.LoadGlobal("ModActive_" + info.name, defaultValue: false);
	}

	public static void Rescan()
	{
		modInfos.Clear();
		if (Directory.Exists(DefaultModFolderPath))
		{
			string[] directories = Directory.GetDirectories(DefaultModFolderPath);
			for (int i = 0; i < directories.Length; i++)
			{
				if (TryProcessModFolder(directories[i], out var info, isSteamItem: false, 0uL))
				{
					modInfos.Add(info);
				}
			}
		}
		ModManager.OnScan?.Invoke(modInfos);
	}

	public static bool TryProcessModFolder(string path, out ModInfo info, bool isSteamItem = false, ulong publishedFileId = 0uL)
	{
		info = default(ModInfo);
		info.path = path;
		string path2 = Path.Combine(path, "info.ini");
		if (!File.Exists(path2))
		{
			return false;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		using (StreamReader streamReader = File.OpenText(path2))
		{
			while (!streamReader.EndOfStream)
			{
				string text = streamReader.ReadLine().Trim();
				if (!string.IsNullOrWhiteSpace(text) && !text.StartsWith('['))
				{
					string[] array = text.Split('=');
					if (array.Length == 2)
					{
						string key = array[0].Trim();
						string value = array[1].Trim();
						dictionary[key] = value;
					}
				}
			}
		}
		if (!dictionary.TryGetValue("name", out var value2))
		{
			Debug.LogError("Failed to get name value in mod info.ini file. Aborting.\n" + path);
			return false;
		}
		if (!dictionary.TryGetValue("displayName", out var value3))
		{
			value3 = value2;
			Debug.LogError("Failed to get displayName value in mod info.ini file.\n" + path);
		}
		if (!dictionary.TryGetValue("description", out var value4))
		{
			value4 = "?";
			Debug.LogError("Failed to get description value in mod info.ini file.\n" + path);
		}
		ulong result = 0uL;
		if (dictionary.TryGetValue("publishedFileId", out var value5) && !ulong.TryParse(value5, out result))
		{
			Debug.LogError("Invalid publishedFileId");
		}
		if (!isSteamItem)
		{
			publishedFileId = result;
		}
		else if (publishedFileId != result)
		{
			Debug.LogError("PublishFileId not match.\npath:" + path);
		}
		info.name = value2;
		info.displayName = value3;
		info.description = value4;
		info.publishedFileId = publishedFileId;
		info.isSteamItem = isSteamItem;
		string dllPath = info.dllPath;
		info.dllFound = File.Exists(dllPath);
		if (!info.dllFound)
		{
			Debug.LogError("Dll for mod " + value2 + " not found.\nExpecting: " + dllPath);
		}
		string path3 = Path.Combine(path, "preview.png");
		if (File.Exists(path3))
		{
			using FileStream fileStream = File.OpenRead(path3);
			Texture2D texture2D = new Texture2D(256, 256);
			byte[] array2 = new byte[fileStream.Length];
			fileStream.Read(array2);
			if (texture2D.LoadImage(array2))
			{
				info.preview = texture2D;
			}
		}
		return true;
	}

	public static bool IsModActive(ModInfo info, out ModBehaviour instance)
	{
		instance = null;
		if (Instance == null)
		{
			return false;
		}
		if (Instance.activeMods.TryGetValue(info.name, out instance))
		{
			return instance != null;
		}
		return false;
	}

	public ModBehaviour GetActiveModBehaviour(ModInfo info)
	{
		if (activeMods.TryGetValue(info.name, out var value))
		{
			return value;
		}
		return null;
	}

	public void DeactivateMod(ModInfo info)
	{
		ModBehaviour activeModBehaviour = GetActiveModBehaviour(info);
		if (!(activeModBehaviour == null))
		{
			try
			{
				activeModBehaviour.NotifyBeforeDeactivate();
				ModManager.OnModWillBeDeactivated?.Invoke(info, activeModBehaviour);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			activeMods.Remove(info.name);
			try
			{
				UnityEngine.Object.Destroy(activeModBehaviour.gameObject);
				ModManager.OnModStatusChanged?.Invoke();
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			SetShouldActivateMod(info, value: false);
		}
	}

	public ModBehaviour ActivateMod(ModInfo info)
	{
		if (!AllowActivatingMod)
		{
			Debug.LogError("Activating mod not allowed! \nUser must first interact with the agreement UI in order to allow activating mods.");
			return null;
		}
		string dllPath = info.dllPath;
		string text = info.name;
		if (IsModActive(info, out var instance))
		{
			Debug.LogError("Mod " + info.name + " instance already exists! Abort. Path: " + info.path, instance);
			return null;
		}
		Debug.Log("Loading mod dll at path: " + dllPath);
		Type type;
		try
		{
			type = Assembly.LoadFrom(dllPath).GetType(text + ".ModBehaviour");
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			string arg = "Mod loading failed: " + text + "\n" + ex.Message;
			OnModLoadingFailed?.Invoke(info.dllPath, arg);
			return null;
		}
		if (type == null || !type.InheritsFrom<ModBehaviour>())
		{
			Debug.LogError("Cannot load mod.\nA type named " + text + ".Mod is expected, and it should inherit from Duckov.Modding.Mod.");
			return null;
		}
		GameObject gameObject = new GameObject(text);
		ModBehaviour modBehaviour;
		try
		{
			modBehaviour = gameObject.AddComponent(type) as ModBehaviour;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.LogError("Failed to create component for mod " + text);
			return null;
		}
		if (modBehaviour == null)
		{
			UnityEngine.Object.Destroy(gameObject);
			Debug.LogError("Failed to create component for mod " + text);
			return null;
		}
		gameObject.transform.SetParent(base.transform);
		Debug.Log("Mod Loaded: " + info.name);
		modBehaviour.Setup(this, info);
		activeMods[info.name] = modBehaviour;
		try
		{
			ModManager.OnModActivated?.Invoke(info, modBehaviour);
			ModManager.OnModStatusChanged?.Invoke();
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
		SetShouldActivateMod(info, value: true);
		return modBehaviour;
	}

	internal static void WriteModInfoINI(ModInfo modInfo)
	{
		string path = Path.Combine(modInfo.path, "info.ini");
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		using FileStream stream = File.Create(path);
		StreamWriter streamWriter = new StreamWriter(stream);
		streamWriter.WriteLine("name = " + modInfo.name);
		streamWriter.WriteLine("displayName = " + modInfo.displayName);
		streamWriter.WriteLine("description = " + modInfo.description);
		streamWriter.WriteLine("");
		streamWriter.WriteLine($"publishedFileId = {modInfo.publishedFileId}");
		streamWriter.Close();
	}
}
