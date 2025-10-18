using System;
using System.Collections.Generic;
using System.Linq;
using JoshH.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace JoshH.UI;

[AddComponentMenu("UI/Effects/UI Gradient")]
[RequireComponent(typeof(RectTransform))]
public class UIGradient : BaseMeshEffect
{
	public enum UIGradientBlendMode
	{
		Override,
		Multiply
	}

	public enum UIGradientType
	{
		Linear,
		Corner,
		ComplexLinear
	}

	[Tooltip("How the gradient color will be blended with the graphics color.")]
	[SerializeField]
	private UIGradientBlendMode blendMode;

	[SerializeField]
	[Range(0f, 1f)]
	private float intensity = 1f;

	[SerializeField]
	private UIGradientType gradientType;

	[SerializeField]
	private Color linearColor1 = Color.yellow;

	[SerializeField]
	private Color linearColor2 = Color.red;

	[SerializeField]
	private Color cornerColorUpperLeft = Color.red;

	[SerializeField]
	private Color cornerColorUpperRight = Color.yellow;

	[SerializeField]
	private Color cornerColorLowerRight = Color.green;

	[SerializeField]
	private Color cornerColorLowerLeft = Color.blue;

	[SerializeField]
	private Gradient linearGradient;

	[SerializeField]
	[Range(0f, 360f)]
	private float angle;

	private RectTransform _rectTransform;

