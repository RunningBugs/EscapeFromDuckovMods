using DG.Tweening;
using UnityEngine;

namespace Duckov.UI.Animations;

public class ScaleToggle : ToggleAnimation
{
	public float idleScale = 1f;

	public float activeScale = 0.9f;

	public float duration = 0.1f;

	public AnimationCurve animationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private Vector3 cachedScale = Vector3.one;

	private RectTransform rectTransform;

	private void CachePose()
	{
		cachedScale = rectTransform.localScale;
	}

	private void Awake()
	{
		rectTransform = base.transform as RectTransform;
		CachePose();
	}

	protected override void OnSetToggle(bool status)
	{
		float num = (status ? activeScale : idleScale);
		_ = num * cachedScale;
		rectTransform.DOKill();
		rectTransform.DOScale(cachedScale * num, duration).SetEase(animationCurve);
	}
}
