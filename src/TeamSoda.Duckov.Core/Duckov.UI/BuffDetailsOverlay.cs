using Duckov.Buffs;
using Duckov.UI.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.UI;

public class BuffDetailsOverlay : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private TextMeshProUGUI text_BuffName;

	[SerializeField]
	private TextMeshProUGUI text_BuffDescription;

	[SerializeField]
	private TextMeshProUGUI text_CountDown;

	[SerializeField]
	private PunchReceiver punchReceiver;

	[SerializeField]
	private float disappearAfterSeconds = 5f;

	private RectTransform rectTransform;

	private Buff target;

	private float timeWhenShowStarted;

	public Buff Target => target;

	private float TimeSinceShowStarted => Time.unscaledTime - timeWhenShowStarted;

	private void Awake()
	{
		rectTransform = base.transform as RectTransform;
		BuffsDisplayEntry.OnBuffsDisplayEntryClicked += OnBuffsDisplayEntryClicked;
	}

	private void OnDestroy()
	{
		BuffsDisplayEntry.OnBuffsDisplayEntryClicked -= OnBuffsDisplayEntryClicked;
	}

	private void OnBuffsDisplayEntryClicked(BuffsDisplayEntry entry, PointerEventData eventData)
	{
		if (fadeGroup.IsShown && target == entry.Target)
		{
			fadeGroup.Hide();
			punchReceiver.Punch();
		}
		else
		{
			Setup(entry);
			Show();
			punchReceiver.Punch();
		}
	}

	public void Setup(Buff target)
	{
		this.target = target;
		if (!(target == null))
		{
			text_BuffName.text = target.DisplayName;
			text_BuffDescription.text = target.Description;
			RefreshCountDown();
		}
	}

	private void Update()
	{
		if (fadeGroup.IsShown || fadeGroup.IsShowingInProgress)
		{
			if (target != null)
			{
				RefreshCountDown();
			}
			else
			{
				fadeGroup.Hide();
			}
			if (TimeSinceShowStarted > disappearAfterSeconds)
			{
				fadeGroup.Hide();
			}
		}
	}

	public void Setup(BuffsDisplayEntry target)
	{
		if (!(target == null))
		{
			Setup(target?.Target);
			RectTransform obj = target.Icon.rectTransform;
			Vector3 position = obj.TransformPoint(obj.rect.max);
			rectTransform.pivot = Vector2.up;
			rectTransform.position = position;
			rectTransform.SetAsLastSibling();
		}
	}

	private void RefreshCountDown()
	{
		if (!(target == null))
		{
			if (target.LimitedLifeTime)
			{
				float remainingTime = target.RemainingTime;
				text_CountDown.text = $"{remainingTime:0.0}s";
			}
			else
			{
				text_CountDown.text = "";
			}
		}
	}

	public void Show()
	{
		fadeGroup.Show();
		timeWhenShowStarted = Time.unscaledTime;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (fadeGroup.IsShown || fadeGroup.IsShowingInProgress)
		{
			punchReceiver.Punch();
			fadeGroup.Hide();
		}
	}
}
