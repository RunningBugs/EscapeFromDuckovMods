using Duckov.Endowment;
using Duckov.Quests;
using UnityEngine;

public class UnlockEndowmentWhenQuestComplete : MonoBehaviour
{
	[SerializeField]
	private Quest quest;

	[SerializeField]
	private EndowmentIndex endowmentToUnlock;

	private void Awake()
	{
		if (quest == null)
		{
			quest = GetComponent<Quest>();
		}
		if (quest != null)
		{
			quest.onCompleted += OnQuestCompleted;
		}
	}

	private void Start()
	{
		if (quest.Complete && !EndowmentManager.GetEndowmentUnlocked(endowmentToUnlock))
		{
			EndowmentManager.UnlockEndowment(endowmentToUnlock);
		}
	}

	private void OnDestroy()
	{
		if (quest != null)
		{
			quest.onCompleted -= OnQuestCompleted;
		}
	}

	private void OnQuestCompleted(Quest quest)
	{
		if (!EndowmentManager.GetEndowmentUnlocked(endowmentToUnlock))
		{
			EndowmentManager.UnlockEndowment(endowmentToUnlock);
		}
	}
}
