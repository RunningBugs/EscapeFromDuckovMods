using UnityEngine;

namespace Duckov.UI.Animations;

public class ScaleLooper : LooperElement
{
	[SerializeField]
	private AnimationCurve uniformScaleOverT = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	[SerializeField]
	private AnimationCurve xOverT = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	[SerializeField]
	private AnimationCurve yOverT = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	[SerializeField]
	private AnimationCurve zOverT = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	protected override void OnTick(LooperClock clock, float t)
	{
		float num = xOverT.Evaluate(t);
		float num2 = yOverT.Evaluate(t);
		float num3 = zOverT.Evaluate(t);
		float num4 = uniformScaleOverT.Evaluate(t);
		num *= num4;
		num2 *= num4;
		num3 *= num4;
		base.transform.localScale = new Vector3(num, num2, num3);
	}
}
