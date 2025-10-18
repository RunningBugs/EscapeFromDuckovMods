using System;
using UnityEngine;

namespace VLB;

public static class Utils
{
	public enum FloatPackingPrecision
	{
		High = 64,
		Low = 8,
		Undef = 0
	}

	private const float kEpsilon = 1E-05f;

	private static FloatPackingPrecision ms_FloatPackingPrecision;

	private const int kFloatPackingHighMinShaderLevel = 35;

	public static float ComputeConeRadiusEnd(float fallOffEnd, float spotAngle)
	{
		return fallOffEnd * Mathf.Tan(spotAngle * (MathF.PI / 180f) * 0.5f);
	}

	public static float ComputeSpotAngle(float fallOffEnd, float coneRadiusEnd)
	{
		return Mathf.Atan2(coneRadiusEnd, fallOffEnd) * 57.29578f * 2f;
	}

	public static void Swap<T>(ref T a, ref T b)
	{
		T val = a;
		a = b;
		b = val;
	}

	public static void ResizeArray<T>(ref T[] array, int newSize)
	{
		Array.Resize(ref array, newSize);
	}

	public static bool IsValidIndex<T>(this T[] array, int idx)
	{
		if (idx >= 0)
		{
			return idx < array.Length;
		}
		return false;
	}

	public static string GetPath(Transform current)
	{
		if (current.parent == null)
		{
			return "/" + current.name;
		}
		return GetPath(current.parent) + "/" + current.name;
	}

	public static T NewWithComponent<T>(string name) where T : Component
	{
		return new GameObject(name, typeof(T)).GetComponent<T>();
	}

	public static T GetOrAddComponent<T>(this GameObject self) where T : Component
	{
		T val = self.GetComponent<T>();
		if (val == null)
		{
			val = self.AddComponent<T>();
		}
		return val;
	}

	public static T GetOrAddComponent<T>(this MonoBehaviour self) where T : Component
	{
		return self.gameObject.GetOrAddComponent<T>();
	}

	public static void ForeachComponentsInAnyChildrenOnly<T>(this GameObject self, Action<T> lambda, bool includeInactive = false) where T : Component
	{
		T[] componentsInChildren = self.GetComponentsInChildren<T>(includeInactive);
		foreach (T val in componentsInChildren)
		{
			if (val.gameObject != self)
			{
				lambda(val);
			}
		}
	}

	public static void ForeachComponentsInDirectChildrenOnly<T>(this GameObject self, Action<T> lambda, bool includeInactive = false) where T : Component
	{
		T[] componentsInChildren = self.GetComponentsInChildren<T>(includeInactive);
		foreach (T val in componentsInChildren)
		{
			if (val.transform.parent == self.transform)
			{
				lambda(val);
			}
		}
	}

	public static void SetupDepthCamera(Camera depthCamera, float coneApexOffsetZ, float maxGeometryDistance, float coneRadiusStart, float coneRadiusEnd, Vector3 beamLocalForward, Vector3 lossyScale, bool isScalable, Quaternion beamInternalLocalRotation, bool shouldScaleMinNearClipPlane)
	{
		if (!isScalable)
		{
			lossyScale.x = (lossyScale.y = 1f);
		}
		float num = coneApexOffsetZ;
		bool flag = num >= 0f;
		num = Mathf.Max(num, 0f);
		depthCamera.orthographic = !flag;
		depthCamera.transform.localPosition = beamLocalForward * (0f - num);
		Quaternion localRotation = beamInternalLocalRotation;
		if (Mathf.Sign(lossyScale.z) < 0f)
		{
			localRotation *= Quaternion.Euler(0f, 180f, 0f);
		}
		depthCamera.transform.localRotation = localRotation;
		if (!Mathf.Approximately(lossyScale.y * lossyScale.z, 0f))
		{
			float num2 = (flag ? 0.1f : 0f);
			float num3 = Mathf.Abs(lossyScale.z);
			depthCamera.nearClipPlane = Mathf.Max(num * num3, num2 * (shouldScaleMinNearClipPlane ? num3 : 1f));
			depthCamera.farClipPlane = (maxGeometryDistance + num * (isScalable ? 1f : num3)) * (isScalable ? num3 : 1f);
			depthCamera.aspect = Mathf.Abs(lossyScale.x / lossyScale.y);
			if (flag)
			{
				float fieldOfView = Mathf.Atan2(coneRadiusEnd * Mathf.Abs(lossyScale.y), depthCamera.farClipPlane) * 57.29578f * 2f;
				depthCamera.fieldOfView = fieldOfView;
			}
			else
			{
				depthCamera.orthographicSize = coneRadiusStart * lossyScale.y;
			}
		}
	}

	public static bool HasFlag(this Enum mask, Enum flags)
	{
		return ((int)(object)mask & (int)(object)flags) == (int)(object)flags;
	}

	public static Vector3 Divide(this Vector3 aVector, Vector3 scale)
	{
		if (Mathf.Approximately(scale.x * scale.y * scale.z, 0f))
		{
			return Vector3.zero;
		}
		return new Vector3(aVector.x / scale.x, aVector.y / scale.y, aVector.z / scale.z);
	}

	public static Vector2 xy(this Vector3 aVector)
	{
		return new Vector2(aVector.x, aVector.y);
	}

	public static Vector2 xz(this Vector3 aVector)
	{
		return new Vector2(aVector.x, aVector.z);
	}

	public static Vector2 yz(this Vector3 aVector)
	{
		return new Vector2(aVector.y, aVector.z);
	}

