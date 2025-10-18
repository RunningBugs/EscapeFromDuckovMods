using Duckov.Utilities;
using UnityEngine;

public class SkillProjectileLineHUD : MonoBehaviour
{
	public ShapesSkillLine line;

	public int fragmentCount = 20;

	[ColorUsage(true, true)]
	public Color lineColor;

	[ColorUsage(true, true)]
	public Color obsticleColor;

	private LayerMask obsticleLayers;

	private RaycastHit[] hits;

	private void Awake()
	{
		obsticleLayers = (int)GameplayDataSettings.Layers.wallLayerMask | (int)GameplayDataSettings.Layers.groundLayerMask | (int)GameplayDataSettings.Layers.fowBlockLayers;
	}

	public bool UpdateLine(Vector3 start, Vector3 target, float verticleSpeed, ref Vector3 hitPoint)
	{
		float magnitude = Physics.gravity.magnitude;
		if (line.points.Length != fragmentCount + 1)
		{
			line.points = new Vector3[fragmentCount + 1];
			line.colors = new Color[fragmentCount + 1];
		}
		float num = verticleSpeed / magnitude;
		float num2 = Mathf.Sqrt(2f * (num * verticleSpeed * 0.5f + start.y - target.y) / magnitude);
		float num3 = num + num2;
		Vector3 vector = start;
		vector.y = 0f;
		Vector3 vector2 = target;
		vector2.y = 0f;
		float num4 = Vector3.Distance(vector, vector2);
		float num5 = 0f;
		Vector3 vector3 = vector2 - vector;
		if (vector3.magnitude > 0f)
		{
			vector3 = vector3.normalized;
			num5 = num4 / num3;
		}
		else
		{
			vector3 = Vector3.zero;
		}
		float num6 = num3 / (float)fragmentCount;
		bool flag = false;
		for (int i = 0; i < fragmentCount + 1; i++)
		{
			float num7 = num6 * (float)i;
			line.points[i] = start + Vector3.up * (verticleSpeed - magnitude * num7 * 0.5f) * num7 + vector3 * num5 * num7;
			Vector3 vector4 = line.points[i];
			if (i > 0 && i < line.points.Length - 1 && !flag)
			{
				Vector3 vector5 = line.points[i - 1];
				flag = CheckObsticle(vector5, vector4, ref hitPoint);
				hitPoint = vector5 + (vector4 - vector5).normalized * (hitPoint - vector5).magnitude;
			}
			if (flag)
			{
				line.colors[i] = obsticleColor;
			}
			else
			{
				line.colors[i] = lineColor;
			}
		}
		line.hitObsticle = flag;
		if (flag)
		{
			line.hitPoint = hitPoint;
		}
		line.DrawLine();
		return flag;
	}

	private bool CheckObsticle(Vector3 from, Vector3 to, ref Vector3 hitPoint)
	{
		if (hits == null)
		{
			hits = new RaycastHit[3];
		}
		if (Physics.SphereCastNonAlloc(from, 0.2f, (to - from).normalized, hits, (to - from).magnitude, obsticleLayers) > 0)
		{
			hitPoint = hits[0].point;
			return true;
		}
		return false;
	}
}
