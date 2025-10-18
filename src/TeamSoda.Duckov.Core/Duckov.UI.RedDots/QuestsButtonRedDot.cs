using Duckov.Quests;
using UnityEngine;

namespace Duckov.UI.RedDots;

public class QuestsButtonRedDot : MonoBehaviour
{
	public GameObject dot;

	private void Awake()
	{
		Quest.onQuestNeedInspectionChanged += OnQuestNeedInspectionChanged;
	}

	private void OnDestroy()
	{
		Quest.onQuestNeedInspectionChanged -= OnQuestNeedInspectionChanged;
	}

	private void OnQuestNeedInspectionChanged(Quest quest)
	{
		Refresh();
	}

	private void Start()
	{
		Refresh();
	}

	private void Refresh()
	{
		dot.SetActive(QuestManager.AnyQuestNeedsInspection);
	}
}
