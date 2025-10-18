using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Quests.Relations;
using Duckov.Quests.Tasks;
using Duckov.UI;
using Duckov.Utilities;
using Saves;
using Sirenix.Utilities;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.Quests;

public class QuestManager : MonoBehaviour, ISaveDataProvider, INeedInspection
{
	[Serializable]
	public struct SaveData
	{
		public List<object> activeQuestsData;

		public List<object> historyQuestsData;

		public List<int> everInspectedQuest;
	}

	[SerializeField]
	private string taskFinishNotificationFormatKey = "UI_Quest_TaskFinishedNotification";

	private static QuestManager instance;

	public static Action<Quest, Task> OnTaskFinishedEvent;

	private List<Quest> activeQuests = new List<Quest>();

	private List<Quest> historyQuests = new List<Quest>();

	private List<int> everInspectedQuest = new List<int>();

	private const string savePrefix = "Quest";

	private const string saveKey = "Data";

	public string TaskFinishNotificationFormat => taskFinishNotificationFormatKey.ToPlainText();

	public static QuestManager Instance => instance;

	public static bool AnyQuestNeedsInspection
	{
		get
		{
			if (Instance == null)
			{
				return false;
			}
			return Instance.NeedInspection;
		}
	}

	public bool NeedInspection
	{
		get
		{
			if (activeQuests == null)
			{
				return false;
			}
			return activeQuests.Any((Quest e) => e != null && e.NeedInspection);
		}
	}

	private ICollection<Quest> QuestPrefabCollection => GameplayDataSettings.QuestCollection;

	private QuestRelationGraph QuestRelation => GameplayDataSettings.QuestRelation;

	public List<Quest> ActiveQuests
	{
		get
		{
			activeQuests.Sort(delegate(Quest a, Quest b)
			{
				int num = (a.AreTasksFinished() ? 1 : 0);
				return (b.AreTasksFinished() ? 1 : 0) - num;
			});
			return activeQuests;
		}
	}

	public List<Quest> HistoryQuests => historyQuests;

	public List<int> EverInspectedQuest => everInspectedQuest;

	public static event Action<QuestManager> onQuestListsChanged;

	public static IEnumerable<int> GetAllRequiredItems()
	{
		if (Instance == null)
		{
			yield break;
		}
		List<Quest> list = Instance.ActiveQuests;
		foreach (Quest item in list)
		{
			if (item.tasks == null)
			{
				continue;
			}
			foreach (Task task in item.tasks)
			{
				if (task is SubmitItems submitItems && !submitItems.IsFinished())
				{
					yield return submitItems.ItemTypeID;
				}
			}
		}
	}

	public static bool AnyActiveQuestNeedsInspection(QuestGiverID giverID)
	{
		if (Instance == null)
		{
			return false;
		}
		if (Instance.activeQuests == null)
		{
			return false;
		}
		return Instance.activeQuests.Any((Quest e) => e != null && e.QuestGiverID == giverID && e.NeedInspection);
	}

	public object GenerateSaveData()
	{
		SaveData saveData = new SaveData
		{
			activeQuestsData = new List<object>(),
			historyQuestsData = new List<object>(),
			everInspectedQuest = new List<int>()
		};
		foreach (Quest activeQuest in ActiveQuests)
		{
			saveData.activeQuestsData.Add(activeQuest.GenerateSaveData());
		}
		foreach (Quest historyQuest in HistoryQuests)
		{
			saveData.historyQuestsData.Add(historyQuest.GenerateSaveData());
		}
		saveData.everInspectedQuest.AddRange(EverInspectedQuest);
		return saveData;
	}

	public void SetupSaveData(object dataObj)
	{
		if (!(dataObj is SaveData saveData))
		{
			Debug.LogError("错误的数据类型");
			return;
		}
		if (saveData.activeQuestsData != null)
		{
			foreach (object activeQuestsDatum in saveData.activeQuestsData)
			{
				int id = ((Quest.SaveData)activeQuestsDatum).id;
				Quest questPrefab = GetQuestPrefab(id);
				if (questPrefab == null)
				{
					Debug.LogError($"未找到Quest {id}");
					continue;
				}
				Quest quest = UnityEngine.Object.Instantiate(questPrefab, base.transform);
				quest.SetupSaveData(activeQuestsDatum);
				activeQuests.Add(quest);
			}
		}
		if (saveData.historyQuestsData != null)
		{
			foreach (object historyQuestsDatum in saveData.historyQuestsData)
			{
				int id2 = ((Quest.SaveData)historyQuestsDatum).id;
				Quest questPrefab2 = GetQuestPrefab(id2);
				if (questPrefab2 == null)
				{
					Debug.LogError($"未找到Quest {id2}");
					continue;
				}
				Quest quest2 = UnityEngine.Object.Instantiate(questPrefab2, base.transform);
				quest2.SetupSaveData(historyQuestsDatum);
				historyQuests.Add(quest2);
			}
		}
		if (saveData.everInspectedQuest != null)
		{
			EverInspectedQuest.Clear();
			EverInspectedQuest.AddRange(saveData.everInspectedQuest);
		}
	}

	private void Save()
	{
		SavesSystem.Save("Quest", "Data", GenerateSaveData());
	}

	private void Load()
	{
		try
		{
			SaveData saveData = SavesSystem.Load<SaveData>("Quest", "Data");
			SetupSaveData(saveData);
		}
		catch
		{
			Debug.LogError("在加载Quest存档时出现了错误");
		}
	}

