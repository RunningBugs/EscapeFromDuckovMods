using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Duckov.UI.Animations;

public class ScaleFade : FadeElement
{
	[SerializeField]
	private float duration = 0.1f;

	[SerializeField]
	private Vector3 scale = Vector3.zero;

	[SerializeField]
	[Range(-1f, 1f)]
	private float uniformScale;

	[SerializeField]
	private AnimationCurve showCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve hideCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private Vector3 cachedScale = Vector3.one;

	private bool initialized;

	private Vector3 HiddenScale => Vector3.one + Vector3.one * uniformScale + scale;

	private void CachePose()
	{
		cachedScale = base.transform.localScale;
	}

	private void RestorePose()
	{
		base.transform.localScale = cachedScale;
	}

	private void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			CachePose();
		}
	}

	protected override UniTask HideTask(int token)
	{
		if (!initialized)
		{
			Initialize();
		}
		if (!base.transform)
		{
			return UniTask.CompletedTask;
		}
		return base.transform.DOScale(HiddenScale, duration).SetEase(hideCurve).ToUniTask();
	}

	protected override void OnSkipHide()
	{
		if (!initialized)
		{
			Initialize();
		}
		base.transform.localScale = HiddenScale;
	}

	protected override void OnSkipShow()
	{
		if (!initialized)
		{
			Initialize();
		}
		RestorePose();
	}

	protected override UniTask ShowTask(int token)
	{
		if (!initialized)
		{
			Initialize();
		}
		return base.transform.DOScale(cachedScale, duration).SetEase(showCurve).OnComplete(RestorePose)
			.ToUniTask();
	}
}
