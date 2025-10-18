using System.Collections.Generic;
using UnityEngine;

public class RoundCornerRectExtrudeShape : ShapeProvider
{
	public Vector2 size = Vector2.one;

	public float bevelSize = 0.1f;

	public bool resample;

	[Range(0.1f, 1f)]
	public float stepLength = 0.25f;

	public override PipeRenderer.OrientedPoint[] GenerateShape()
	{
		float num = bevelSize;
		Vector2 vector = new Vector2((0f - size.x) / 2f + num, size.y / 2f - num);
		Vector2 vector2 = vector + new Vector2(-1f, 1f).normalized * num;
		Vector2 vector3 = vector + new Vector2(0f, 1f) * num;
		PipeRenderer.OrientedPoint orientedPoint = new PipeRenderer.OrientedPoint
		{
			position = vector2,
			normal = new Vector2(-1f, 1f),
			uv = Vector2.zero
		};
		PipeRenderer.OrientedPoint orientedPoint2 = new PipeRenderer.OrientedPoint
		{
			position = vector3,
			normal = new Vector3(0f, 1f),
			uv = num * Vector2.one
		};
		PipeRenderer.OrientedPoint orientedPoint3 = orientedPoint2;
		orientedPoint3.position.x = 0f - orientedPoint3.position.x;
		orientedPoint3.normal.x = 0f - orientedPoint3.normal.x;
		orientedPoint3.uv = Vector2.one - orientedPoint3.uv;
		PipeRenderer.OrientedPoint orientedPoint4 = orientedPoint;
		orientedPoint4.position.x = 0f - orientedPoint4.position.x;
		orientedPoint4.normal.x = 0f - orientedPoint4.normal.x;
		orientedPoint4.uv = Vector2.one;
		PipeRenderer.OrientedPoint item = orientedPoint4;
		item.uv = Vector2.zero;
		PipeRenderer.OrientedPoint orientedPoint5 = default(PipeRenderer.OrientedPoint);
		orientedPoint5.position = vector;
		orientedPoint5.position.x = 0f - orientedPoint5.position.x;
		orientedPoint5.position.x += num;
		orientedPoint5.normal = new Vector3(1f, 0f);
		orientedPoint5.uv = orientedPoint2.uv;
		PipeRenderer.OrientedPoint orientedPoint6 = orientedPoint5;
		orientedPoint6.position.y = 0f - orientedPoint6.position.y;
		orientedPoint6.uv = orientedPoint3.uv;
		PipeRenderer.OrientedPoint orientedPoint7 = orientedPoint4;
		orientedPoint7.position.y = 0f - orientedPoint7.position.y;
		orientedPoint7.normal = new Vector2(1f, -1f);
		orientedPoint7.uv = Vector2.one;
		PipeRenderer.OrientedPoint item2 = orientedPoint7;
		item2.uv = Vector2.zero;
		PipeRenderer.OrientedPoint item3 = orientedPoint3;
		item3.position.y = 0f - item3.position.y;
		item3.normal = Vector2.down;
		item3.uv = orientedPoint5.uv;
		PipeRenderer.OrientedPoint item4 = orientedPoint2;
		item4.position.y = 0f - item4.position.y;
		item4.normal = Vector2.down;
		item4.uv = orientedPoint3.uv;
		PipeRenderer.OrientedPoint orientedPoint8 = orientedPoint;
		orientedPoint8.position.y = 0f - orientedPoint8.position.y;
		orientedPoint8.normal = new Vector2(-1f, -1f);
		orientedPoint8.uv = Vector2.one;
		PipeRenderer.OrientedPoint item5 = orientedPoint8;
		item5.uv = Vector2.zero;
		PipeRenderer.OrientedPoint item6 = orientedPoint6;
		item6.position.x = 0f - item6.position.x;
		item6.normal = Vector2.left;
		item6.uv = orientedPoint2.uv;
		PipeRenderer.OrientedPoint orientedPoint9 = orientedPoint5;
		orientedPoint9.position.x = 0f - orientedPoint9.position.x;
		orientedPoint9.normal = Vector2.left;
		orientedPoint9.uv = orientedPoint3.uv;
		PipeRenderer.OrientedPoint item7 = orientedPoint9;
		item7.uv = Vector2.zero;
		List<PipeRenderer.OrientedPoint> list = new List<PipeRenderer.OrientedPoint>();
		list.Add(orientedPoint);
		list.Add(orientedPoint2);
		list.Add(orientedPoint3);
		list.Add(orientedPoint4);
		list.Add(item);
		list.Add(orientedPoint5);
		list.Add(orientedPoint6);
		list.Add(orientedPoint7);
		list.Add(item2);
		list.Add(item3);
		list.Add(item4);
		list.Add(orientedPoint8);
		list.Add(item5);
		list.Add(item6);
		list.Add(orientedPoint9);
		list.Add(item7);
		list.Reverse();
		if (resample)
		{
			list = Resample(list, stepLength);
		}
		return list.ToArray();
	}

	private List<PipeRenderer.OrientedPoint> Resample(List<PipeRenderer.OrientedPoint> original, float stepLength)
	{
		if (stepLength < 0.01f)
		{
			return new List<PipeRenderer.OrientedPoint>();
		}
		List<PipeRenderer.OrientedPoint> list = new List<PipeRenderer.OrientedPoint>();
		int i = 0;
		float num = 0f;
		for (; i < original.Count; i++)
		{
			PipeRenderer.OrientedPoint orientedPoint = original[i];
			PipeRenderer.OrientedPoint orientedPoint2 = ((i + 1 >= original.Count) ? original[0] : original[i + 1]);
			Vector3 vector = orientedPoint2.position - orientedPoint.position;
			Vector3 normalized = vector.normalized;
			float magnitude = vector.magnitude;
			for (float num2 = 0f; num2 < magnitude; num2 += stepLength)
			{
				Vector3 position = orientedPoint.position + normalized * num2;
				float t = num2 / magnitude;
				num += num2;
				PipeRenderer.OrientedPoint item = new PipeRenderer.OrientedPoint
				{
					position = position,
					normal = Vector3.Lerp(orientedPoint.normal, orientedPoint2.normal, t),
					uv = num * Vector2.one
				};
				list.Add(item);
			}
		}
		return list;
	}
}
