using UnityEngine;

namespace Duckov.UI.Animations;

public class LocalPositionLooper : LooperElement
{
	[SerializeField]
	private Vector3 localPositionA;

	[SerializeField]
	private Vector3 localPositionB;

	[SerializeField]
	private AnimationCurve curve;

	protected override void OnTick(LooperClock clock, float t)
	{
		if (!(base.transform == null))
		{
			Vector2 vector = Vector2.Lerp(localPositionA, localPositionB, curve.Evaluate(t));
			base.transform.localPosition = vector;
		}
	}
}