	public IEnumerable<Quest> GetAllQuestsByQuestGiverID(QuestGiverID questGiverID)
	{
		return QuestPrefabCollection.Where((Quest e) => e != null && e.QuestGiverID == questGiverID);
	}

	private Quest GetQuestPrefab(int id)
	{
		return QuestPrefabCollection.FirstOrDefault((Quest q) => q != null && q.ID == id);
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		if (instance != this)
		{
			Debug.LogError("侦测到多个QuestManager！");
			return;
		}
		RegisterEvents();
		Load();
	}

	private void OnDestroy()
	{
		UnregisterEvents();
	}

	private void RegisterEvents()
	{
		Quest.onQuestStatusChanged += OnQuestStatusChanged;
		Quest.onQuestCompleted += OnQuestCompleted;
		SavesSystem.OnCollectSaveData += Save;
		SavesSystem.OnSetFile += Load;
	}

	private void UnregisterEvents()
	{
		Quest.onQuestStatusChanged -= OnQuestStatusChanged;
		Quest.onQuestCompleted -= OnQuestCompleted;
		SavesSystem.OnCollectSaveData -= Save;
		SavesSystem.OnSetFile -= Load;
	}

	private void OnQuestCompleted(Quest quest)
	{
		if (!activeQuests.Remove(quest))
		{
			Debug.LogWarning(quest.DisplayName + " 并不存在于活跃任务表中。已终止操作。");
			return;
		}
		historyQuests.Add(quest);
		QuestManager.onQuestListsChanged?.Invoke(this);
	}

	private void OnQuestStatusChanged(Quest quest)
	{
	}

	public void ActivateQuest(int id, QuestGiverID? overrideQuestGiverID = null)
	{
		Quest quest = UnityEngine.Object.Instantiate(GetQuestPrefab(id), base.transform);
		if (overrideQuestGiverID.HasValue)
		{
			quest.QuestGiverID = overrideQuestGiverID.Value;
		}
		activeQuests.Add(quest);
		quest.NotifyActivated();
		QuestManager.onQuestListsChanged?.Invoke(this);
	}

	internal static bool IsQuestAvaliable(int id)
	{
		if (Instance == null)
		{
			return false;
		}
		if (IsQuestFinished(id))
		{
			return false;
		}
		if (instance.activeQuests.Any((Quest e) => e.ID == id))
		{
			return false;
		}
		return Instance.GetQuestPrefab(id).MeetsPrerequisit();
	}

	internal static bool IsQuestFinished(int id)
	{
		if (instance == null)
		{
			return false;
		}
		return instance.historyQuests.Any((Quest e) => e.ID == id);
	}

	internal static bool AreQuestFinished(IEnumerable<int> requiredQuestIDs)
	{
		if (instance == null)
		{
			return false;
		}
		HashSet<int> hashSet = new HashSet<int>();
		hashSet.AddRange(requiredQuestIDs);
		foreach (Quest historyQuest in instance.historyQuests)
		{
			hashSet.Remove(historyQuest.ID);
		}
		return hashSet.Count <= 0;
	}

	internal static List<Quest> GetActiveQuestsFromGiver(QuestGiverID giverID)
	{
		List<Quest> result = new List<Quest>();
		if (instance == null)
		{
			return result;
		}
		return instance.ActiveQuests.Where((Quest e) => e.QuestGiverID == giverID).ToList();
	}

	internal static List<Quest> GetHistoryQuestsFromGiver(QuestGiverID giverID)
	{
		List<Quest> result = new List<Quest>();
		if (instance == null)
		{
			return result;
		}
		return instance.historyQuests.Where((Quest e) => e != null && e.QuestGiverID == giverID).ToList();
	}

	internal static bool IsQuestActive(Quest quest)
	{
		if (instance == null)
		{
			return false;
		}
		return instance.activeQuests.Contains(quest);
	}

	internal static bool IsQuestActive(int questID)
	{
		if (instance == null)
		{
			return false;
		}
		if (!instance.activeQuests.Any((Quest e) => e.ID == questID))
		{
			return false;
		}
		return true;
	}

	internal static bool AreQuestsActive(IEnumerable<int> requiredQuestIDs)
	{
		if (instance == null)
		{
			return false;
		}
		foreach (int id in requiredQuestIDs)
		{
			if (!instance.activeQuests.Any((Quest e) => e.ID == id))
			{
				return false;
			}
		}
		return true;
	}

	private void OnTaskFinished(Quest quest, Task task)
	{
		NotificationText.Push(TaskFinishNotificationFormat.Format(new
		{
			questDisplayName = quest.DisplayName,
			finishedTasks = quest.FinishedTaskCount,
			totalTasks = quest.tasks.Count
		}));
		OnTaskFinishedEvent?.Invoke(quest, task);
		AudioManager.Post("UI/mission_small");
	}

	internal static void NotifyTaskFinished(Quest quest, Task task)
	{
		instance?.OnTaskFinished(quest, task);
	}

	internal static bool EverInspected(int id)
	{
		if (Instance == null)
		{
			return false;
		}
		return Instance.EverInspectedQuest.Contains(id);
	}

	internal static void SetEverInspected(int id)
	{
		if (!EverInspected(id) && !(Instance == null))
		{
			Instance.EverInspectedQuest.Add(id);
		}
	}
}
