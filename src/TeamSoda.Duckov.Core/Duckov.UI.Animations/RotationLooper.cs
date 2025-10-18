using UnityEngine;

namespace Duckov.UI.Animations;

public class RotationLooper : LooperElement
{
	[SerializeField]
	private Vector3 eulerRotationA;

	[SerializeField]
	private Vector3 eulerRotationB;

	[SerializeField]
	private AnimationCurve curve;

	protected override void OnTick(LooperClock clock, float t)
	{
		if (!(base.transform == null))
		{
			Vector3 euler = Vector3.Lerp(eulerRotationA, eulerRotationB, curve.Evaluate(t));
			base.transform.localRotation = Quaternion.Euler(euler);
		}
	}
}
