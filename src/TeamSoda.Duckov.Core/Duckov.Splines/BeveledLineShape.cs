using System.Collections.Generic;
using UnityEngine;

namespace Duckov.Splines;

[RequireComponent(typeof(PipeRenderer))]
public class BeveledLineShape : ShapeProvider
{
	public PipeRenderer pipeRenderer;

	public Points pointsComponent;

	[Header("Shape")]
	public float bevelSize = 0.5f;

	[Header("Subdivide")]
	public int subdivision = 2;

	public bool subdivideByLength;

	public float subdivisionLength = 0.1f;

	[Header("UV")]
	public float uvMultiplier = 1f;

	public float uvOffset;

	[Header("Extra")]
	public bool useProtectionOffset;

	public float protectionOffset = 0.2f;

	public bool edit;

	[HideInInspector]
	public List<Vector3> points
	{
		get
		{
			if ((bool)pointsComponent)
			{
				return pointsComponent.points;
			}
			return null;
		}
	}

	public override PipeRenderer.OrientedPoint[] GenerateShape()
	{
		List<PipeRenderer.OrientedPoint> list = new List<PipeRenderer.OrientedPoint>();
		if (!pointsComponent || points.Count <= 1)
		{
			return list.ToArray();
		}
		if (pointsComponent.worldSpace)
		{
			pointsComponent.worldSpace = false;
		}
		int count = points.Count;
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = points[i];
			if (i == 0 || i == count - 1)
			{
				PipeRenderer.OrientedPoint item = new PipeRenderer.OrientedPoint
				{
					position = vector,
					tangent = ((i == 0) ? (points[i + 1] - vector).normalized : (vector - points[i - 1]).normalized),
					normal = Vector3.up,
					rotationalAxisVector = Vector3.forward
				};
				list.Add(item);
				continue;
			}
			Vector3 vector2 = points[i - 1];
			Vector3 vector3 = points[i + 1];
			Vector3 vector4 = vector - vector2;
			Vector3 vector5 = vector3 - vector2;
			Vector3 vector6 = Vector3.Cross(vector3 - vector, vector2 - vector);
			if (vector4.magnitude == 0f || vector5.magnitude == 0f || vector4.normalized == vector5.normalized || vector4.normalized == -vector5.normalized)
			{
				Vector3 normalized = (vector3 - vector).normalized;
				PipeRenderer.OrientedPoint item2 = new PipeRenderer.OrientedPoint
				{
					position = vector,
					tangent = normalized,
					normal = vector6,
					rotationalAxisVector = Vector3.forward,
					rotation = Quaternion.LookRotation(normalized, vector6),
					uv = Vector2.zero
				};
				list.Add(item2);
				continue;
			}
			float a = ((i >= 2) ? ((vector - vector2).magnitude / 2f) : (vector - vector2).magnitude);
			float b = ((i < count - 2) ? ((vector - vector3).magnitude / 2f) : (vector - vector3).magnitude);
			float clipDistance = Mathf.Min(a, b);
			Vector3 o;
			Vector3 axis;
			Vector3[] array = Bevel.Evaluate(vector, vector2, vector3, subdivision, bevelSize, out o, out axis, protectionOffset, useProtectionOffset, clipDistance);
			for (int j = 0; j < array.Length; j++)
			{
				Vector3 vector7 = array[j];
				Vector3 vector8 = ((j < array.Length - 1) ? array[j + 1] : vector3) - vector7;
				Vector3 vector9 = o - vector7;
				Vector3 vector10 = ((useProtectionOffset && (j == 0 || j == array.Length - 1)) ? vector8.normalized : (Quaternion.AngleAxis(-90f, axis) * vector9));
				float num = 0.001f;
				if (!(vector8.magnitude < num))
				{
					Quaternion rotation = Quaternion.LookRotation(vector10, vector6);
					Vector3 forward = Vector3.forward;
					PipeRenderer.OrientedPoint item3 = new PipeRenderer.OrientedPoint
					{
						position = vector7,
						tangent = vector10,
						normal = vector6,
						rotationalAxisVector = forward,
						rotation = rotation,
						uv = Vector2.zero
					};
					list.Add(item3);
				}
			}
		}
		if (subdivideByLength && subdivisionLength > 0f)
		{
			for (int k = 0; k < list.Count - 1; k++)
			{
				PipeRenderer.OrientedPoint orientedPoint = list[k];
				PipeRenderer.OrientedPoint orientedPoint2 = list[k + 1];
				Vector3 vector11 = orientedPoint2.position - orientedPoint.position;
				Vector3 normalized2 = vector11.normalized;
				float magnitude = vector11.magnitude;
				if (!(magnitude > subdivisionLength))
				{
					continue;
				}
				int num2 = Mathf.FloorToInt(magnitude / subdivisionLength);
				for (int l = 1; l <= num2; l++)
				{
					Vector3 vector12 = orientedPoint.position + l * normalized2 * subdivisionLength;
					if ((vector12 - orientedPoint2.position).magnitude < subdivisionLength)
					{
						break;
					}
					PipeRenderer.OrientedPoint item4 = new PipeRenderer.OrientedPoint
					{
						position = vector12,
						normal = orientedPoint.normal,
						rotation = orientedPoint.rotation,
						rotationalAxisVector = orientedPoint.rotationalAxisVector,
						tangent = orientedPoint.tangent,
						uv = orientedPoint.uv
					};
					list.Insert(k + l, item4);
				}
			}
		}
		PipeRenderer.OrientedPoint[] array2 = list.ToArray();
		array2 = PipeHelperFunctions.RemoveDuplicates(array2);
		PipeHelperFunctions.RecalculateNormals(ref array2);
		PipeHelperFunctions.RecalculateUvs(ref array2, uvMultiplier, uvOffset);
		return array2;
	}
}
