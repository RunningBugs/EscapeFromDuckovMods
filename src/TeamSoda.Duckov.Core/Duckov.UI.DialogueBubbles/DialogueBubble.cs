using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.UI.DialogueBubbles;

public class DialogueBubble : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private float defaultSpeed = 10f;

	[SerializeField]
	private float sustainDuration = 2f;

	[SerializeField]
	private float defaultYOffset = 2f;

	[SerializeField]
	private GameObject interactIndicator;

	private bool interacted;

	private bool animating;

	private int taskToken;

	private Transform target;

	private float _yOffset;

	private float screenYOffset = 0.06f;

	private UniTask task;

	public Transform Target => target;

	private float YOffset
	{
		get
		{
			if (!(_yOffset >= 0f))
			{
				return defaultYOffset;
			}
			return _yOffset;
		}
	}

	private void LateUpdate()
	{
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		if (!(target == null))
		{
			Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position + Vector3.up * YOffset);
			screenPoint.y += screenYOffset * (float)Screen.height;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(base.transform.parent as RectTransform, screenPoint, null, out var localPoint))
			{
				base.transform.localPosition = localPoint;
			}
		}
	}

	public UniTask Show(string text, Transform target, float yOffset = -1f, bool needInteraction = false, bool skippable = false, float speed = -1f, float duration = 2f)
	{
		task = ShowTask(text, target, yOffset, needInteraction, skippable, speed, duration);
		return task;
	}

	public async UniTask ShowTask(string text, Transform target, float yOffset = -1f, bool needInteraction = false, bool skippable = false, float speed = -1f, float duration = 2f)
	{
		_yOffset = yOffset;
		this.target = target;
		sustainDuration = duration;
		interactIndicator.gameObject.SetActive(value: false);
		int currentToken = (taskToken = Random.Range(1, int.MaxValue));
		TMP_TextInfo textInfo = this.text.GetTextInfo(text);
		if (textInfo.characterCount < 1)
		{
			animating = false;
			await Hide();
			return;
		}
		animating = true;
		this.text.text = text;
		this.text.maxVisibleCharacters = 0;
		await fadeGroup.ShowAndReturnTask();
		if (taskToken != currentToken)
		{
			return;
		}
		int characterCount = textInfo.characterCount;
		if (speed <= 0f)
		{
			speed = defaultSpeed;
		}
		interacted = false;
		for (int i = 0; i <= characterCount; i++)
		{
			this.text.maxVisibleCharacters = i;
			await UniTask.WaitForSeconds(1f / speed, ignoreTimeScale: true);
			if (taskToken != currentToken)
			{
				return;
			}
			if (target == null)
			{
				Hide().Forget();
				return;
			}
			if (!target.gameObject.activeInHierarchy)
			{
				Hide().Forget();
				return;
			}
			if (skippable && interacted)
			{
				break;
			}
		}
		this.text.maxVisibleCharacters = characterCount;
		animating = false;
		if (needInteraction)
		{
			interactIndicator.gameObject.SetActive(value: true);
			await WaitForInteraction(currentToken);
		}
		else
		{
			float startTime = Time.unscaledTime;
			float num;
			do
			{
				await UniTask.NextFrame();
				num = Time.unscaledTime - startTime;
				if (taskToken != currentToken)
				{
					return;
				}
			}
			while ((bool)target && target.gameObject.activeInHierarchy && !(num >= sustainDuration));
		}
		if (taskToken == currentToken)
		{
			Hide().Forget();
		}
	}

	private async UniTask WaitForInteraction(int currentToken)
	{
		interacted = false;
		do
		{
			await UniTask.NextFrame();
		}
		while (currentToken == taskToken && !interacted && target.gameObject.activeInHierarchy);
	}

	public void Interact()
	{
		interacted = true;
	}

	private async UniTask Hide()
	{
		animating = false;
		await fadeGroup.HideAndReturnTask();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Interact();
	}

	private void Awake()
	{
		DialogueBubblesManager.onPointerClick += OnPointerClick;
	}
}
