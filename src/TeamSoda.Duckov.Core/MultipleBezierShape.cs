using System.Collections.Generic;
using UnityEngine;

public class MultipleBezierShape : ShapeProvider
{
	public Vector3[] points;

	public int subdivisions = 16;

	public bool lockedHandles;

	public float rotationOffset;

	public float twist;

	public bool edit;

	public override PipeRenderer.OrientedPoint[] GenerateShape()
	{
		List<PipeRenderer.OrientedPoint> list = new List<PipeRenderer.OrientedPoint>();
		for (int i = 0; i < points.Length / 4; i++)
		{
			Vector3 p = points[i * 4];
			Vector3 p2 = points[i * 4 + 1];
			Vector3 p3 = points[i * 4 + 2];
			Vector3 p4 = points[i * 4 + 3];
			PipeRenderer.OrientedPoint[] collection = BezierSpline.GenerateShape(p, p2, p3, p4, subdivisions);
			if (list.Count > 0)
			{
				list.RemoveAt(list.Count - 1);
			}
			list.AddRange(collection);
		}
		PipeRenderer.OrientedPoint[] result = list.ToArray();
		PipeHelperFunctions.RecalculateNormals(ref result);
		PipeHelperFunctions.RecalculateUvs(ref result);
		PipeHelperFunctions.RotatePoints(ref result, rotationOffset, twist);
		return result;
	}
}
