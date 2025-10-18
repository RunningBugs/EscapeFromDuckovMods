using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiCirceExtrudeShape : ShapeProvider
{
	[Serializable]
	public struct Circle
	{
		public Vector3 origin;

		public float radius;
	}

	public Circle[] circles;

	public int subdivision = 4;

	public override PipeRenderer.OrientedPoint[] GenerateShape()
	{
		List<PipeRenderer.OrientedPoint> list = new List<PipeRenderer.OrientedPoint>();
		float num = 360f / (float)subdivision;
		float num2 = 1f / (float)(subdivision * circles.Length);
		for (int i = 0; i < circles.Length; i++)
		{
			Circle circle = circles[i];
			float radius = circle.radius;
			Vector3 origin = circle.origin;
			Vector3 vector = Vector3.up * radius;
			for (int j = 0; j < subdivision; j++)
			{
				Quaternion quaternion = Quaternion.AngleAxis(num * (float)j, Vector3.forward);
				Vector3 position = origin + quaternion * vector;
				list.Add(new PipeRenderer.OrientedPoint
				{
					position = position,
					rotation = quaternion,
					uv = num2 * (float)(i * subdivision + j) * Vector2.one
				});
			}
			list.Add(new PipeRenderer.OrientedPoint
			{
				position = vector,
				rotation = Quaternion.AngleAxis(0f, Vector3.forward),
				uv = num2 * (float)((i + 1) * subdivision) * Vector2.one
			});
		}
		return list.ToArray();
	}
}
