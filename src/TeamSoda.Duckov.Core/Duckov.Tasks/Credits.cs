using Cysharp.Threading.Tasks;
using Duckov.Achievements;
using Duckov.UI.Animations;
using UnityEngine;

namespace Duckov.Tasks;

public class Credits : MonoBehaviour, ITaskBehaviour
{
	private RectTransform rectTransform;

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private RectTransform content;

	[SerializeField]
	private float scrollSpeed;

	[SerializeField]
	private float holdForSeconds;

	[SerializeField]
	private bool fadeOut;

	[SerializeField]
	private bool mute;

	private UniTask task;

	private bool skip;

	private void Awake()
	{
		rectTransform = base.transform as RectTransform;
	}

	public void Begin()
	{
		if (task.Status != UniTaskStatus.Pending)
		{
			skip = false;
			fadeGroup.SkipHide();
			fadeGroup.gameObject.SetActive(value: true);
			task = Task();
		}
	}

	public bool IsPending()
	{
		return task.Status == UniTaskStatus.Pending;
	}

	public bool IsComplete()
	{
		return !IsPending();
	}

	private async UniTask Task()
	{
		if (!mute)
		{
			AudioManager.PlayBGM("mus_main_theme");
		}
		fadeGroup.Show();
		await UniTask.Yield();
		content.anchoredPosition = Vector3.zero;
		float height = rectTransform.rect.height;
		float height2 = content.rect.height;
		float yMax = height * 0.5f + height2;
		float y = 0f;
		while (y < yMax)
		{
			y += Time.deltaTime * scrollSpeed * (skip ? 20f : 1f);
			content.anchoredPosition = Vector3.up * y;
			await UniTask.Yield();
		}
		float holdBuffer = 0f;
		while (!skip)
		{
			await UniTask.Yield();
			holdBuffer += Time.unscaledDeltaTime;
			if (holdBuffer > holdForSeconds)
			{
				break;
			}
		}
		if (fadeOut)
		{
			await fadeGroup.HideAndReturnTask();
		}
		if (AchievementManager.Instance != null)
		{
			AchievementManager.Instance.Unlock("Escape_From_Duckov");
		}
		if (!mute)
		{
			AudioManager.StopBGM();
		}
	}

	public void Skip()
	{
		skip = true;
		if (fadeOut && fadeGroup.IsFading)
		{
			fadeGroup.SkipHide();
		}
		if (!mute)
		{
			AudioManager.StopBGM();
		}
	}
}
