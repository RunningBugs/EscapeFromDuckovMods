using DG.Tweening;
using UnityEngine;

namespace Duckov.UI.Animations;

public class SizeDeltaToggle : ToggleAnimation
{
	public Vector2 idleSizeDelta = Vector2.zero;

	public Vector2 activeSizeDelta = Vector2.one * 12f;

	public float duration = 0.1f;

	public AnimationCurve animationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private Vector2 cachedSizeDelta = Vector3.one;

	private RectTransform _rectTransform;

	private RectTransform RectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	private void CachePose()
	{
		cachedSizeDelta = RectTransform.sizeDelta;
	}

	private void Awake()
	{
		CachePose();
	}

	protected override void OnSetToggle(bool status)
	{
		if (base.gameObject.activeInHierarchy)
		{
			Vector2 endValue = (status ? activeSizeDelta : idleSizeDelta);
			RectTransform.DOKill();
			RectTransform.DOSizeDelta(endValue, duration).SetEase(animationCurve);
		}
	}
}
