using UnityEngine;

[RequireComponent(typeof(PipeRenderer))]
public class CircularExtrudeShape : ShapeProvider
{
	public PipeRenderer pipeRenderer;

	public float radius = 1f;

	public int subdivision = 12;

	public Vector3 offset = Vector3.zero;

	public override PipeRenderer.OrientedPoint[] GenerateShape()
	{
		Vector3 vector = Vector3.up * radius;
		float num = 360f / (float)subdivision;
		float num2 = 1f / (float)subdivision;
		PipeRenderer.OrientedPoint[] array = new PipeRenderer.OrientedPoint[subdivision + 1];
		for (int i = 0; i < subdivision; i++)
		{
			Quaternion quaternion = Quaternion.AngleAxis(num * (float)i, Vector3.forward);
			Vector3 position = quaternion * vector + offset;
			array[i] = new PipeRenderer.OrientedPoint
			{
				position = position,
				rotation = quaternion,
				uv = num2 * (float)i * Vector2.one
			};
		}
		array[subdivision] = new PipeRenderer.OrientedPoint
		{
			position = vector + offset,
			rotation = Quaternion.AngleAxis(0f, Vector3.forward),
			uv = Vector2.one
		};
		return array;
	}

	private void OnDrawGizmosSelected()
	{
		if (pipeRenderer == null)
		{
			pipeRenderer = GetComponent<PipeRenderer>();
		}
		if (pipeRenderer != null && pipeRenderer.extrudeShapeProvider == null)
		{
			pipeRenderer.extrudeShapeProvider = this;
		}
	}
}
