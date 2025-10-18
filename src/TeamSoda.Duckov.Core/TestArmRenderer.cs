using UnityEngine;

public class TestArmRenderer : MonoBehaviour
{
	public LineRenderer[] lineRenderers;

	public Transform[] joints;

	private void Awake()
	{
		LineRenderer[] array = lineRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].useWorldSpace = true;
		}
	}

	private void LateUpdate()
	{
		DrawLine(lineRenderers[0], joints[0], joints[1]);
		DrawLine(lineRenderers[1], joints[1], joints[2]);
		DrawLine(lineRenderers[2], joints[3], joints[4]);
		DrawLine(lineRenderers[3], joints[4], joints[5]);
	}

	private void DrawLine(LineRenderer lineRenderer, Transform start, Transform end)
	{
		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(0, start.position);
		lineRenderer.SetPosition(1, end.position);
	}
}
