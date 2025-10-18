using UnityEngine;

namespace Duckov.UI.Animations;

public class AnchoredPositionLooper : LooperElement
{
	[SerializeField]
	private Vector2 anchoredPositionA;

	[SerializeField]
	private Vector2 anchoredPositionB;

	[SerializeField]
	private AnimationCurve curve;

	private RectTransform rectTransform;

	private void Awake()
	{
		rectTransform = base.transform as RectTransform;
	}

	protected override void OnTick(LooperClock clock, float t)
	{
		if (!(rectTransform == null))
		{
			Vector2 anchoredPosition = Vector2.Lerp(anchoredPositionA, anchoredPositionB, curve.Evaluate(t));
			rectTransform.anchoredPosition = anchoredPosition;
		}
	}
}
