using UnityEngine;
using UnityEngine.InputSystem;

namespace Duckov.UI;

public class FollowCursor : MonoBehaviour
{
	private RectTransform parentRectTransform;

	private RectTransform rectTransform;

	private void Awake()
	{
		parentRectTransform = base.transform.parent as RectTransform;
		rectTransform = base.transform as RectTransform;
	}

	private void Update()
	{
		Vector2 value = Mouse.current.position.value;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, value, null, out var localPoint);
		rectTransform.localPosition = localPoint;
	}
}
