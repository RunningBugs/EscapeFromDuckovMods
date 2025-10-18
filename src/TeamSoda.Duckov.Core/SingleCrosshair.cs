using UnityEngine;

public class SingleCrosshair : MonoBehaviour
{
	public float rotation;

	public Vector3 axis;

	public float minDistance;

	public float scatterMoveScale = 5f;

	private float currentScatter;

	public bool controlRectWidthHeight;

	public float minScale = 100f;

	public float scatterScaleFactor = 5f;

	public void UpdateScatter(float _scatter)
	{
		currentScatter = _scatter;
		RectTransform rectTransform = base.transform as RectTransform;
		rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
		Vector3 vector = Vector3.zero;
		if (axis != Vector3.zero)
		{
			vector = rectTransform.parent.InverseTransformDirection(rectTransform.TransformDirection(axis));
		}
		rectTransform.anchoredPosition = vector * (minDistance + currentScatter * scatterMoveScale);
		if (controlRectWidthHeight)
		{
			float num = minScale + currentScatter * scatterScaleFactor;
			rectTransform.sizeDelta = Vector2.one * num;
		}
	}

	private void OnValidate()
	{
		UpdateScatter(0f);
	}
}
