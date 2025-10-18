using System.Collections.Generic;
using UnityEngine;

public class BezierSpline : ShapeProvider
{
	public PipeRenderer pipeRenderer;

	public Vector3[] points = new Vector3[4];

	public int subdivisions = 12;

	public bool drawGizmos;

	public static Vector3 GetPoint(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
	{
		float num = Mathf.Pow(1f - t, 3f);
		float num2 = 3f * t * (1f - t) * (1f - t);
		float num3 = 3f * t * t * (1f - t);
		float num4 = t * t * t;
		return num * p1 + num2 * p2 + num3 * p3 + num4 * p4;
	}

	public static Vector3 GetTangent(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
	{
		float num = -3f * (1f - t) * (1f - t);
		float num2 = 3f * (1f - t) * (1f - t) - 6f * t * (1f - t);
		float num3 = 6f * t * (1f - t) - 3f * t * t;
		float num4 = 3f * t * t;
		return num * p1 + num2 * p2 + num3 * p3 + num4 * p4;
	}

	public static Vector3 GetNormal(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
	{
		Vector3 tangent = GetTangent(p1, p2, p3, p4, t);
		Vector3 vector = 6f * (1f - t) * p1 - (6f * (1f - t) + (6f - 12f * t)) * p2 + (6f - 12f * t - 6f * t) * p3 + 6f * t * p4;
		Vector3 normalized = tangent.normalized;
		Vector3 normalized2 = (normalized + vector).normalized;
		return Vector3.Cross(Vector3.Cross(normalized, normalized2), normalized).normalized;
	}

	public static PipeRenderer.OrientedPoint[] GenerateShape(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int subdivisions)
	{
		List<PipeRenderer.OrientedPoint> list = new List<PipeRenderer.OrientedPoint>();
		float num = 1f / (float)subdivisions;
		float num2 = 0f;
		Vector3 vector = Vector3.zero;
		for (int i = 0; i <= subdivisions; i++)
		{
			float t = (float)i * num;
			Vector3 point = GetPoint(p0, p1, p2, p3, t);
			Vector3 tangent = GetTangent(p0, p1, p2, p3, t);
			Vector3 normal = GetNormal(p0, p1, p2, p3, t);
			if (i > 0)
			{
				num2 += (point - vector).magnitude;
			}
			Quaternion identity = Quaternion.identity;
			identity = Quaternion.LookRotation(tangent, normal);
			PipeRenderer.OrientedPoint item = new PipeRenderer.OrientedPoint
			{
				position = point,
				tangent = tangent,
				normal = normal,
				rotation = identity,
				rotationalAxisVector = Vector3.forward,
				uv = Vector2.one * num2
			};
			list.Add(item);
			vector = point;
		}
		PipeRenderer.OrientedPoint[] result = list.ToArray();
		PipeHelperFunctions.RecalculateNormals(ref result);
		return result;
	}

	public override PipeRenderer.OrientedPoint[] GenerateShape()
	{
		List<PipeRenderer.OrientedPoint> list = new List<PipeRenderer.OrientedPoint>();
		float num = 1f / (float)subdivisions;
		Vector3 p = points[0];
		Vector3 p2 = points[1];
		Vector3 p3 = points[2];
		Vector3 p4 = points[3];
		float num2 = 0f;
		Vector3 vector = Vector3.zero;
		for (int i = 0; i <= subdivisions; i++)
		{
			float t = (float)i * num;
			Vector3 point = GetPoint(p, p2, p3, p4, t);
			Vector3 tangent = GetTangent(p, p2, p3, p4, t);
			Vector3 normal = GetNormal(p, p2, p3, p4, t);
			if (i > 0)
			{
				num2 += (point - vector).magnitude;
			}
			Quaternion identity = Quaternion.identity;
			identity = Quaternion.LookRotation(tangent, normal);
			PipeRenderer.OrientedPoint item = new PipeRenderer.OrientedPoint
			{
				position = point,
				tangent = tangent,
				normal = normal,
				rotation = identity,
				rotationalAxisVector = Vector3.forward,
				uv = Vector2.one * num2
			};
			list.Add(item);
			vector = point;
		}
		PipeRenderer.OrientedPoint[] result = list.ToArray();
		PipeHelperFunctions.RecalculateNormals(ref result);
		return result;
	}

	private void OnDrawGizmosSelected()
	{
		if (drawGizmos)
		{
			Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
			for (int i = 0; i < points.Length; i++)
			{
				Gizmos.DrawWireCube(localToWorldMatrix.MultiplyPoint(points[i]), Vector3.one * 0.1f);
			}
			float num = 1f / (float)subdivisions;
			for (int j = 0; j < subdivisions; j++)
			{
				Vector3 point = GetPoint(points[0], points[1], points[2], points[3], num * (float)j);
				Vector3 point2 = GetPoint(points[0], points[1], points[2], points[3], num * (float)(j + 1));
				point = localToWorldMatrix.MultiplyPoint(point);
				point2 = localToWorldMatrix.MultiplyPoint(point2);
				Gizmos.DrawLine(point, point2);
				Vector3 tangent = GetTangent(points[0], points[1], points[2], points[3], num * (float)j);
				tangent = localToWorldMatrix.MultiplyVector(tangent);
				Vector3 to = point + tangent * 0.1f;
				Gizmos.DrawLine(point, to);
			}
		}
	}
}
