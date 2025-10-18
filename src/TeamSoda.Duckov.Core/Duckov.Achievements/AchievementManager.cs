using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Economy;
using Duckov.Endowment;
using Duckov.Quests;
using Duckov.Rules.UI;
using Duckov.Scenes;
using Saves;
using UnityEngine;

namespace Duckov.Achievements;

public class AchievementManager : MonoBehaviour
{
	private struct KillCountAchievement
	{
		public string key;

		public int value;

		public KillCountAchievement(string key, int value)
		{
			this.key = key;
			this.value = value;
		}
	}

	private List<string> _unlockedAchievements = new List<string>();

	private readonly string[] evacuateSceneIDs = new string[1] { "Level_GroundZero_Main" };

	private readonly string[] achievementSceneIDs = new string[6] { "Base", "Level_GroundZero_Main", "Level_HiddenWarehouse_Main", "Level_Farm_Main", "Level_JLab_Main", "Level_StormZone_Main" };

	private readonly KillCountAchievement[] KillCountAchivements = new KillCountAchievement[16]
	{
		new KillCountAchievement("Cname_ShortEagle", 10),
		new KillCountAchievement("Cname_ShortEagle", 1),
		new KillCountAchievement("Cname_Speedy", 1),
		new KillCountAchievement("Cname_StormBoss1", 1),
		new KillCountAchievement("Cname_StormBoss2", 1),
		new KillCountAchievement("Cname_StormBoss3", 1),
		new KillCountAchievement("Cname_StormBoss4", 1),
		new KillCountAchievement("Cname_StormBoss5", 1),
		new KillCountAchievement("Cname_Boss_Sniper", 1),
		new KillCountAchievement("Cname_Vida", 1),
		new KillCountAchievement("Cname_Roadblock", 1),
		new KillCountAchievement("Cname_SchoolBully", 1),
		new KillCountAchievement("Cname_Boss_Fly", 1),
		new KillCountAchievement("Cname_Boss_Arcade", 1),
		new KillCountAchievement("Cname_UltraMan", 1),
		new KillCountAchievement("Cname_LabTestObjective", 1)
	};

	public static AchievementManager Instance => GameManager.AchievementManager;

	public static bool CanUnlockAchievement
	{
		get
		{
			if (DifficultySelection.CustomDifficultyMarker)
			{
				return false;
			}
			return true;
		}
	}

	public List<string> UnlockedAchievements => _unlockedAchievements;

	public static event Action<AchievementManager> OnAchievementDataLoaded;

	public static event Action<string> OnAchievementUnlocked;

	private void Awake()
	{
		Load();
		RegisterEvents();
	}

	private void OnDestroy()
	{
		UnregisterEvents();
	}

	private void Start()
	{
		MakeSureMoneyAchievementsUnlocked();
	}

	private void RegisterEvents()
	{
		Quest.onQuestCompleted += OnQuestCompleted;
		SavesCounter.OnKillCountChanged = (Action<string, int>)Delegate.Combine(SavesCounter.OnKillCountChanged, new Action<string, int>(OnKillCountChanged));
		MultiSceneCore.OnSetSceneVisited += OnSetSceneVisited;
		LevelManager.OnEvacuated += OnEvacuated;
		EconomyManager.OnMoneyChanged += OnMoneyChanged;
		EndowmentManager.OnEndowmentUnlock = (Action<EndowmentIndex>)Delegate.Combine(EndowmentManager.OnEndowmentUnlock, new Action<EndowmentIndex>(OnEndowmentUnlocked));
		EconomyManager.OnEconomyManagerLoaded += OnEconomyManagerLoaded;
	}

	private void UnregisterEvents()
	{
		Quest.onQuestCompleted -= OnQuestCompleted;
		SavesCounter.OnKillCountChanged = (Action<string, int>)Delegate.Remove(SavesCounter.OnKillCountChanged, new Action<string, int>(OnKillCountChanged));
		MultiSceneCore.OnSetSceneVisited -= OnSetSceneVisited;
		LevelManager.OnEvacuated -= OnEvacuated;
		EconomyManager.OnMoneyChanged -= OnMoneyChanged;
		EndowmentManager.OnEndowmentUnlock = (Action<EndowmentIndex>)Delegate.Remove(EndowmentManager.OnEndowmentUnlock, new Action<EndowmentIndex>(OnEndowmentUnlocked));
		EconomyManager.OnEconomyManagerLoaded -= OnEconomyManagerLoaded;
	}

