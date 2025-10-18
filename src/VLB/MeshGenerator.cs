using System;
using UnityEngine;

namespace VLB;

public static class MeshGenerator
{
	public enum CapMode
	{
		None,
		OneVertexPerCap_1Cap,
		OneVertexPerCap_2Caps,
		SpecificVerticesPerCap_1Cap,
		SpecificVerticesPerCap_2Caps
	}

	private const float kMinTruncatedRadius = 0.001f;

	private static float GetAngleOffset(int numSides)
	{
		if (numSides != 4)
		{
			return 0f;
		}
		return MathF.PI / 4f;
	}

	private static float GetRadiiScale(int numSides)
	{
		if (numSides != 4)
		{
			return 1f;
		}
		return Mathf.Sqrt(2f);
	}

	public static Mesh GenerateConeZ_RadiusAndAngle(float lengthZ, float radiusStart, float coneAngle, int numSides, int numSegments, bool cap, bool doubleSided)
	{
		float radiusEnd = lengthZ * Mathf.Tan(coneAngle * (MathF.PI / 180f) * 0.5f);
		return GenerateConeZ_Radii(lengthZ, radiusStart, radiusEnd, numSides, numSegments, cap, doubleSided);
	}

	public static Mesh GenerateConeZ_Angle(float lengthZ, float coneAngle, int numSides, int numSegments, bool cap, bool doubleSided)
	{
		return GenerateConeZ_RadiusAndAngle(lengthZ, 0f, coneAngle, numSides, numSegments, cap, doubleSided);
	}

	public static Mesh GenerateConeZ_Radii(float lengthZ, float radiusStart, float radiusEnd, int numSides, int numSegments, bool cap, bool doubleSided)
	{
		Mesh mesh = new Mesh();
		bool flag = cap && radiusStart > 0f;
		radiusStart = Mathf.Max(radiusStart, 0.001f);
		float radiiScale = GetRadiiScale(numSides);
		radiusStart *= radiiScale;
		radiusEnd *= radiiScale;
		int num = numSides * (numSegments + 2);
		int num2 = num;
		if (flag)
		{
			num2 += numSides + 1;
		}
		float angleOffset = GetAngleOffset(numSides);
		Vector3[] array = new Vector3[num2];
		for (int i = 0; i < numSides; i++)
		{
			float f = angleOffset + MathF.PI * 2f * (float)i / (float)numSides;
			float num3 = Mathf.Cos(f);
			float num4 = Mathf.Sin(f);
			for (int j = 0; j < numSegments + 2; j++)
			{
				float num5 = (float)j / (float)(numSegments + 1);
				float num6 = Mathf.Lerp(radiusStart, radiusEnd, num5);
				array[i + j * numSides] = new Vector3(num6 * num3, num6 * num4, num5 * lengthZ);
			}
		}
		if (flag)
		{
			int num7 = num;
			array[num7] = Vector3.zero;
			num7++;
			for (int k = 0; k < numSides; k++)
			{
				float f2 = angleOffset + MathF.PI * 2f * (float)k / (float)numSides;
				float num8 = Mathf.Cos(f2);
				float num9 = Mathf.Sin(f2);
				array[num7] = new Vector3(radiusStart * num8, radiusStart * num9, 0f);
				num7++;
			}
		}
		if (!doubleSided)
		{
			mesh.vertices = array;
		}
		else
		{
			Vector3[] array2 = new Vector3[array.Length * 2];
			array.CopyTo(array2, 0);
			array.CopyTo(array2, array.Length);
			mesh.vertices = array2;
		}
		Vector2[] array3 = new Vector2[num2];
		int num10 = 0;
		for (int l = 0; l < num; l++)
		{
			array3[num10++] = Vector2.zero;
		}
		if (flag)
		{
			for (int m = 0; m < numSides + 1; m++)
			{
				array3[num10++] = new Vector2(1f, 0f);
			}
		}
		if (!doubleSided)
		{
			mesh.uv = array3;
		}
		else
		{
			Vector2[] array4 = new Vector2[array3.Length * 2];
			array3.CopyTo(array4, 0);
			array3.CopyTo(array4, array3.Length);
			for (int n = 0; n < array3.Length; n++)
			{
				Vector2 vector = array4[n + array3.Length];
				array4[n + array3.Length] = new Vector2(vector.x, 1f);
			}
			mesh.uv = array4;
		}
		int num11 = numSides * 2 * Mathf.Max(numSegments + 1, 1) * 3;
		if (flag)
		{
			num11 += numSides * 3;
		}
		int[] array5 = new int[num11];
		int num12 = 0;
		for (int num13 = 0; num13 < numSides; num13++)
		{
			int num14 = num13 + 1;
			if (num14 == numSides)
			{
				num14 = 0;
			}
			for (int num15 = 0; num15 < numSegments + 1; num15++)
			{
				int num16 = num15 * numSides;
				array5[num12++] = num16 + num13;
				array5[num12++] = num16 + num14;
				array5[num12++] = num16 + num13 + numSides;
				array5[num12++] = num16 + num14 + numSides;
				array5[num12++] = num16 + num13 + numSides;
				array5[num12++] = num16 + num14;
			}
		}
		if (flag)
		{
			for (int num17 = 0; num17 < numSides - 1; num17++)
			{
				array5[num12++] = num;
				array5[num12++] = num + num17 + 2;
				array5[num12++] = num + num17 + 1;
			}
			array5[num12++] = num;
			array5[num12++] = num + 1;
			array5[num12++] = num + numSides;
		}
		if (!doubleSided)
		{
			mesh.triangles = array5;
		}
		else
		{
			int[] array6 = new int[array5.Length * 2];
			array5.CopyTo(array6, 0);
			for (int num18 = 0; num18 < array5.Length; num18 += 3)
			{
				array6[array5.Length + num18] = array5[num18] + num2;
				array6[array5.Length + num18 + 1] = array5[num18 + 2] + num2;
				array6[array5.Length + num18 + 2] = array5[num18 + 1] + num2;
			}
			mesh.triangles = array6;
		}
		mesh.bounds = ComputeBounds(lengthZ, radiusStart, radiusEnd);
		return mesh;
	}