	protected RectTransform rectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = base.transform as RectTransform;
			}
			return _rectTransform;
		}
	}

	public UIGradientBlendMode BlendMode
	{
		get
		{
			return blendMode;
		}
		set
		{
			blendMode = value;
			ForceUpdateGraphic();
		}
	}

	public float Intensity
	{
		get
		{
			return intensity;
		}
		set
		{
			intensity = Mathf.Clamp01(value);
			ForceUpdateGraphic();
		}
	}

	public UIGradientType GradientType
	{
		get
		{
			return gradientType;
		}
		set
		{
			gradientType = value;
			ForceUpdateGraphic();
		}
	}

	public Color LinearColor1
	{
		get
		{
			return linearColor1;
		}
		set
		{
			linearColor1 = value;
			ForceUpdateGraphic();
		}
	}

	public Color LinearColor2
	{
		get
		{
			return linearColor2;
		}
		set
		{
			linearColor2 = value;
			ForceUpdateGraphic();
		}
	}

	public Color CornerColorUpperLeft
	{
		get
		{
			return cornerColorUpperLeft;
		}
		set
		{
			cornerColorUpperLeft = value;
			ForceUpdateGraphic();
		}
	}

	public Color CornerColorUpperRight
	{
		get
		{
			return cornerColorUpperRight;
		}
		set
		{
			cornerColorUpperRight = value;
			ForceUpdateGraphic();
		}
	}

	public Color CornerColorLowerRight
	{
		get
		{
			return cornerColorLowerRight;
		}
		set
		{
			cornerColorLowerRight = value;
			ForceUpdateGraphic();
		}
	}

	public Color CornerColorLowerLeft
	{
		get
		{
			return cornerColorLowerLeft;
		}
		set
		{
			cornerColorLowerLeft = value;
			ForceUpdateGraphic();
		}
	}

	public float Angle
	{
		get
		{
			return angle;
		}
		set
		{
			if (value < 0f)
			{
				angle = value % 360f + 360f;
			}
			else
			{
				angle = value % 360f;
			}
			ForceUpdateGraphic();
		}
	}

	public Gradient LinearGradient
	{
		get
		{
			return linearGradient;
		}
		set
		{
			linearGradient = value;
			ForceUpdateGraphic();
		}
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		if (!base.enabled)
		{
			return;
		}
		UIVertex vertex = default(UIVertex);
		if (gradientType == UIGradientType.ComplexLinear)
		{
			CutMesh(vh);
		}
		for (int i = 0; i < vh.currentVertCount; i++)
		{
			vh.PopulateUIVertex(ref vertex, i);
			Vector2 normalizedPosition = ((Vector2)vertex.position - rectTransform.rect.min) / (rectTransform.rect.max - rectTransform.rect.min);
			normalizedPosition = RotateNormalizedPosition(normalizedPosition, angle);
			Color c = Color.black;
			if (gradientType == UIGradientType.Linear)
			{
				c = GetColorInGradient(linearColor1, linearColor1, linearColor2, linearColor2, normalizedPosition);
			}
			else if (gradientType == UIGradientType.Corner)
			{
				c = GetColorInGradient(cornerColorUpperLeft, cornerColorUpperRight, cornerColorLowerRight, cornerColorLowerLeft, normalizedPosition);
			}
			else if (gradientType == UIGradientType.ComplexLinear)
			{
				c = linearGradient.Evaluate(normalizedPosition.y);
			}
			vertex.color = BlendColor(vertex.color, c, blendMode, intensity);
			vh.SetUIVertex(vertex, i);
		}
	}

	protected void CutMesh(VertexHelper vh)
	{
		List<UIVertex> list = new List<UIVertex>();
		vh.GetUIVertexStream(list);
		vh.Clear();
		List<UIVertex> list2 = new List<UIVertex>();
		Vector2 cutDirection = GetCutDirection();
		foreach (float item in linearGradient.alphaKeys.Select((GradientAlphaKey x) => x.time).Union(linearGradient.colorKeys.Select((GradientColorKey x) => x.time)))
		{
			list2.Clear();
			Vector2 cutOrigin = GetCutOrigin(item);
			if (!((double)item < 0.001) && !((double)item > 0.999))
			{
				for (int num = 0; num < list.Count; num += 3)
				{
					CutTriangle(list, num, list2, cutDirection, cutOrigin);
				}
				list.Clear();
				list.AddRange(list2);
			}
		}
		vh.AddUIVertexTriangleStream(list);
	}

	private UIVertex UIVertexLerp(UIVertex v1, UIVertex v2, float f)
	{
		return new UIVertex
		{
			position = Vector3.Lerp(v1.position, v2.position, f),
			color = Color.Lerp(v1.color, v2.color, f),
			uv0 = Vector2.Lerp(v1.uv0, v2.uv0, f),
			uv1 = Vector2.Lerp(v1.uv1, v2.uv1, f),
			uv2 = Vector2.Lerp(v1.uv2, v2.uv2, f),
			uv3 = Vector2.Lerp(v1.uv3, v2.uv3, f)
		};
	}

	private Vector2 GetCutDirection()
	{
		Vector2 v = Vector2.up.Rotate(0f - angle);
		v = new Vector2(v.x / rectTransform.rect.size.x, v.y / rectTransform.rect.size.y);
		return v.Rotate(90f);
	}

	private void CutTriangle(List<UIVertex> tris, int idx, List<UIVertex> list, Vector2 cutDirection, Vector2 point)
	{
		UIVertex uIVertex = tris[idx];
		UIVertex uIVertex2 = tris[idx + 1];
		UIVertex uIVertex3 = tris[idx + 2];
		float f = OnLine(uIVertex2.position, uIVertex3.position, point, cutDirection);
		float f2 = OnLine(uIVertex.position, uIVertex2.position, point, cutDirection);
		float f3 = OnLine(uIVertex3.position, uIVertex.position, point, cutDirection);
		if (IsOnLine(f2))
		{
			if (IsOnLine(f))
			{
				UIVertex item = UIVertexLerp(uIVertex, uIVertex2, f2);
				UIVertex item2 = UIVertexLerp(uIVertex2, uIVertex3, f);
				list.AddRange(new List<UIVertex> { uIVertex, item, uIVertex3, item, item2, uIVertex3, item, uIVertex2, item2 });
			}
			else
			{
				UIVertex item3 = UIVertexLerp(uIVertex, uIVertex2, f2);
				UIVertex item4 = UIVertexLerp(uIVertex3, uIVertex, f3);
				list.AddRange(new List<UIVertex> { uIVertex3, item4, uIVertex2, item4, item3, uIVertex2, item4, uIVertex, item3 });
			}
		}
		else if (IsOnLine(f))
		{
			UIVertex item5 = UIVertexLerp(uIVertex2, uIVertex3, f);
			UIVertex item6 = UIVertexLerp(uIVertex3, uIVertex, f3);
			list.AddRange(new List<UIVertex> { uIVertex2, item5, uIVertex, item5, item6, uIVertex, item5, uIVertex3, item6 });
		}
		else
		{
			list.AddRange(tris.GetRange(idx, 3));
		}
	}

	private bool IsOnLine(float f)
	{
		if (f <= 1f)
		{
			return f > 0f;
		}
		return false;
	}

	private float OnLine(Vector2 p1, Vector2 p2, Vector2 o, Vector2 dir)
	{
		float num = (p2.x - p1.x) * dir.y - (p2.y - p1.y) * dir.x;
		if (num == 0f)
		{
			return -1f;
		}
		return ((o.x - p1.x) * dir.y - (o.y - p1.y) * dir.x) / num;
	}

	private float ProjectedDistance(Vector2 p1, Vector2 p2, Vector2 normal)
	{
		return Vector2.Distance(Vector3.Project(p1, normal), Vector3.Project(p2, normal));
	}

	private Vector2 GetCutOrigin(float f)
	{
		Vector2 vector = Vector2.up.Rotate(0f - angle);
		vector = new Vector2(vector.x / rectTransform.rect.size.x, vector.y / rectTransform.rect.size.y);
		Vector3 vector2;
		Vector3 vector3;
		if (angle % 180f < 90f)
		{
			vector2 = Vector3.Project(Vector2.Scale(rectTransform.rect.size, Vector2.down + Vector2.left) * 0.5f, vector);
			vector3 = Vector3.Project(Vector2.Scale(rectTransform.rect.size, Vector2.up + Vector2.right) * 0.5f, vector);
		}
		else
		{
			vector2 = Vector3.Project(Vector2.Scale(rectTransform.rect.size, Vector2.up + Vector2.left) * 0.5f, vector);
			vector3 = Vector3.Project(Vector2.Scale(rectTransform.rect.size, Vector2.down + Vector2.right) * 0.5f, vector);
		}
		if (angle % 360f >= 180f)
		{
			return Vector2.Lerp(vector3, vector2, f) + rectTransform.rect.center;
		}
		return Vector2.Lerp(vector2, vector3, f) + rectTransform.rect.center;
	}

	private Color BlendColor(Color c1, Color c2, UIGradientBlendMode mode, float intensity)
	{
		switch (mode)
		{
		case UIGradientBlendMode.Override:
			return Color.Lerp(c1, c2, intensity);
		case UIGradientBlendMode.Multiply:
			return Color.Lerp(c1, c1 * c2, intensity);
		default:
			Debug.LogErrorFormat("Mode is not supported: {0}", mode);
			return c1;
		}
	}

	private Vector2 RotateNormalizedPosition(Vector2 normalizedPosition, float angle)
	{
		float f = MathF.PI / 180f * ((angle < 0f) ? (angle % 90f + 90f) : (angle % 90f));
		float num = Mathf.Sin(f) + Mathf.Cos(f);
		return (normalizedPosition - Vector2.one * 0.5f).Rotate(angle) / num + Vector2.one * 0.5f;
	}

	public void ForceUpdateGraphic()
	{
		if (base.graphic != null)
		{
			base.graphic.SetVerticesDirty();
		}
	}

	private Color GetColorInGradient(Color ul, Color ur, Color lr, Color ll, Vector2 normalizedPosition)
	{
		return Color.Lerp(Color.Lerp(ll, lr, normalizedPosition.x), Color.Lerp(ul, ur, normalizedPosition.x), normalizedPosition.y);
	}
}
