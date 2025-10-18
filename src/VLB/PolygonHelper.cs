using System.Collections.Generic;
using UnityEngine;

namespace VLB;

public class PolygonHelper : MonoBehaviour
{
	public struct Plane2D
	{
		public Vector2 normal;

		public float distance;

		public float Distance(Vector2 point)
		{
			return Vector2.Dot(normal, point) + distance;
		}

		public Vector2 ClosestPoint(Vector2 pt)
		{
			return pt - normal * Distance(pt);
		}

		public Vector2 Intersect(Vector2 p1, Vector2 p2)
		{
			float num = Vector2.Dot(normal, p1 - p2);
			if (Utils.IsAlmostZero(num))
			{
				return (p1 + p2) * 0.5f;
			}
			float num2 = (normal.x * p1.x + normal.y * p1.y + distance) / num;
			return p1 + num2 * (p2 - p1);
		}

		public bool GetSide(Vector2 point)
		{
			return Distance(point) > 0f;
		}

		public static Plane2D FromPoints(Vector3 p1, Vector3 p2)
		{
			Vector3 normalized = (p2 - p1).normalized;
			return new Plane2D
			{
				normal = new Vector2(normalized.y, 0f - normalized.x),
				distance = (0f - normalized.y) * p1.x + normalized.x * p1.y
			};
		}

		public static Plane2D FromNormalAndPoint(Vector3 normalizedNormal, Vector3 p1)
		{
			return new Plane2D
			{
				normal = normalizedNormal,
				distance = (0f - normalizedNormal.x) * p1.x - normalizedNormal.y * p1.y
			};
		}

		public void Flip()
		{
			normal = -normal;
			distance = 0f - distance;
		}

		public Vector2[] CutConvex(Vector2[] poly)
		{
			List<Vector2> list = new List<Vector2>(poly.Length);
			Vector2 vector = poly[^1];
			foreach (Vector2 vector2 in poly)
			{
				bool side = GetSide(vector);
				bool side2 = GetSide(vector2);
				if (side && side2)
				{
					list.Add(vector2);
				}
				else if (side && !side2)
				{
					list.Add(Intersect(vector, vector2));
				}
				else if (!side && side2)
				{
					list.Add(Intersect(vector, vector2));
					list.Add(vector2);
				}
				vector = vector2;
			}
			return list.ToArray();
		}

		public override string ToString()
		{
			return $"{normal.x} x {normal.y} + {distance}";
		}
	}
}