	public static Mesh GenerateConeZ_Radii_DoubleCaps(float lengthZ, float radiusStart, float radiusEnd, int numSides, bool inverted)
	{
		Mesh mesh = new Mesh();
		radiusStart = Mathf.Max(radiusStart, 0.001f);
		int vertCountSides = numSides * 2;
		int num = vertCountSides;
		Func<int, int> vertSidesStartFromSlide = (int slideID) => numSides * slideID;
		Func<int, int> vertCenterFromSlide = (int slideID) => vertCountSides + slideID;
		int num2 = num + 2;
		float angleOffset = GetAngleOffset(numSides);
		Vector3[] array = new Vector3[num2];
		for (int num3 = 0; num3 < numSides; num3++)
		{
			float f = angleOffset + MathF.PI * 2f * (float)num3 / (float)numSides;
			float num4 = Mathf.Cos(f);
			float num5 = Mathf.Sin(f);
			for (int num6 = 0; num6 < 2; num6++)
			{
				float num7 = num6;
				float num8 = Mathf.Lerp(radiusStart, radiusEnd, num7);
				array[num3 + vertSidesStartFromSlide(num6)] = new Vector3(num8 * num4, num8 * num5, num7 * lengthZ);
			}
		}
		array[vertCenterFromSlide(0)] = Vector3.zero;
		array[vertCenterFromSlide(1)] = new Vector3(0f, 0f, lengthZ);
		mesh.vertices = array;
		int num9 = numSides * 2 * 3;
		num9 += numSides * 3;
		num9 += numSides * 3;
		int[] indices = new int[num9];
		int ind = 0;
		for (int num10 = 0; num10 < numSides; num10++)
		{
			int num11 = num10 + 1;
			if (num11 == numSides)
			{
				num11 = 0;
			}
			for (int num12 = 0; num12 < 1; num12++)
			{
				int num13 = num12 * numSides;
				indices[ind] = num13 + num10;
				indices[ind + (inverted ? 1 : 2)] = num13 + num11;
				indices[ind + ((!inverted) ? 1 : 2)] = num13 + num10 + numSides;
				indices[ind + 3] = num13 + num11 + numSides;
				indices[ind + (inverted ? 4 : 5)] = num13 + num10 + numSides;
				indices[ind + (inverted ? 5 : 4)] = num13 + num11;
				ind += 6;
			}
		}
		Action<int, bool> obj = delegate(int slideID, bool invert)
		{
			int num14 = vertSidesStartFromSlide(slideID);
			for (int i = 0; i < numSides - 1; i++)
			{
				indices[ind] = vertCenterFromSlide(slideID);
				indices[ind + (invert ? 1 : 2)] = num14 + i + 1;
				indices[ind + ((!invert) ? 1 : 2)] = num14 + i;
				ind += 3;
			}
			indices[ind] = vertCenterFromSlide(slideID);
			indices[ind + (invert ? 1 : 2)] = num14;
			indices[ind + ((!invert) ? 1 : 2)] = num14 + numSides - 1;
			ind += 3;
		};
		obj(0, inverted);
		obj(1, !inverted);
		mesh.triangles = indices;
		Bounds bounds = new Bounds(new Vector3(0f, 0f, lengthZ * 0.5f), new Vector3(Mathf.Max(radiusStart, radiusEnd) * 2f, Mathf.Max(radiusStart, radiusEnd) * 2f, lengthZ));
		mesh.bounds = bounds;
		return mesh;
	}

