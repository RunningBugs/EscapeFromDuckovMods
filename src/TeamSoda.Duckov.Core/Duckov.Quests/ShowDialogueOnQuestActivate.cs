using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.UI.DialogueBubbles;
using UnityEngine;

namespace Duckov.Quests;

public class ShowDialogueOnQuestActivate : MonoBehaviour
{
	[Serializable]
	public class DialogueEntry
	{
		[TextArea]
		public string content;
	}

	[SerializeField]
	private Quest quest;

	[SerializeField]
	private List<DialogueEntry> dialogueEntries;

	private Transform cachedQuestGiverTransform;

	private void Awake()
	{
		if (quest == null)
		{
			quest = GetComponent<Quest>();
		}
		quest.onActivated += OnQuestActivated;
	}

	private void OnQuestActivated(Quest quest)
	{
		ShowDIalogue().Forget();
	}

	private async UniTask ShowDIalogue()
	{
		cachedQuestGiverTransform = null;
		await GameplayUIManager.TemporaryHide();
		cachedQuestGiverTransform = GetQuestGiverTransform(quest);
		if (cachedQuestGiverTransform == null)
		{
			Debug.LogError("没找到QuestGiver " + quest.QuestGiverID.ToString() + " 的transform");
		}
		else
		{
			foreach (DialogueEntry dialogueEntry in dialogueEntries)
			{
				await ShowDialogueEntry(dialogueEntry);
			}
		}
		await GameplayUIManager.ReverseTemporaryHide();
	}

	private async UniTask ShowDialogueEntry(DialogueEntry cur)
	{
		await DialogueBubblesManager.Show(cur.content, cachedQuestGiverTransform, -1f, needInteraction: true, skippable: true);
	}

	private Transform GetQuestGiverTransform(Quest quest)
	{
		QuestGiverID id = quest.QuestGiverID;
		QuestGiver questGiver = UnityEngine.Object.FindObjectsByType<QuestGiver>(FindObjectsSortMode.None).FirstOrDefault((QuestGiver e) => e != null && e.ID == id);
		if (questGiver == null)
		{
			return null;
		}
		return questGiver.transform;
	}
}
