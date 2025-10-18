using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Duckov.Quests.Relations;
using Duckov.Scenes;
using Duckov.Utilities;
using Eflatun.SceneReference;
using ItemStatsSystem;
using Saves;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Duckov.Quests;

public class Quest : MonoBehaviour, ISaveDataProvider, INeedInspection
{
	[Serializable]
	public struct SaveData
	{
		public int id;

		public bool complete;

		public bool needInspection;

		public QuestGiverID questGiverID;

		public List<(int id, object data)> taskStatus;

		public List<(int id, object data)> rewardStatus;
	}

	public struct QuestInfo
	{
		public int questId;

		public QuestInfo(Quest quest)
		{
			questId = quest.id;
		}
	}

	[SerializeField]
	private int id;

	[LocalizationKey("Quests")]
	[SerializeField]
	private string displayName;

	[LocalizationKey("Quests")]
	[SerializeField]
	private string description;

	[SerializeField]
	private int requireLevel;

	[SerializeField]
	private bool lockInDemo;

	[FormerlySerializedAs("requiredItem")]
	[SerializeField]
	[ItemTypeID]
	private int requiredItemID;

	[SerializeField]
	private int requiredItemCount = 1;

	[SceneID]
	[SerializeField]
	private string requireSceneID;

	[SerializeField]
	private QuestGiverID questGiverID;

	[SerializeField]
	internal List<Condition> prerequisit = new List<Condition>();

	[SerializeField]
	internal List<Task> tasks = new List<Task>();

	[SerializeField]
	internal List<Reward> rewards = new List<Reward>();

	private ReadOnlyCollection<Reward> _readonly_rewards;

	[SerializeField]
	[HideInInspector]
	private int nextTaskID;

	[SerializeField]
	[HideInInspector]
	private int nextRewardID;

	private ReadOnlyCollection<Condition> prerequisits_ReadOnly;

	[SerializeField]
	private bool complete;

	[SerializeField]
	private bool needInspection;

	public UnityEvent OnCompletedUnityEvent;

	public SceneInfoEntry RequireSceneInfo => SceneInfoCollection.GetSceneInfo(requireSceneID);

	public SceneReference RequireScene => RequireSceneInfo?.SceneReference;

	public List<Task> Tasks => tasks;

	public ReadOnlyCollection<Reward> Rewards
	{
		get
		{
			if (_readonly_rewards == null)
			{
				_readonly_rewards = new ReadOnlyCollection<Reward>(rewards);
			}
			return _readonly_rewards;
		}
	}

	public ReadOnlyCollection<Condition> Prerequisits
	{
		get
		{
			if (prerequisits_ReadOnly == null)
			{
				prerequisits_ReadOnly = new ReadOnlyCollection<Condition>(prerequisit);
			}
			return prerequisits_ReadOnly;
		}
	}

	public bool SceneRequirementSatisfied
	{
		get
		{
			SceneReference requireScene = RequireScene;
			if (requireScene == null)
			{
				return true;
			}
			if (requireScene.UnsafeReason == SceneReferenceUnsafeReason.Empty)
			{
				return true;
			}
			if (requireScene.UnsafeReason == SceneReferenceUnsafeReason.None)
			{
				return requireScene.LoadedScene.isLoaded;
			}
			return true;
		}
	}

	public int RequireLevel => requireLevel;

	public bool LockInDemo => lockInDemo;

	public bool Complete
	{
		get
		{
			return complete;
		}
		internal set
		{
			complete = value;
			this.onStatusChanged?.Invoke(this);
			Quest.onQuestStatusChanged?.Invoke(this);
			if (complete)
			{
				this.onCompleted?.Invoke(this);
				OnCompletedUnityEvent?.Invoke();
				Quest.onQuestCompleted?.Invoke(this);
			}
		}
	}

	public bool NeedInspection
	{
		get
		{
			if (!Active && !QuestManager.EverInspected(ID))
			{
				return true;
			}
			if (!Active)
			{
				return false;
			}
			if (Active && AreTasksFinished())
			{
				return true;
			}
			if (AnyTaskNeedInspection())
			{
				return true;
			}
			return needInspection;
		}
		set
		{
			needInspection = value;
			this.onNeedInspectionChanged?.Invoke(this);
			Quest.onQuestNeedInspectionChanged?.Invoke(this);
		}
	}

	public int ID
	{
		get
		{
			return id;
		}
		internal set
		{
			id = value;
		}
	}

	public bool Active => QuestManager.IsQuestActive(this);

	public int RequiredItemID => requiredItemID;

	public int RequiredItemCount => requiredItemCount;

	public string DisplayName => displayName.ToPlainText();

	public string Description => description.ToPlainText();

	public string DisplayNameRaw
	{
		get
		{
			return displayName;
		}
		set
		{
			displayName = value;
		}
	}

	public string DescriptionRaw
	{
		get
		{
			return description;
		}
		set
		{
			description = value;
		}
	}

	public QuestGiverID QuestGiverID
	{
		get
		{
			return questGiverID;
		}
		internal set
		{
			questGiverID = value;
		}
	}

	public object FinishedTaskCount => tasks.Count((Task e) => e.IsFinished());

