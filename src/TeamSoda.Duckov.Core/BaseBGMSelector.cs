using System;
using Duckov;
using Saves;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

public class BaseBGMSelector : MonoBehaviour
{
	[Serializable]
	public struct Entry
	{
		public string switchName;

		public string musicName;

		public string author;
	}

	[SerializeField]
	private string switchGroupName = "BGM";

	[SerializeField]
	private DialogueBubbleProxy proxy;

	public Entry[] entries;

	private int index;

	private const string savekey = "BaseBGMSelector";

	private bool waitForStinger;

	[LocalizationKey("Default")]
	private string BGMInfoFormatKey
	{
		get
		{
			return "BGMInfoFormat";
		}
		set
		{
		}
	}

	private string BGMInfoFormat => BGMInfoFormatKey.ToPlainText();

	private void Awake()
	{
		SavesSystem.OnCollectSaveData += Save;
		Load();
		waitForStinger = true;
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= Save;
	}

	private void Update()
	{
		if (waitForStinger && LevelManager.AfterInit && !AudioManager.IsStingerPlaying)
		{
			waitForStinger = false;
			Set(index);
		}
	}

	private void Load(bool play = false)
	{
		index = SavesSystem.Load<int>("BaseBGMSelector");
		Set(index, showInfo: false, play);
	}

	private void Save()
	{
		SavesSystem.Save("BaseBGMSelector", index);
	}

	public void Set(int index, bool showInfo = false, bool play = true)
	{
		waitForStinger = false;
		if (index < 0 || index >= entries.Length)
		{
			int num = index;
			index = Mathf.Clamp(index, 0, entries.Length - 1);
			Debug.LogError($"Index {num} Out Of Range,clampped to {index}");
		}
		Entry entry = entries[index];
		AudioManager.StopBGM();
		if (play)
		{
			AudioManager.PlayBGM(entry.switchName);
		}
		if (showInfo)
		{
			string text = BGMInfoFormat.Format(new
			{
				name = entry.musicName,
				author = entry.author,
				index = index + 1
			});
			proxy.Pop(text, 200f);
		}
	}

	public void Set(string switchName)
	{
		int num = GetIndex(switchName);
		if (num >= 0)
		{
			Set(num);
		}
	}

	public int GetIndex(string switchName)
	{
		for (int i = 0; i < entries.Length; i++)
		{
			if (entries[i].switchName == switchName)
			{
				return i;
			}
		}
		return -1;
	}

	public void SetNext()
	{
		index++;
		if (index >= entries.Length)
		{
			index = 0;
		}
		Set(index, showInfo: true);
	}

	public void SetPrevious()
	{
		index--;
		if (index < 0)
		{
			index = entries.Length - 1;
		}
		Set(index, showInfo: true);
	}
}
