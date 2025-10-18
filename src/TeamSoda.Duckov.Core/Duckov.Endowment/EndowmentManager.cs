using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Duckov.Achievements;
using Saves;
using UnityEngine;

namespace Duckov.Endowment;

public class EndowmentManager : MonoBehaviour
{
	private const string SaveKey = "Endowment_SelectedIndex";

	public static Action<EndowmentIndex> OnEndowmentChanged;

	public static Action<EndowmentIndex> OnEndowmentUnlock;

	[SerializeField]
	private List<EndowmentEntry> entries = new List<EndowmentEntry>();

	private ReadOnlyCollection<EndowmentEntry> _entries_ReadOnly;

	private static EndowmentManager _instance { get; set; }

	public static EndowmentManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_ = GameManager.Instance;
			}
			return _instance;
		}
	}

	public static EndowmentIndex SelectedIndex
	{
		get
		{
			return SavesSystem.Load<EndowmentIndex>("Endowment_SelectedIndex");
		}
		private set
		{
			SavesSystem.Save("Endowment_SelectedIndex", value);
		}
	}

	public ReadOnlyCollection<EndowmentEntry> Entries
	{
		get
		{
			if (_entries_ReadOnly == null)
			{
				_entries_ReadOnly = new ReadOnlyCollection<EndowmentEntry>(entries);
			}
			return _entries_ReadOnly;
		}
	}

	public static EndowmentEntry Current
	{
		get
		{
			if (_instance == null)
			{
				return null;
			}
			return _instance.entries.Find((EndowmentEntry e) => e != null && e.Index == SelectedIndex);
		}
	}

	public static EndowmentIndex CurrentIndex
	{
		get
		{
			if (Current == null)
			{
				return EndowmentIndex.None;
			}
			return Current.Index;
		}
	}

	private EndowmentEntry GetEntry(EndowmentIndex index)
	{
		return entries.Find((EndowmentEntry e) => e != null && e.Index == index);
	}

	private static string GetUnlockKey(EndowmentIndex index)
	{
		return $"Endowment_Unlock_R_{index}";
	}

	public static bool GetEndowmentUnlocked(EndowmentIndex index)
	{
		if (Instance != null)
		{
			if (Instance.GetEntry(index).UnlockedByDefault)
			{
				return true;
			}
		}
		else
		{
			Debug.LogError("Endowment Manager 不存在。");
		}
		return SavesSystem.LoadGlobal(GetUnlockKey(index), defaultValue: false);
	}

	private static void SetEndowmentUnlocked(EndowmentIndex index, bool value = true)
	{
		SavesSystem.SaveGlobal(GetUnlockKey(index), value);
	}

	public static bool UnlockEndowment(EndowmentIndex index)
	{
		try
		{
			OnEndowmentUnlock?.Invoke(index);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		if (GetEndowmentUnlocked(index))
		{
			Debug.Log("尝试解锁天赋，但天赋已经解锁");
			return false;
		}
		SetEndowmentUnlocked(index);
		return true;
	}

	private void Awake()
	{
		if (_instance != null)
		{
			Debug.LogError("检测到多个Endowment Manager");
			return;
		}
		_instance = this;
		if (LevelManager.LevelInited)
		{
			ApplyCurrentEndowment();
		}
		LevelManager.OnLevelInitialized += OnLevelInitialized;
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelInitialized -= OnLevelInitialized;
	}

	private void OnLevelInitialized()
	{
		ApplyCurrentEndowment();
		MakeSureEndowmentAchievementsUnlocked();
	}

	private void MakeSureEndowmentAchievementsUnlocked()
	{
		for (int i = 0; i < 5; i++)
		{
			EndowmentIndex index = (EndowmentIndex)i;
			EndowmentEntry entry = Instance.GetEntry(index);
			if (!(entry == null) && !entry.UnlockedByDefault && GetEndowmentUnlocked(index))
			{
				AchievementManager.UnlockEndowmentAchievement(index);
			}
		}
	}

	private void ApplyCurrentEndowment()
	{
		if (!LevelManager.LevelInited)
		{
			return;
		}
		foreach (EndowmentEntry entry in entries)
		{
			if (!(entry == null))
			{
				entry.Deactivate();
			}
		}
		EndowmentEntry current2 = Current;
		if (!(current2 == null))
		{
			current2.Activate();
		}
	}

	internal void SelectIndex(EndowmentIndex index)
	{
		SelectedIndex = index;
		ApplyCurrentEndowment();
		OnEndowmentChanged?.Invoke(index);
	}
}
