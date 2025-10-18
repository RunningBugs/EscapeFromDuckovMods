using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.Quests.UI;

public class QuestCompletePanel : MonoBehaviour
{
	[SerializeField]
	private FadeGroup mainFadeGroup;

	[SerializeField]
	private TextMeshProUGUI questNameText;

	[SerializeField]
	private RewardEntry rewardEntryTemplate;

	[SerializeField]
	private Button skipButton;

	[SerializeField]
	private Button takeAllButton;

	private PrefabPool<RewardEntry> _rewardEntryPool;

	private Quest target;

	private bool skipClicked;

	private PrefabPool<RewardEntry> RewardEntryPool
	{
		get
		{
			if (_rewardEntryPool == null)
			{
				_rewardEntryPool = new PrefabPool<RewardEntry>(rewardEntryTemplate, rewardEntryTemplate.transform.parent);
				rewardEntryTemplate.gameObject.SetActive(value: false);
			}
			return _rewardEntryPool;
		}
	}

	public Quest Target => target;

	private void Awake()
	{
		skipButton.onClick.AddListener(Skip);
		takeAllButton.onClick.AddListener(TakeAll);
	}

	private void TakeAll()
	{
		if (target == null)
		{
			return;
		}
		foreach (Reward reward in target.rewards)
		{
			if (!reward.Claimed)
			{
				reward.Claim();
			}
		}
	}

	public void Skip()
	{
		skipClicked = true;
	}

	public async UniTask Show(Quest quest)
	{
		target = quest;
		SetupContent(quest);
		await mainFadeGroup.ShowAndReturnTask();
		await WaitForEndOfInteraction();
		if (target == null)
		{
			return;
		}
		foreach (Reward reward in target.rewards)
		{
			if (!reward.Claimed && reward.AutoClaim)
			{
				reward.Claim();
			}
		}
		await mainFadeGroup.HideAndReturnTask();
	}

	private async UniTask WaitForEndOfInteraction()
	{
		skipClicked = false;
		while (!(target == null) && !target.AreRewardsClaimed() && !skipClicked)
		{
			await UniTask.NextFrame();
		}
	}

	private void SetupContent(Quest quest)
	{
		target = quest;
		if (quest == null)
		{
			return;
		}
		questNameText.text = quest.DisplayName;
		RewardEntryPool.ReleaseAll();
		foreach (Reward reward in quest.rewards)
		{
			RewardEntry rewardEntry = RewardEntryPool.Get(rewardEntryTemplate.transform.parent);
			rewardEntry.Setup(reward);
			rewardEntry.transform.SetAsLastSibling();
		}
	}
}
