using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using Duckov.Utilities;
using TMPro;
using UnityEngine;

namespace Duckov.Quests.UI;

public class QuestViewDetails : MonoBehaviour
{
	private Quest target;

	[SerializeField]
	private TaskEntry taskEntryPrefab;

	[SerializeField]
	private RewardEntry rewardEntry;

	[SerializeField]
	private GameObject placeHolder;

	[SerializeField]
	private FadeGroup contentFadeGroup;

	[SerializeField]
	private TextMeshProUGUI displayName;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private TextMeshProUGUI questGiverDisplayName;

	[SerializeField]
	private Transform tasksParent;

	[SerializeField]
	private Transform rewardsParent;

	[SerializeField]
	private QuestRequiredItem requiredItem;

	[SerializeField]
	private bool interactable;

	private PrefabPool<TaskEntry> _taskEntryPool;

	private PrefabPool<RewardEntry> _rewardEntryPool;

	private Quest showingQuest;

	private int activeTaskToken;

	public Quest Target => target;

	public bool Interactable
	{
		get
		{
			return interactable;
		}
		internal set
		{
			interactable = value;
		}
	}

	private PrefabPool<TaskEntry> TaskEntryPool
	{
		get
		{
			if (_taskEntryPool == null)
			{
				_taskEntryPool = new PrefabPool<TaskEntry>(taskEntryPrefab, tasksParent);
			}
			return _taskEntryPool;
		}
	}

	private PrefabPool<RewardEntry> RewardEntryPool
	{
		get
		{
			if (_rewardEntryPool == null)
			{
				_rewardEntryPool = new PrefabPool<RewardEntry>(rewardEntry, rewardsParent);
			}
			return _rewardEntryPool;
		}
	}

	private void Awake()
	{
		rewardEntry.gameObject.SetActive(value: false);
		taskEntryPrefab.gameObject.SetActive(value: false);
	}

	internal void Refresh()
	{
		RefreshAsync().Forget();
	}

	private int GetNewToken()
	{
		int num;
		for (num = activeTaskToken; num == activeTaskToken; num = Random.Range(1, int.MaxValue))
		{
		}
		activeTaskToken = num;
		return activeTaskToken;
	}

	private async UniTask RefreshAsync()
	{
		int token = GetNewToken();
		UnregisterEvents();
		if (showingQuest != target)
		{
			if (target == null)
			{
				placeHolder.SetActive(value: true);
			}
			await contentFadeGroup.HideAndReturnTask();
			if (token != activeTaskToken)
			{
				return;
			}
		}
		showingQuest = target;
		if (target == null)
		{
			placeHolder.SetActive(value: true);
			contentFadeGroup.SkipHide();
			return;
		}
		placeHolder.SetActive(value: false);
		target.NeedInspection = false;
		displayName.text = target.DisplayName;
		questGiverDisplayName.text = GameplayDataSettings.Quests.GetDisplayName(target.QuestGiverID);
		description.text = target.Description;
		requiredItem.Set(target.RequiredItemID, target.RequiredItemCount);
		SetupTasks();
		SetupRewards();
		RegisterEvents();
		await contentFadeGroup.ShowAndReturnTask();
	}

	private void SetupTasks()
	{
		TaskEntryPool.ReleaseAll();
		if (target == null)
		{
			return;
		}
		foreach (Task task in target.tasks)
		{
			TaskEntry taskEntry = TaskEntryPool.Get(tasksParent);
			taskEntry.Interactable = Interactable;
			taskEntry.Setup(task);
			taskEntry.transform.SetAsLastSibling();
		}
	}

	private void SetupRewards()
	{
		RewardEntryPool.ReleaseAll();
		if (target == null)
		{
			return;
		}
		foreach (Reward reward in target.rewards)
		{
			if (reward == null)
			{
				Debug.LogError($"任务 {target.ID} - {target.DisplayName} 中包含值为 null 的奖励。");
				continue;
			}
			RewardEntry obj = RewardEntryPool.Get(rewardsParent);
			obj.Interactable = Interactable;
			obj.Setup(reward);
			obj.transform.SetAsLastSibling();
		}
	}

	private void RegisterEvents()
	{
		if (!(target == null))
		{
			target.onStatusChanged += OnTargetStatusChanged;
		}
	}

	private void UnregisterEvents()
	{
		if (!(target == null))
		{
			target.onStatusChanged -= OnTargetStatusChanged;
		}
	}

	private void OnTargetStatusChanged(Quest quest)
	{
		Refresh();
	}

	internal void Setup(Quest quest)
	{
		target = quest;
		Refresh();
	}

	private void OnDestroy()
	{
		UnregisterEvents();
	}
}
