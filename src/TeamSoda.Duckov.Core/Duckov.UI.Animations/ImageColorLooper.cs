using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI.Animations;

public class ImageColorLooper : LooperElement
{
	[SerializeField]
	private Image image;

	[GradientUsage(true)]
	[SerializeField]
	private Gradient colorOverT;

	[SerializeField]
	private AnimationCurve alphaOverT;

	protected override void OnTick(LooperClock clock, float t)
	{
		Color color = colorOverT.Evaluate(t);
		float num = alphaOverT.Evaluate(t);
		color.a *= num;
		image.color = color;
	}
}
