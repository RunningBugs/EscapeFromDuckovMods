using System.Collections.Generic;
using Duckov.Quests;
using NodeCanvas.DialogueTrees;
using NodeCanvas.StateMachines;
using Saves;
using Unity.VisualScripting;
using UnityEngine;

public class CutScene : MonoBehaviour
{
	public enum PlayTiming
	{
		Start = 0,
		OnTriggerEnter = 2,
		Manual = 3
	}

	[SerializeField]
	private string id;

	[SerializeField]
	private bool playOnce = true;

	[SerializeField]
	private bool setActiveFalseWhenFinished = true;

	[SerializeField]
	private bool setupActorReferencesUsingIDs;

	[SerializeField]
	private Collider trigger;

	[SerializeField]
	private List<Condition> prerequisites = new List<Condition>();

	[SerializeField]
	private FSMOwner fsmOwner;

	[SerializeField]
	private DialogueTreeController dialogueTreeOwner;

	[SerializeField]
	private PlayTiming playTiming;

	private bool playing;

	private string SaveKey => "CutScene_" + id;

	private bool UseTrigger => playTiming == PlayTiming.OnTriggerEnter;

	private bool HideFSMOwnerField
	{
		get
		{
			if (!fsmOwner)
			{
				return dialogueTreeOwner;
			}
			return false;
		}
	}

	private bool HideDialogueTreeOwnerField
	{
		get
		{
			if ((bool)fsmOwner)
			{
				return !dialogueTreeOwner;
			}
			return false;
		}
	}

	private bool Played => SavesSystem.Load<bool>(SaveKey);

	public void MarkPlayed()
	{
		if (!string.IsNullOrWhiteSpace(id))
		{
			SavesSystem.Save(SaveKey, value: true);
		}
	}

	private void OnEnable()
	{
	}

	private void Awake()
	{
		if (UseTrigger)
		{
			InitializeTrigger();
		}
	}

	private void InitializeTrigger()
	{
		if (trigger == null)
		{
			Debug.LogError("CutScene想要使用Trigger触发，但没有配置Trigger引用。", this);
		}
		OnTriggerEnterEvent onTriggerEnterEvent = trigger.AddComponent<OnTriggerEnterEvent>();
		onTriggerEnterEvent.onlyMainCharacter = true;
		onTriggerEnterEvent.triggerOnce = true;
		onTriggerEnterEvent.DoOnTriggerEnter.AddListener(PlayIfNessisary);
	}

	private void Start()
	{
		if (playTiming == PlayTiming.Start)
		{
			PlayIfNessisary();
		}
	}

	private void Update()
	{
		if (!playing)
		{
			return;
		}
		if ((bool)fsmOwner)
		{
			if (!fsmOwner.isRunning)
			{
				playing = false;
				OnPlayFinished();
			}
		}
		else if ((bool)dialogueTreeOwner && !dialogueTreeOwner.isRunning)
		{
			playing = false;
			OnPlayFinished();
		}
	}

	private void OnPlayFinished()
	{
		MarkPlayed();
		if (setActiveFalseWhenFinished)
		{
			base.gameObject.SetActive(value: false);
		}
		if (playOnce && string.IsNullOrWhiteSpace(id))
		{
			Debug.LogError("CutScene没有填写ID，无法记录", base.gameObject);
		}
	}

	public void PlayIfNessisary()
	{
		if (playOnce && Played)
		{
			base.gameObject.SetActive(value: false);
		}
		else if (prerequisites.Satisfied())
		{
			Play();
		}
	}

	public void Play()
	{
		if ((bool)fsmOwner)
		{
			fsmOwner.StartBehaviour();
			playing = true;
		}
		else if ((bool)dialogueTreeOwner)
		{
			if (setupActorReferencesUsingIDs)
			{
				SetupActors();
			}
			dialogueTreeOwner.StartBehaviour();
			playing = true;
		}
	}

	private void SetupActors()
	{
		if (dialogueTreeOwner == null)
		{
			return;
		}
		if (dialogueTreeOwner.behaviour == null)
		{
			Debug.LogError("Dialoguetree没有配置", dialogueTreeOwner);
			return;
		}
		foreach (DialogueTree.ActorParameter actorParameter in dialogueTreeOwner.behaviour.actorParameters)
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
					dialogueTreeOwner.SetActorReference(text, duckovDialogueActor);
				}
			}
		}
	}
}