	public event Action<Quest> onNeedInspectionChanged;

	internal event Action<Quest> onStatusChanged;

	internal event Action<Quest> onActivated;

	internal event Action<Quest> onCompleted;

	public static event Action<Quest> onQuestStatusChanged;

	public static event Action<Quest> onQuestNeedInspectionChanged;

	public static event Action<Quest> onQuestActivated;

	public static event Action<Quest> onQuestCompleted;

	private bool AnyTaskNeedInspection()
	{
		if (tasks != null)
		{
			foreach (Task task in tasks)
			{
				if (!(task == null) && task.NeedInspection)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool MeetsPrerequisit()
	{
		if (RequireLevel > EXPManager.Level)
		{
			return false;
		}
		if (LockInDemo && GameMetaData.Instance.IsDemo)
		{
			return false;
		}
		QuestRelationGraph questRelation = GameplayDataSettings.QuestRelation;
		if (questRelation.GetNode(id) != null)
		{
			if (!QuestManager.AreQuestFinished(questRelation.GetRequiredIDs(id)))
			{
				return false;
			}
			foreach (Condition item in prerequisit)
			{
				if (!item.Evaluate())
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public bool AreTasksFinished()
	{
		foreach (Task task in tasks)
		{
			if (task == null)
			{
				Debug.LogError($"存在空的Task，QuestID：{id}");
			}
			else if (!task.IsFinished())
			{
				return false;
			}
		}
		return true;
	}

	public void Initialize()
	{
	}

	public void OnValidate()
	{
		displayName = $"Quest_{id}";
		description = $"Quest_{id}_Desc";
	}

	public object GenerateSaveData()
	{
		SaveData saveData = new SaveData
		{
			id = id,
			complete = complete,
			needInspection = needInspection,
			taskStatus = new List<(int, object)>(),
			rewardStatus = new List<(int, object)>()
		};
		foreach (Task task in tasks)
		{
			int iD = task.ID;
			object item = task.GenerateSaveData();
			if (!(task == null))
			{
				saveData.taskStatus.Add((iD, item));
			}
		}
		foreach (Reward reward in rewards)
		{
			if (reward == null)
			{
				Debug.LogError($"Null Reward detected in quest {id}");
				continue;
			}
			int iD2 = reward.ID;
			object item2 = reward.GenerateSaveData();
			saveData.rewardStatus.Add((iD2, item2));
		}
		return saveData;
	}

	public void SetupSaveData(object obj)
	{
		SaveData saveData = (SaveData)obj;
		if (saveData.id != id)
		{
			Debug.LogError("任务ID不匹配，加载失败");
			return;
		}
		complete = saveData.complete;
		needInspection = saveData.needInspection;
		foreach (var cur in saveData.taskStatus)
		{
			Task task = tasks.Find((Task e) => e.ID == cur.id);
			if (task == null)
			{
				Debug.LogWarning($"未找到Task {cur.id}");
			}
			else
			{
				task.SetupSaveData(cur.data);
			}
		}
		foreach (var cur2 in saveData.rewardStatus)
		{
			Reward reward = rewards.Find((Reward e) => e.ID == cur2.id);
			if (reward == null)
			{
				Debug.LogWarning($"未找到Reward {cur2.id}");
				continue;
			}
			reward.SetupSaveData(cur2.data);
			reward.NotifyReload(this);
		}
		InitTasks();
		if (!complete)
		{
			return;
		}
		foreach (Reward reward2 in rewards)
		{
			if (!(reward2 == null) && !reward2.Claimed && reward2.AutoClaim)
			{
				reward2.Claim();
			}
		}
	}

	internal void NotifyTaskFinished(Task task)
	{
		if (task.Master != this)
		{
			Debug.LogError("Task.Master 与 Quest不匹配");
			return;
		}
		Quest.onQuestStatusChanged?.Invoke(this);
		this.onStatusChanged?.Invoke(this);
		QuestManager.NotifyTaskFinished(this, task);
	}

	internal void NotifyRewardClaimed(Reward reward)
	{
		if (reward.Master != this)
		{
			Debug.LogError("Reward.Master 与Quest 不匹配");
		}
		if (AreRewardsClaimed())
		{
			needInspection = false;
		}
		Quest.onQuestStatusChanged?.Invoke(this);
		this.onStatusChanged?.Invoke(this);
		Quest.onQuestNeedInspectionChanged?.Invoke(this);
	}

	internal bool AreRewardsClaimed()
	{
		foreach (Reward reward in rewards)
		{
			if (!reward.Claimed)
			{
				return false;
			}
		}
		return true;
	}

	internal void NotifyActivated()
	{
		InitTasks();
		this.onStatusChanged?.Invoke(this);
		this.onActivated?.Invoke(this);
		Quest.onQuestActivated?.Invoke(this);
	}

	private void InitTasks()
	{
		foreach (Task task in tasks)
		{
			task.Init();
		}
	}

	public bool TryComplete()
	{
		if (Complete)
		{
			return false;
		}
		if (!AreTasksFinished())
		{
			return false;
		}
		Complete = true;
		return true;
	}

	internal QuestInfo GetInfo()
	{
		return new QuestInfo(this);
	}
}
