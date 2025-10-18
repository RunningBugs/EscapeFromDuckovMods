using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.UI.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov;

public class StrongNotification : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private FadeGroup mainFadeGroup;

	[SerializeField]
	private FadeGroup contentFadeGroup;

	[SerializeField]
	private TextMeshProUGUI textMain;

	[SerializeField]
	private TextMeshProUGUI textSub;

	[SerializeField]
	private Image image;

	[SerializeField]
	private float contentDelay = 0.5f;

	private static List<StrongNotificationContent> pending = new List<StrongNotificationContent>();

	private UniTask showingTask;

	private bool confirmed;

	public static StrongNotification Instance { get; private set; }

	private bool showing => showingTask.Status == UniTaskStatus.Pending;

	public static bool Showing
	{
		get
		{
			if (Instance == null)
			{
				return false;
			}
			return Instance.showing;
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		UIInputManager.OnConfirm += OnConfirm;
		UIInputManager.OnCancel += OnCancel;
		View.OnActiveViewChanged += View_OnActiveViewChanged;
	}

	private void OnDestroy()
	{
		UIInputManager.OnConfirm -= OnConfirm;
		UIInputManager.OnCancel -= OnCancel;
		View.OnActiveViewChanged -= View_OnActiveViewChanged;
	}

	private void View_OnActiveViewChanged()
	{
		confirmed = true;
	}

	private void OnCancel(UIInputEventData data)
	{
		confirmed = true;
	}

	private void OnConfirm(UIInputEventData data)
	{
		confirmed = true;
	}

	private void Update()
	{
		if (!showing && pending.Count > 0)
		{
			BeginShow();
		}
	}

	private void BeginShow()
	{
		showingTask = ShowTask();
	}

	private async UniTask ShowTask()
	{
		await mainFadeGroup.ShowAndReturnTask();
		await UniTask.WaitForSeconds(contentDelay, ignoreTimeScale: true);
		while (pending.Count > 0)
		{
			StrongNotificationContent cur = pending[0];
			pending.RemoveAt(0);
			await DisplayContent(cur);
		}
		await mainFadeGroup.HideAndReturnTask();
	}

	private async UniTask DisplayContent(StrongNotificationContent cur)
	{
		if (cur != null)
		{
			textMain.text = cur.mainText;
			textSub.text = cur.subText;
			if (cur.image != null)
			{
				image.sprite = cur.image;
				image.gameObject.SetActive(value: true);
			}
			else
			{
				image.gameObject.SetActive(value: false);
			}
			await contentFadeGroup.ShowAndReturnTask();
			confirmed = false;
			while (!confirmed)
			{
				await UniTask.NextFrame();
			}
			await contentFadeGroup.HideAndReturnTask();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		confirmed = true;
	}

	public static void Push(StrongNotificationContent content)
	{
		pending.Add(content);
	}

	public static void Push(string mainText, string subText = "")
	{
		pending.Add(new StrongNotificationContent(mainText, subText));
	}
}
