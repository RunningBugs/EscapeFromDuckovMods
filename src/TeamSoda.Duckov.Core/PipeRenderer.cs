using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PipeRenderer : MonoBehaviour
{
	[Serializable]
	public struct OrientedPoint
	{
		public Vector3 position;

		public Quaternion rotation;

		public Vector3 tangent;

		public Vector3 rotationalAxisVector;

		public Vector3 normal;

		public Vector2 uv;
	}

	public MeshFilter meshFilter;

	public ShapeProvider splineShapeProvider;

	public ShapeProvider extrudeShapeProvider;

	[Header("UV")]
	public float uvTwist;

	[Header("Options")]
	public float extrudeShapeScale = 1f;

	public float sectionLength = 10f;

	public bool caps;

	public bool useExtrudeShapeScaleCurve;

	public AnimationCurve extrudeShapeScaleCurve = AnimationCurve.Constant(0f, 1f, 1f);

	public Color vertexColor = Color.white;

	public bool recalculateNormal = true;

	public bool revertFaces;

	[Header("Gizmos")]
	public bool drawSplinePoints;

	public OrientedPoint[] splineInUse;

	public OrientedPoint[] extrudeShape
	{
		get
		{
			if (extrudeShapeProvider == null)
			{
				Debug.LogWarning("Extrude shape is null, please add an extrude shape such as \"CircularExtrudeShape\"");
				return new OrientedPoint[1]
				{
					new OrientedPoint
					{
						position = Vector3.zero
					}
				};
			}
			return extrudeShapeProvider.GenerateShape();
		}
	}

	public OrientedPoint[] splineShape
	{
		get
		{
			if (splineShapeProvider == null)
			{
				Debug.LogWarning("Spline shape is null, please add a spline shape such as \"Beveled Line Shape\"");
				return new OrientedPoint[1]
				{
					new OrientedPoint
					{
						position = Vector3.zero
					}
				};
			}
			return splineShapeProvider.GenerateShape();
		}
	}

	public float GetTotalLength()
	{
		return PipeHelperFunctions.GetTotalLength(splineInUse);
	}

	public static Mesh GeneratePipeMesh(OrientedPoint[] extrudeShape, OrientedPoint[] splineShape, Color vertexColor, float uvTwist = 0f, float extrudeShapeScale = 1f, AnimationCurve extrudeShapeScaleCurve = null, float sectionLength = 0f, bool caps = false, bool recalculateNormal = true, bool revertFaces = false)
	{
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		List<int> list3 = new List<int>();
		List<Vector2> list4 = new List<Vector2>();
		float num = 0f;
		float totalLength = PipeHelperFunctions.GetTotalLength(splineShape);
		if (sectionLength <= 0f)
		{
			sectionLength = totalLength;
		}
		for (int i = 0; i < splineShape.Length; i++)
		{
			OrientedPoint orientedPoint = splineShape[i];
			Vector3 position = orientedPoint.position;
			Quaternion rotation = orientedPoint.rotation;
			if (i > 0)
			{
				OrientedPoint orientedPoint2 = splineShape[i - 1];
				num += (position - orientedPoint2.position).magnitude;
			}
			float time = num % sectionLength / sectionLength;
			float num2 = extrudeShapeScaleCurve?.Evaluate(time) ?? 1f;
			for (int j = 0; j < extrudeShape.Length; j++)
			{
				OrientedPoint orientedPoint3 = extrudeShape[j];
				Vector3 vector = position + extrudeShapeScale * num2 * (rotation * orientedPoint3.position);
				Vector3 vector2 = (recalculateNormal ? (vector - position).normalized : (rotation * orientedPoint3.normal));
				Vector2 item = new Vector2(orientedPoint3.uv.y + num * uvTwist, orientedPoint.uv.x);
				list.Add(vector);
				list2.Add(revertFaces ? (-vector2) : vector2);
				list4.Add(item);
			}
			if (i <= 0)
			{
				continue;
			}
			int num3 = i * extrudeShape.Length;
			for (int k = 0; k < extrudeShape.Length - 1; k++)
			{
				int num4 = num3 + k;
				int num5 = num3 + k + 1;
				int item2 = num5 - extrudeShape.Length;
				int item3 = num4 - extrudeShape.Length;
				if (revertFaces)
				{
					list3.Add(num5);
					list3.Add(item3);
					list3.Add(num4);
					list3.Add(item2);
					list3.Add(item3);
					list3.Add(num5);
				}
				else
				{
					list3.Add(num4);
					list3.Add(item3);
					list3.Add(num5);
					list3.Add(num5);
					list3.Add(item3);
					list3.Add(item2);
				}
			}
		}
		if (caps)
		{
			Vector3 item4 = -splineShape[0].tangent;
			int num6 = 0;
			int[] array = new int[extrudeShape.Length];
			for (int l = 0; l < extrudeShape.Length; l++)
			{
				int index = num6 + l;
				list.Add(list[index]);
				list2.Add(item4);
				list4.Add(list4[index]);
				array[l] = list.Count - 1;
			}
			Vector3 zero = Vector3.zero;
			for (int m = 0; m < array.Length; m++)
			{
				zero += list[m];
			}
			zero /= (float)array.Length;
			list.Add(zero);
			list2.Add(item4);
			list4.Add(Vector2.one * 0f / 5f);
			int item5 = list.Count - 1;
			for (int n = 0; n < array.Length - 1; n++)
			{
				list3.Add(item5);
				list3.Add(array[n + 1]);
				list3.Add(array[n]);
			}
			item4 = splineShape[^1].tangent;
			num6 = extrudeShape.Length * (splineShape.Length - 1);
			for (int num7 = 0; num7 < extrudeShape.Length; num7++)
			{
				int index2 = num6 + num7;
				list.Add(list[index2]);
				list2.Add(item4);
				list4.Add(list4[index2]);
				array[num7] = list.Count - 1;
			}
			zero = Vector3.zero;
			for (int num8 = 0; num8 < array.Length; num8++)
			{
				zero += list[array[num8]];
			}
			zero /= (float)array.Length;
			list.Add(zero);
			list2.Add(item4);
			list4.Add(Vector2.one * 0f / 5f);
			item5 = list.Count - 1;
			for (int num9 = 0; num9 < array.Length - 1; num9++)
			{
				list3.Add(array[num9]);
				list3.Add(array[num9 + 1]);
				list3.Add(item5);
			}
		}
		Color[] array2 = new Color[list.Count];
		for (int num10 = 0; num10 < array2.Length; num10++)
		{
			array2[num10] = vertexColor;
		}
		Mesh mesh = new Mesh();
		mesh.vertices = list.ToArray();
		mesh.uv = list4.ToArray();
		mesh.triangles = list3.ToArray();
		mesh.normals = list2.ToArray();
		mesh.RecalculateTangents();
		mesh.colors = array2;
		mesh.name = "Generated Mesh";
		return mesh;
	}

	private void OnDrawGizmosSelected()
	{
		if (meshFilter == null)
		{
			meshFilter = GetComponent<MeshFilter>();
		}
		splineInUse = splineShape;
		meshFilter.mesh = GeneratePipeMesh(extrudeShape, splineInUse, vertexColor, uvTwist, extrudeShapeScale, useExtrudeShapeScaleCurve ? extrudeShapeScaleCurve : null, sectionLength, caps, recalculateNormal, revertFaces);
		if (drawSplinePoints)
		{
			Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
			for (int i = 0; i < splineInUse.Length; i++)
			{
				OrientedPoint orientedPoint = splineInUse[i];
				Vector3 vector = localToWorldMatrix.MultiplyPoint(orientedPoint.position);
				Gizmos.DrawWireCube(vector, Vector3.one * 0.01f);
				Vector3 vector2 = localToWorldMatrix.MultiplyVector(orientedPoint.tangent);
				Gizmos.DrawLine(vector, vector + vector2 * 0.02f);
			}
		}
	}

	private void Start()
	{
		if (meshFilter == null)
		{
			meshFilter = GetComponent<MeshFilter>();
		}
	}

	public Vector3 GetPositionByOffset(float offset, out Quaternion rotation)
	{
		if (splineInUse == null || splineInUse.Length < 1)
		{
			rotation = Quaternion.identity;
			return Vector3.zero;
		}
		if (offset <= 0f)
		{
			rotation = splineInUse[0].rotation;
			return splineInUse[0].position;
		}
		float num = 0f;
		for (int i = 1; i < splineInUse.Length; i++)
		{
			OrientedPoint orientedPoint = splineInUse[i];
			OrientedPoint orientedPoint2 = splineInUse[i - 1];
			float num2 = num;
			float magnitude = (orientedPoint.position - orientedPoint2.position).magnitude;
			num += magnitude;
			float num3 = num;
			if (num3 > offset)
			{
				float num4 = num3 - num2;
				float num5 = (offset - num2) / num4;
				rotation = Quaternion.Lerp(orientedPoint2.rotation, orientedPoint.rotation, num5);
				return orientedPoint2.position + (orientedPoint.position - orientedPoint2.position) * num5;
			}
		}
		rotation = splineInUse[splineInUse.Length - 1].rotation;
		return splineInUse[splineInUse.Length - 1].position;
	}
}
