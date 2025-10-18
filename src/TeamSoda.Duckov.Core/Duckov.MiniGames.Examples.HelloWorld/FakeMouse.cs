using UnityEngine;

namespace Duckov.MiniGames.Examples.HelloWorld;

public class FakeMouse : MiniGameBehaviour
{
	[SerializeField]
	private float sensitivity = 1f;

	private RectTransform rectTransform;

	private RectTransform parentRectTransform;

	private void Awake()
	{
		rectTransform = base.transform as RectTransform;
		parentRectTransform = base.transform.parent as RectTransform;
	}

	protected override void OnUpdate(float deltaTime)
	{
		Vector3 localPosition = rectTransform.localPosition;
		localPosition += (Vector3)base.Game.GetAxis(1) * sensitivity;
		Rect rect = parentRectTransform.rect;
		localPosition.x = Mathf.Clamp(localPosition.x, rect.xMin, rect.xMax);
		localPosition.y = Mathf.Clamp(localPosition.y, rect.yMin, rect.yMax);
		rectTransform.localPosition = localPosition;
	}
}
