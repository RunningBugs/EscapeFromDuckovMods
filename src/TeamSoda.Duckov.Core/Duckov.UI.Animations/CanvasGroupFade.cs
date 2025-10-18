using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Duckov.UI.Animations;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFade : FadeElement
{
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private AnimationCurve showingCurve;

	[SerializeField]
	private AnimationCurve hidingCurve;

	[SerializeField]
	private float fadeDuration = 0.2f;

	[SerializeField]
	private bool manageBlockRaycast;

	private bool awaked;

	private float ShowingDuration => fadeDuration;

	private float HidingDuration => fadeDuration;

	private void Awake()
	{
		if (canvasGroup == null || canvasGroup.gameObject != base.gameObject)
		{
			canvasGroup = GetComponent<CanvasGroup>();
		}
		awaked = true;
	}

	private void OnValidate()
	{
		if (canvasGroup == null || canvasGroup.gameObject != base.gameObject)
		{
			canvasGroup = GetComponent<CanvasGroup>();
		}
	}

	protected override UniTask ShowTask(int taskToken)
	{
		if (canvasGroup == null)
		{
			return default(UniTask);
		}
		if (!awaked)
		{
			canvasGroup.alpha = 0f;
		}
		if (manageBlockRaycast)
		{
			canvasGroup.blocksRaycasts = true;
		}
		return FadeTask(taskToken, base.IsFading ? canvasGroup.alpha : 0f, 1f, showingCurve, ShowingDuration);
	}

	protected override UniTask HideTask(int taskToken)
	{
		if (canvasGroup == null)
		{
			return default(UniTask);
		}
		if (manageBlockRaycast)
		{
			canvasGroup.blocksRaycasts = false;
		}
		return FadeTask(taskToken, base.IsFading ? canvasGroup.alpha : 1f, 0f, hidingCurve, HidingDuration);
	}

	private async UniTask FadeTask(int token, float beginAlpha, float targetAlpha, AnimationCurve animationCurve, float duration)
	{
		float time = 0f;
		while (time < duration)
		{
			if (!CheckTaskValid())
			{
				return;
			}
			time += Time.unscaledDeltaTime;
			float time2 = time / duration;
			float t = animationCurve.Evaluate(time2);
			float alpha = Mathf.Lerp(beginAlpha, targetAlpha, t);
			canvasGroup.alpha = alpha;
			await UniTask.NextFrame();
		}
		if (CheckTaskValid())
		{
			canvasGroup.alpha = targetAlpha;
		}
		bool CheckTaskValid()
		{
			if (canvasGroup != null)
			{
				return token == base.ActiveTaskToken;
			}
			return false;
		}
	}

	protected override void OnSkipHide()
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 0f;
		}
		if (manageBlockRaycast)
		{
			canvasGroup.blocksRaycasts = false;
		}
	}

	protected override void OnSkipShow()
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 1f;
		}
		if (manageBlockRaycast)
		{
			canvasGroup.blocksRaycasts = true;
		}
	}
}
