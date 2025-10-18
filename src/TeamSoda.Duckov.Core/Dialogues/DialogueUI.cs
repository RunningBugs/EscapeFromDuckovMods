using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.UI.Animations;
using Duckov.Utilities;
using NodeCanvas.DialogueTrees;
using SodaCraft.Localizations;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Dialogues;

public class DialogueUI : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private static DialogueUI instance;

	[SerializeField]
	private FadeGroup mainFadeGroup;

	[SerializeField]
	private FadeGroup textAreaFadeGroup;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private GameObject continueIndicator;

	[SerializeField]
	private float speed = 10f;

	[SerializeField]
	private RectTransform actorPositionIndicator;

	[SerializeField]
	private FadeGroup actorNameFadeGroup;

	[SerializeField]
	private TextMeshProUGUI actorNameText;

	[SerializeField]
	private GameObject actorPortraitContainer;

	[SerializeField]
	private Image actorPortraitDisplay;

	[SerializeField]
	private FadeGroup choiceListFadeGroup;

	[SerializeField]
	private Menu choiceMenu;

	[SerializeField]
	private DialogueUIChoice choiceTemplate;

	private PrefabPool<DialogueUIChoice> _choicePool;

	private DuckovDialogueActor talkingActor;

	private int confirmedChoice;

	private bool waitingForChoice;

	private bool confirmed;

	private PrefabPool<DialogueUIChoice> ChoicePool
	{
		get
		{
			if (_choicePool == null)
			{
				_choicePool = new PrefabPool<DialogueUIChoice>(choiceTemplate);
			}
			return _choicePool;
		}
	}

	public static bool Active
	{
		get
		{
			if (instance == null)
			{
				return false;
			}
			return instance.mainFadeGroup.IsShown;
		}
	}

	public static event Action OnDialogueStatusChanged;

	private void Awake()
	{
		instance = this;
		choiceTemplate.gameObject.SetActive(value: false);
		RegisterEvents();
	}

	private void OnDestroy()
	{
		UnregisterEvents();
	}

	private void Update()
	{
		RefreshActorPositionIndicator();
		if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
		{
			Confirm();
		}
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void RegisterEvents()
	{
		DialogueTree.OnDialogueStarted += OnDialogueStarted;
		DialogueTree.OnDialoguePaused += OnDialoguePaused;
		DialogueTree.OnDialogueFinished += OnDialogueFinished;
		DialogueTree.OnSubtitlesRequest += OnSubtitlesRequest;
		DialogueTree.OnMultipleChoiceRequest += OnMultipleChoiceRequest;
	}

	private void UnregisterEvents()
	{
		DialogueTree.OnDialogueStarted -= OnDialogueStarted;
		DialogueTree.OnDialoguePaused -= OnDialoguePaused;
		DialogueTree.OnDialogueFinished -= OnDialogueFinished;
		DialogueTree.OnSubtitlesRequest -= OnSubtitlesRequest;
		DialogueTree.OnMultipleChoiceRequest -= OnMultipleChoiceRequest;
	}

	private void OnMultipleChoiceRequest(MultipleChoiceRequestInfo info)
	{
		DoMultipleChoice(info).Forget();
	}

	private void OnSubtitlesRequest(SubtitlesRequestInfo info)
	{
		DoSubtitle(info).Forget();
	}

	public static void HideTextFadeGroup()
	{
		instance.MHideTextFadeGroup();
	}

	private void MHideTextFadeGroup()
	{
		textAreaFadeGroup.Hide();
	}

	private void OnDialogueFinished(DialogueTree tree)
	{
		textAreaFadeGroup.Hide();
		InputManager.ActiveInput(base.gameObject);
		mainFadeGroup.Hide();
		DialogueUI.OnDialogueStatusChanged?.Invoke();
	}

	private void OnDialoguePaused(DialogueTree tree)
	{
		DialogueUI.OnDialogueStatusChanged?.Invoke();
	}

	private void OnDialogueStarted(DialogueTree tree)
	{
		InputManager.DisableInput(base.gameObject);
		mainFadeGroup.Show();
		DialogueUI.OnDialogueStatusChanged?.Invoke();
		actorNameFadeGroup.SkipHide();
	}

	private async UniTask DoSubtitle(SubtitlesRequestInfo info)
	{
		SetupActorInfo(info.actor);
		continueIndicator.SetActive(value: false);
		string text = info.statement.text;
		TMP_TextInfo textInfo = this.text.GetTextInfo(text);
		this.text.text = text;
		this.text.maxVisibleCharacters = 0;
		await textAreaFadeGroup.ShowAndReturnTask();
		int totalCharacterCount = textInfo.characterCount;
		float buffer = 0f;
		confirmed = false;
		for (int c = 1; c <= totalCharacterCount; c++)
		{
			while (buffer < 1f && !confirmed)
			{
				buffer += Time.unscaledDeltaTime * speed;
				await UniTask.NextFrame();
			}
			buffer -= 1f;
			if (c == 1)
			{
				AudioManager.Post("UI/dialogue_start");
			}
			else
			{
				AudioManager.Post("UI/dialogue_bump");
			}
			this.text.maxVisibleCharacters = c;
		}
		this.text.maxVisibleCharacters = totalCharacterCount;
		await WaitForConfirm();
		await textAreaFadeGroup.HideAndReturnTask();
		this.text.text = string.Empty;
		talkingActor = null;
		info.Continue();
	}

	private void SetupActorInfo(IDialogueActor actor)
	{
		if (!(actor is DuckovDialogueActor duckovDialogueActor))
		{
			actorNameFadeGroup.Hide();
			actorPortraitContainer.gameObject.SetActive(value: false);
			actorPositionIndicator.gameObject.SetActive(value: false);
			talkingActor = null;
			return;
		}
		talkingActor = duckovDialogueActor;
		Sprite portraitSprite = duckovDialogueActor.portraitSprite;
		string nameKey = duckovDialogueActor.NameKey;
		_ = duckovDialogueActor.transform;
		actorNameText.text = nameKey.ToPlainText();
		actorNameFadeGroup.Show();
		actorPortraitContainer.SetActive(portraitSprite);
		actorPortraitDisplay.sprite = portraitSprite;
		if (talkingActor.transform != null)
		{
			actorPositionIndicator.gameObject.SetActive(value: true);
		}
		RefreshActorPositionIndicator();
	}

	private void RefreshActorPositionIndicator()
	{
		if (talkingActor == null)
		{
			actorPositionIndicator.gameObject.SetActive(value: false);
		}
		else
		{
			actorPositionIndicator.MatchWorldPosition(talkingActor.transform.position + talkingActor.Offset);
		}
	}

	private async UniTask DoMultipleChoice(MultipleChoiceRequestInfo info)
	{
		await DisplayOptions(info.options);
		int choice = await WaitForChoice();
		await choiceListFadeGroup.HideAndReturnTask();
		info.SelectOption(choice);
	}

	private async UniTask DisplayOptions(Dictionary<IStatement, int> options)
	{
		ChoicePool.ReleaseAll();
		foreach (KeyValuePair<IStatement, int> option in options)
		{
			DialogueUIChoice dialogueUIChoice = ChoicePool.Get();
			dialogueUIChoice.Setup(this, option);
			dialogueUIChoice.transform.SetAsLastSibling();
		}
		choiceMenu.SelectDefault();
		await choiceListFadeGroup.ShowAndReturnTask();
		choiceMenu.Focused = true;
	}

	internal void NotifyChoiceConfirmed(DialogueUIChoice choice)
	{
		confirmedChoice = choice.Index;
	}

	private async UniTask<int> WaitForChoice()
	{
		confirmedChoice = -1;
		waitingForChoice = true;
		while (confirmedChoice < 0)
		{
			await UniTask.NextFrame();
		}
		waitingForChoice = false;
		return confirmedChoice;
	}

	public void Confirm()
	{
		confirmed = true;
	}

	private async UniTask WaitForConfirm()
	{
		continueIndicator.SetActive(value: true);
		confirmed = false;
		while (!confirmed)
		{
			await UniTask.NextFrame();
		}
		continueIndicator.SetActive(value: false);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Confirm();
	}
}
