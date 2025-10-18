using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI.Animations;

public class ChangeGraphicsColorToggle : ToggleComponent
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private Color trueColor;

	[SerializeField]
	private Color falseColor;

	[SerializeField]
	private float duration = 0.1f;

	protected override void OnSetToggle(ToggleAnimation master, bool value)
	{
		image.DOKill();
		image.DOColor(value ? trueColor : falseColor, duration);
	}
}