	private void OnEconomyManagerLoaded()
	{
		MakeSureMoneyAchievementsUnlocked();
	}

	private void OnEndowmentUnlocked(EndowmentIndex index)
	{
		Unlock($"Endowmment_{index}");
	}

	public static void UnlockEndowmentAchievement(EndowmentIndex index)
	{
		if (!(Instance == null))
		{
			Instance.Unlock($"Endowmment_{index}");
		}
	}

	private void OnMoneyChanged(long oldValue, long newValue)
	{
		if (oldValue < 10000 && newValue >= 10000)
		{
			Unlock("Money_10K");
		}
		if (oldValue < 100000 && newValue >= 100000)
		{
			Unlock("Money_100K");
		}
		if (oldValue < 1000000 && newValue >= 1000000)
		{
			Unlock("Money_1M");
		}
	}

	private void MakeSureMoneyAchievementsUnlocked()
	{
		long money = EconomyManager.Money;
		if (money >= 10000)
		{
			Unlock("Money_10K");
		}
		if (money >= 100000)
		{
			Unlock("Money_100K");
		}
		if (money >= 1000000)
		{
			Unlock("Money_1M");
		}
	}

	private void OnEvacuated(EvacuationInfo info)
	{
		string mainSceneID = MultiSceneCore.MainSceneID;
		if (evacuateSceneIDs.Contains(mainSceneID))
		{
			Unlock("Evacuate_" + mainSceneID);
		}
	}

	private void OnSetSceneVisited(string id)
	{
		if (achievementSceneIDs.Contains(id))
		{
			Unlock("Arrive_" + id);
		}
	}

	private void OnKillCountChanged(string key, int value)
	{
		Unlock("FirstBlood");
		if (AchievementDatabase.Instance == null)
		{
			return;
		}
		Debug.Log("COUNTING " + key);
		KillCountAchievement[] killCountAchivements = KillCountAchivements;
		for (int i = 0; i < killCountAchivements.Length; i++)
		{
			KillCountAchievement killCountAchievement = killCountAchivements[i];
			if (killCountAchievement.key == key && value >= killCountAchievement.value)
			{
				Unlock($"Kill_{key}_{killCountAchievement.value}");
			}
		}
	}

	private void OnQuestCompleted(Quest quest)
	{
		if (!(AchievementDatabase.Instance == null))
		{
			string id = $"Quest_{quest.ID}";
			if (AchievementDatabase.TryGetAchievementData(id, out var _))
			{
				Unlock(id);
			}
		}
	}

	private void Save()
	{
		SavesSystem.SaveGlobal("Achievements", UnlockedAchievements);
	}

	private void Load()
	{
		UnlockedAchievements.Clear();
		List<string> list = SavesSystem.LoadGlobal<List<string>>("Achievements");
		if (list != null)
		{
			UnlockedAchievements.AddRange(list);
		}
		AchievementManager.OnAchievementDataLoaded?.Invoke(this);
	}

	public void Unlock(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			Debug.LogError("Trying to unlock a empty acheivement.", this);
			return;
		}
		id = id.Trim();
		if (!AchievementDatabase.TryGetAchievementData(id, out var _))
		{
			Debug.LogError("Invalid acheivement id: " + id);
		}
		if (!UnlockedAchievements.Contains(id) && CanUnlockAchievement)
		{
			UnlockedAchievements.Add(id);
			Save();
			AchievementManager.OnAchievementUnlocked?.Invoke(id);
		}
	}

	public static bool IsIDValid(string id)
	{
		if (AchievementDatabase.Instance == null)
		{
			return false;
		}
		return AchievementDatabase.Instance.IsIDValid(id);
	}
}
