using UnityEngine;

namespace Duckov.MiniGames.Examples.FPS;

public class UICrossHair : MiniGameBehaviour
{
	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private RectTransform canvasRectTransform;

	[SerializeField]
	private FPSGunControl gunControl;

	private float ScatterAngle
	{
		get
		{
			if ((bool)gunControl)
			{
				return gunControl.ScatterAngle;
			}
			return 0f;
		}
	}

	private void Awake()
	{
		if (rectTransform == null)
		{
			rectTransform = GetComponent<RectTransform>();
		}
	}

	protected override void OnUpdate(float deltaTime)
	{
		float scatterAngle = ScatterAngle;
		float fieldOfView = base.Game.Camera.fieldOfView;
		float y = canvasRectTransform.sizeDelta.y;
		float num = scatterAngle / fieldOfView;
		float num2 = Mathf.FloorToInt(y * num / 2f) * 2 + 1;
		rectTransform.sizeDelta = num2 * Vector2.one;
	}
}
