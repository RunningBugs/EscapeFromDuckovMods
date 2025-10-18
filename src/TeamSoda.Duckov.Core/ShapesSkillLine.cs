using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class ShapesSkillLine : MonoBehaviour
{
	public Vector3[] points;

	public Color[] colors;

	public Vector3 hitPoint;

	public bool hitObsticle;

	public ShapesBlendMode blendMode;

	public bool worldSpace;

	public float dotRadius = 0.02f;

	public float lineThickness = 0.02f;

	private Camera cam;

	private void Awake()
	{
	}

	public void DrawLine()
	{
		if (!cam)
		{
			if ((bool)LevelManager.Instance)
			{
				cam = LevelManager.Instance.GameCamera.renderCamera;
			}
			if (!cam)
			{
				return;
			}
		}
		if (points.Length == 0)
		{
			return;
		}
		using (Draw.Command(cam))
		{
			Draw.LineGeometry = LineGeometry.Billboard;
			Draw.BlendMode = blendMode;
			Draw.ThicknessSpace = ThicknessSpace.Meters;
			Draw.Thickness = lineThickness;
			Draw.ZTest = CompareFunction.Always;
			if (!worldSpace)
			{
				Draw.Matrix = base.transform.localToWorldMatrix;
			}
			for (int i = 0; i < points.Length - 1; i++)
			{
				Draw.Sphere(points[i], dotRadius, colors[i]);
				Draw.Line(points[i], points[i + 1], colors[i]);
			}
			Draw.Sphere(points[points.Length - 1], dotRadius, colors[colors.Length - 1]);
			if (hitObsticle)
			{
				Draw.Sphere(hitPoint, dotRadius, colors[0]);
			}
		}
	}
}