	public static Vector2 yx(this Vector3 aVector)
	{
		return new Vector2(aVector.y, aVector.x);
	}

	public static Vector2 zx(this Vector3 aVector)
	{
		return new Vector2(aVector.z, aVector.x);
	}

	public static Vector2 zy(this Vector3 aVector)
	{
		return new Vector2(aVector.z, aVector.y);
	}

	public static bool Approximately(this float a, float b, float epsilon = 1E-05f)
	{
		return Mathf.Abs(a - b) < epsilon;
	}

	public static bool Approximately(this Vector2 a, Vector2 b, float epsilon = 1E-05f)
	{
		return Vector2.SqrMagnitude(a - b) < epsilon;
	}

	public static bool Approximately(this Vector3 a, Vector3 b, float epsilon = 1E-05f)
	{
		return Vector3.SqrMagnitude(a - b) < epsilon;
	}

	public static bool Approximately(this Vector4 a, Vector4 b, float epsilon = 1E-05f)
	{
		return Vector4.SqrMagnitude(a - b) < epsilon;
	}

	public static Vector4 AsVector4(this Vector3 vec3, float w)
	{
		return new Vector4(vec3.x, vec3.y, vec3.z, w);
	}

	public static Vector4 PlaneEquation(Vector3 normalizedNormal, Vector3 pt)
	{
		return normalizedNormal.AsVector4(0f - Vector3.Dot(normalizedNormal, pt));
	}

	public static float GetVolumeCubic(this Bounds self)
	{
		return self.size.x * self.size.y * self.size.z;
	}

	public static float GetMaxArea2D(this Bounds self)
	{
		return Mathf.Max(Mathf.Max(self.size.x * self.size.y, self.size.y * self.size.z), self.size.x * self.size.z);
	}

	public static Color Opaque(this Color self)
	{
		return new Color(self.r, self.g, self.b, 1f);
	}

	public static Color ComputeComplementaryColor(this Color self, bool blackAndWhite)
	{
		if (blackAndWhite)
		{
			if (!((double)self.r * 0.299 + (double)self.g * 0.587 + (double)self.b * 0.114 > 0.729411780834198))
			{
				return Color.white;
			}
			return Color.black;
		}
		return new Color(1f - self.r, 1f - self.g, 1f - self.b);
	}

	public static Plane TranslateCustom(this Plane plane, Vector3 translation)
	{
		plane.distance += Vector3.Dot(translation.normalized, plane.normal) * translation.magnitude;
		return plane;
	}

	public static Vector3 ClosestPointOnPlaneCustom(this Plane plane, Vector3 point)
	{
		return point - plane.GetDistanceToPoint(point) * plane.normal;
	}

	public static bool IsAlmostZero(float f)
	{
		return Mathf.Abs(f) < 0.001f;
	}

	public static bool IsValid(this Plane plane)
	{
		return plane.normal.sqrMagnitude > 0.5f;
	}

	public static void SetKeywordEnabled(this Material mat, string name, bool enabled)
	{
		if (enabled)
		{
			mat.EnableKeyword(name);
		}
		else
		{
			mat.DisableKeyword(name);
		}
	}

	public static void SetShaderKeywordEnabled(string name, bool enabled)
	{
		if (enabled)
		{
			Shader.EnableKeyword(name);
		}
		else
		{
			Shader.DisableKeyword(name);
		}
	}

	public static Matrix4x4 SampleInMatrix(this Gradient self, int floatPackingPrecision)
	{
		Matrix4x4 result = default(Matrix4x4);
		for (int i = 0; i < 16; i++)
		{
			Color color = self.Evaluate(Mathf.Clamp01((float)i / 15f));
			result[i] = color.PackToFloat(floatPackingPrecision);
		}
		return result;
	}

	public static Color[] SampleInArray(this Gradient self, int samplesCount)
	{
		Color[] array = new Color[samplesCount];
		for (int i = 0; i < samplesCount; i++)
		{
			array[i] = self.Evaluate(Mathf.Clamp01((float)i / (float)(samplesCount - 1)));
		}
		return array;
	}

	private static Vector4 Vector4_Floor(Vector4 vec)
	{
		return new Vector4(Mathf.Floor(vec.x), Mathf.Floor(vec.y), Mathf.Floor(vec.z), Mathf.Floor(vec.w));
	}

	public static float PackToFloat(this Color color, int floatPackingPrecision)
	{
		Vector4 vector = Vector4_Floor(color * (floatPackingPrecision - 1));
		return 0f + vector.x * (float)floatPackingPrecision * (float)floatPackingPrecision * (float)floatPackingPrecision + vector.y * (float)floatPackingPrecision * (float)floatPackingPrecision + vector.z * (float)floatPackingPrecision + vector.w;
	}

	public static FloatPackingPrecision GetFloatPackingPrecision()
	{
		if (ms_FloatPackingPrecision == FloatPackingPrecision.Undef)
		{
			ms_FloatPackingPrecision = ((SystemInfo.graphicsShaderLevel >= 35) ? FloatPackingPrecision.High : FloatPackingPrecision.Low);
		}
		return ms_FloatPackingPrecision;
	}

	public static bool HasAtLeastOneFlag(this Enum mask, Enum flags)
	{
		return ((int)(object)mask & (int)(object)flags) != 0;
	}

	public static void MarkCurrentSceneDirty()
	{
	}

	public static void MarkObjectDirty(UnityEngine.Object obj)
	{
	}
}
