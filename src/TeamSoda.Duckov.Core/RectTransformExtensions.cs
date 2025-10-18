using UnityEngine;

public static class RectTransformExtensions
{
	public static Camera GetUICamera()
	{
		return null;
	}

	public static void MatchWorldPosition(this RectTransform rectTransform, Vector3 worldPosition, Vector3 worldSpaceOffset = default(Vector3))
	{
		RectTransform rectTransform2 = rectTransform.parent as RectTransform;
		if (!(rectTransform2 == null))
		{
			worldPosition += worldSpaceOffset;
			Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform2, screenPoint, GetUICamera(), out var localPoint);
			rectTransform.localPosition = localPoint;
		}
	}
}
