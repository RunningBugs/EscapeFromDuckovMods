using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.Quests.UI;

public class TaskEntry : MonoBehaviour, IPoolable, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private Image statusIcon;

	[SerializeField]
	private Image taskIcon;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private Button interactionButton;

	[SerializeField]
	private GameObject targetNotInteractablePlaceHolder;

	[SerializeField]
	private TextMeshProUGUI interactionText;

	[SerializeField]
	private TextMeshProUGUI interactionPlaceHolderText;

	[SerializeField]
	private Sprite unsatisfiedIcon;

	[SerializeField]
	private Sprite satisfiedIcon;

	[SerializeField]
	private bool interactable;

	private Task target;

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

	private void Awake()
	{
		interactionButton.onClick.AddListener(OnInteractionButtonClicked);
	}

	private void OnInteractionButtonClicked()
	{
		if (!(target == null))
		{
			target.Interact();
		}
	}

	public void NotifyPooled()
	{
	}

	public void NotifyReleased()
	{
		UnregisterEvents();
		target = null;
	}

	internal void Setup(Task target)
	{
		UnregisterEvents();
		this.target = target;
		RegisterEvents();
		Refresh();
	}

	private void Refresh()
	{
		if (!(target == null))
		{
			description.text = target.Description;
			string[] extraDescriptsions = target.ExtraDescriptsions;
			foreach (string text in extraDescriptsions)
			{
				TextMeshProUGUI textMeshProUGUI = description;
				textMeshProUGUI.text = textMeshProUGUI.text + "  \n- " + text;
			}
			Sprite icon = target.Icon;
			if ((bool)icon)
			{
				taskIcon.sprite = icon;
				taskIcon.gameObject.SetActive(value: true);
			}
			else
			{
				taskIcon.gameObject.SetActive(value: false);
			}
			bool flag = target.IsFinished();
			statusIcon.sprite = (flag ? satisfiedIcon : unsatisfiedIcon);
			if (Interactable && !flag && target.Interactable)
			{
				bool possibleValidInteraction = target.PossibleValidInteraction;
				interactionText.text = target.InteractText;
				interactionPlaceHolderText.text = target.InteractText;
				interactionButton.gameObject.SetActive(possibleValidInteraction);
				targetNotInteractablePlaceHolder.gameObject.SetActive(!possibleValidInteraction);
			}
			else
			{
				interactionButton.gameObject.SetActive(value: false);
				targetNotInteractablePlaceHolder.gameObject.SetActive(value: false);
			}
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

	private void OnTargetStatusChanged(Task task)
	{
		if (task != target)
		{
			Debug.LogError("目标不匹配。");
		}
		else
		{
			Refresh();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!eventData.used && CheatMode.Active && UIInputManager.Ctrl && UIInputManager.Alt && UIInputManager.Shift)
		{
			target.ForceFinish();
			eventData.Use();
		}
	}
}
