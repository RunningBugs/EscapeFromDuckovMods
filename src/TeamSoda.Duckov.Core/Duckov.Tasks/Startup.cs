using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Duckov.Utilities;
using Saves;
using UnityEngine;

namespace Duckov.Tasks;

public class Startup : MonoBehaviour
{
	public List<MonoBehaviour> beforeSequence = new List<MonoBehaviour>();

	public List<MonoBehaviour> waitForTasks = new List<MonoBehaviour>();

	private void Awake()
	{
		MoveOldSaves();
	}

	private void MoveOldSaves()
	{
		string fullPathToSavesFolder = SavesSystem.GetFullPathToSavesFolder();
		if (!Directory.Exists(fullPathToSavesFolder))
		{
			Directory.CreateDirectory(fullPathToSavesFolder);
		}
		for (int i = 1; i <= 3; i++)
		{
			string saveFileName = SavesSystem.GetSaveFileName(i);
			string text = Path.Combine(Application.persistentDataPath, saveFileName);
			string text2 = Path.Combine(fullPathToSavesFolder, saveFileName);
			if (File.Exists(text) && !File.Exists(text2))
			{
				Debug.Log("Transporting:\n" + text + "\n->\n" + text2);
				SavesSystem.UpgradeSaveFileAssemblyInfo(text);
				File.Move(text, text2);
			}
		}
		string path = "Options.ES3";
		string text3 = Path.Combine(Application.persistentDataPath, path);
		string text4 = Path.Combine(fullPathToSavesFolder, path);
		if (File.Exists(text3) && !File.Exists(text4))
		{
			Debug.Log("Transporting:\n" + text3 + "\n->\n" + text4);
			SavesSystem.UpgradeSaveFileAssemblyInfo(text3);
			File.Move(text3, text4);
		}
		string globalSaveDataFileName = SavesSystem.GlobalSaveDataFileName;
		string text5 = Path.Combine(Application.persistentDataPath, globalSaveDataFileName);
		string text6 = Path.Combine(fullPathToSavesFolder, globalSaveDataFileName);
		if (!File.Exists(text5))
		{
			text5 = Path.Combine(Application.persistentDataPath, "Global.csv");
		}
		if (File.Exists(text5) && !File.Exists(text6))
		{
			Debug.Log("Transporting:\n" + text5 + "\n->\n" + text6);
			SavesSystem.UpgradeSaveFileAssemblyInfo(text5);
			File.Move(text5, text6);
		}
	}

	private void Start()
	{
		StartupFlow().Forget();
	}

	private async UniTask StartupFlow()
	{
		foreach (MonoBehaviour item in beforeSequence)
		{
			if (!(item == null) && item is ITaskBehaviour task)
			{
				task.Begin();
				while (task.IsPending())
				{
					await UniTask.Yield();
				}
			}
		}
		while (!EvaluateWaitList())
		{
			await UniTask.Yield();
		}
		SceneLoader.StaticLoadSingle(GameplayDataSettings.SceneManagement.MainMenuScene);
	}

	private bool EvaluateWaitList()
	{
		foreach (MonoBehaviour waitForTask in waitForTasks)
		{
			if (!(waitForTask == null) && waitForTask is ITaskBehaviour taskBehaviour && !taskBehaviour.IsComplete())
			{
				return false;
			}
		}
		return true;
	}
}