	public static Bounds ComputeBounds(float lengthZ, float radiusStart, float radiusEnd)
	{
		float num = Mathf.Max(radiusStart, radiusEnd) * 2f;
		return new Bounds(new Vector3(0f, 0f, lengthZ * 0.5f), new Vector3(num, num, lengthZ));
	}

	private static int GetCapAdditionalVerticesCount(CapMode capMode, int numSides)
	{
		return capMode switch
		{
			CapMode.None => 0, 
			CapMode.OneVertexPerCap_1Cap => 1, 
			CapMode.OneVertexPerCap_2Caps => 2, 
			CapMode.SpecificVerticesPerCap_1Cap => numSides + 1, 
			CapMode.SpecificVerticesPerCap_2Caps => 2 * (numSides + 1), 
			_ => 0, 
		};
	}

	private static int GetCapAdditionalIndicesCount(CapMode capMode, int numSides)
	{
		switch (capMode)
		{
		case CapMode.None:
			return 0;
		case CapMode.OneVertexPerCap_1Cap:
		case CapMode.SpecificVerticesPerCap_1Cap:
			return numSides * 3;
		case CapMode.OneVertexPerCap_2Caps:
		case CapMode.SpecificVerticesPerCap_2Caps:
			return 2 * (numSides * 3);
		default:
			return 0;
		}
	}

	public static int GetVertexCount(int numSides, int numSegments, CapMode capMode, bool doubleSided)
	{
		int num = numSides * (numSegments + 2);
		num += GetCapAdditionalVerticesCount(capMode, numSides);
		if (doubleSided)
		{
			num *= 2;
		}
		return num;
	}

	public static int GetIndicesCount(int numSides, int numSegments, CapMode capMode, bool doubleSided)
	{
		int num = numSides * (numSegments + 1) * 2 * 3;
		num += GetCapAdditionalIndicesCount(capMode, numSides);
		if (doubleSided)
		{
			num *= 2;
		}
		return num;
	}

	public static int GetSharedMeshVertexCount()
	{
		return GetVertexCount(Config.Instance.sharedMeshSides, Config.Instance.sharedMeshSegments, CapMode.SpecificVerticesPerCap_1Cap, Config.Instance.SD_requiresDoubleSidedMesh);
	}

	public static int GetSharedMeshIndicesCount()
	{
		return GetIndicesCount(Config.Instance.sharedMeshSides, Config.Instance.sharedMeshSegments, CapMode.SpecificVerticesPerCap_1Cap, Config.Instance.SD_requiresDoubleSidedMesh);
	}

	public static int GetSharedMeshHDVertexCount()
	{
		return GetVertexCount(Config.Instance.sharedMeshSides, 0, CapMode.OneVertexPerCap_2Caps, doubleSided: false);
	}

	public static int GetSharedMeshHDIndicesCount()
	{
		return GetIndicesCount(Config.Instance.sharedMeshSides, 0, CapMode.OneVertexPerCap_2Caps, doubleSided: false);
	}
}
