using Duckov.UI;
using NodeCanvas.DialogueTrees;
using UnityEngine;

namespace Duckov.Quests;

public class PlayDialogueGraphOnQuestActive : MonoBehaviour
{
	[SerializeField]
	private Quest quest;

	[SerializeField]
	private DialogueTreeController dialogueTreeController;

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
		if (View.ActiveView != null)
		{
			View.ActiveView.Close();
		}
		SetupActors();
		PlayDialogue();
	}

	private void PlayDialogue()
	{
		dialogueTreeController.StartDialogue();
	}

	private void SetupActors()
	{
		if (dialogueTreeController.behaviour == null)
		{
			Debug.LogError("Dialoguetree没有配置", dialogueTreeController);
			return;
		}
		foreach (DialogueTree.ActorParameter actorParameter in dialogueTreeController.behaviour.actorParameters)
		{
			string text = actorParameter.name;
			if (!string.IsNullOrEmpty(text))
			{
				DuckovDialogueActor duckovDialogueActor = DuckovDialogueActor.Get(text);
				if (duckovDialogueActor == null)
				{
					Debug.LogError("未找到actor ID:" + text);
				}
				else
				{
					dialogueTreeController.SetActorReference(text, duckovDialogueActor);
				}
			}
		}
	}
}
