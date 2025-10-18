using UnityEngine;

namespace VLB;

[DisallowMultipleComponent]
[RequireComponent(typeof(VolumetricLightBeamAbstractBase))]
[HelpURL("http://saladgamer.com/vlb-doc/comp-triggerzone/")]
[AddComponentMenu("VLB/Common/Trigger Zone")]
public class TriggerZone : MonoBehaviour
{
	private enum TriggerZoneUpdateRate
	{
		OnEnable,
		OnOcclusionChange
	}

	public const string ClassName = "TriggerZone";

	public bool setIsTrigger = true;

	public float rangeMultiplier = 1f;

	private const int kMeshColliderNumSides = 8;

	private VolumetricLightBeamAbstractBase m_Beam;

	private DynamicOcclusionRaycasting m_DynamicOcclusionRaycasting;

	private PolygonCollider2D m_PolygonCollider2D;

	private TriggerZoneUpdateRate updateRate
	{
		get
		{
			if (UtilsBeamProps.GetDimensions(m_Beam) == Dimensions.Dim3D)
			{
				return TriggerZoneUpdateRate.OnEnable;
			}
			if (!(m_DynamicOcclusionRaycasting != null))
			{
				return TriggerZoneUpdateRate.OnEnable;
			}
			return TriggerZoneUpdateRate.OnOcclusionChange;
		}
	}

	private void OnEnable()
	{
		m_Beam = GetComponent<VolumetricLightBeamAbstractBase>();
		m_DynamicOcclusionRaycasting = GetComponent<DynamicOcclusionRaycasting>();
		switch (updateRate)
		{
		case TriggerZoneUpdateRate.OnEnable:
			ComputeZone();
			base.enabled = false;
			break;
		case TriggerZoneUpdateRate.OnOcclusionChange:
			if ((bool)m_DynamicOcclusionRaycasting)
			{
				m_DynamicOcclusionRaycasting.onOcclusionProcessed += OnOcclusionProcessed;
			}
			break;
		}
	}

	private void OnOcclusionProcessed()
	{
		ComputeZone();
	}

	private void ComputeZone()
	{
		if (!m_Beam)
		{
			return;
		}
		float coneRadiusStart = UtilsBeamProps.GetConeRadiusStart(m_Beam);
		float num = UtilsBeamProps.GetFallOffEnd(m_Beam) * rangeMultiplier;
		float num2 = Mathf.LerpUnclamped(coneRadiusStart, UtilsBeamProps.GetConeRadiusEnd(m_Beam), rangeMultiplier);
		if (UtilsBeamProps.GetDimensions(m_Beam) == Dimensions.Dim3D)
		{
			MeshCollider orAddComponent = base.gameObject.GetOrAddComponent<MeshCollider>();
			Mathf.Min(UtilsBeamProps.GetGeomSides(m_Beam), 8);
			Mesh mesh = MeshGenerator.GenerateConeZ_Radii_DoubleCaps(num, coneRadiusStart, num2, 8, inverted: false);
			mesh.hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
			orAddComponent.sharedMesh = mesh;
			orAddComponent.convex = setIsTrigger;
			orAddComponent.isTrigger = setIsTrigger;
			return;
		}
		if (m_PolygonCollider2D == null)
		{
			m_PolygonCollider2D = base.gameObject.GetOrAddComponent<PolygonCollider2D>();
		}
		Vector2[] array = new Vector2[4]
		{
			new Vector2(0f, 0f - coneRadiusStart),
			new Vector2(num, 0f - num2),
			new Vector2(num, num2),
			new Vector2(0f, coneRadiusStart)
		};
		if ((bool)m_DynamicOcclusionRaycasting && m_DynamicOcclusionRaycasting.planeEquationWS.IsValid())
		{
			Plane planeEquationWS = m_DynamicOcclusionRaycasting.planeEquationWS;
			if (Utils.IsAlmostZero(planeEquationWS.normal.z))
			{
				Vector3 vector = planeEquationWS.ClosestPointOnPlaneCustom(Vector3.zero);
				Vector3 vector2 = planeEquationWS.ClosestPointOnPlaneCustom(Vector3.up);
				if (Utils.IsAlmostZero(Vector3.SqrMagnitude(vector - vector2)))
				{
					vector = planeEquationWS.ClosestPointOnPlaneCustom(Vector3.right);
				}
				vector = base.transform.InverseTransformPoint(vector);
				vector2 = base.transform.InverseTransformPoint(vector2);
				PolygonHelper.Plane2D plane2D = PolygonHelper.Plane2D.FromPoints(vector, vector2);
				if (plane2D.normal.x > 0f)
				{
					plane2D.Flip();
				}
				array = plane2D.CutConvex(array);
			}
		}
		m_PolygonCollider2D.points = array;
		m_PolygonCollider2D.isTrigger = setIsTrigger;
	}
}
