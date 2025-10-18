using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace VLB;

[ExecuteInEditMode]
[HelpURL("http://saladgamer.com/vlb-doc/comp-dynocclusion-sd-raycasting/")]
[AddComponentMenu("VLB/SD/Dynamic Occlusion (Raycasting)")]
public class DynamicOcclusionRaycasting : DynamicOcclusionAbstractBase
{
	public struct HitResult
	{
		public Vector3 point;

		public Vector3 normal;

		public float distance;

		private Collider2D collider2D;

		private Collider collider3D;

		public bool hasCollider
		{
			get
			{
				if (!collider2D)
				{
					return collider3D;
				}
				return true;
			}
		}

		public string name
		{
			get
			{
				if ((bool)collider3D)
				{
					return collider3D.name;
				}
				if ((bool)collider2D)
				{
					return collider2D.name;
				}
				return "null collider";
			}
		}

		public Bounds bounds
		{
			get
			{
				if ((bool)collider3D)
				{
					return collider3D.bounds;
				}
				if ((bool)collider2D)
				{
					return collider2D.bounds;
				}
				return default(Bounds);
			}
		}

		public HitResult(ref RaycastHit hit3D)
		{
			point = hit3D.point;
			normal = hit3D.normal;
			distance = hit3D.distance;
			collider3D = hit3D.collider;
			collider2D = null;
		}

		public HitResult(ref RaycastHit2D hit2D)
		{
			point = hit2D.point;
			normal = hit2D.normal;
			distance = hit2D.distance;
			collider2D = hit2D.collider;
			collider3D = null;
		}

		public void SetNull()
		{
			collider2D = null;
			collider3D = null;
		}
	}

	private enum Direction
	{
		Up = 0,
		Down = 1,
		Left = 2,
		Right = 3,
		Max2D = 1,
		Max3D = 3
	}

	public new const string ClassName = "DynamicOcclusionRaycasting";

	public Dimensions dimensions;

	public LayerMask layerMask = Consts.DynOcclusion.LayerMaskDefault;

	public bool considerTriggers;

	public float minOccluderArea;

	public float minSurfaceRatio = 0.5f;

	public float maxSurfaceDot = 0.25f;

	public PlaneAlignment planeAlignment;

	public float planeOffset = 0.1f;

	[FormerlySerializedAs("fadeDistanceToPlane")]
	public float fadeDistanceToSurface = 0.25f;

	private HitResult m_CurrentHit;

	private float m_RangeMultiplier = 1f;

	private uint m_PrevNonSubHitDirectionId;

	[Obsolete("Use 'fadeDistanceToSurface' instead")]
	public float fadeDistanceToPlane
	{
		get
		{
			return fadeDistanceToSurface;
		}
		set
		{
			fadeDistanceToSurface = value;
		}
	}

	public Plane planeEquationWS { get; private set; }

	private QueryTriggerInteraction queryTriggerInteraction
	{
		get
		{
			if (!considerTriggers)
			{
				return QueryTriggerInteraction.Ignore;
			}
			return QueryTriggerInteraction.Collide;
		}
	}

	private float raycastMaxDistance => m_Master.raycastDistance * m_RangeMultiplier * m_Master.GetLossyScale().z;

	public bool IsColliderHiddenByDynamicOccluder(Collider collider)
	{
		if (!planeEquationWS.IsValid())
		{
			return false;
		}
		return !GeometryUtility.TestPlanesAABB(new Plane[1] { planeEquationWS }, collider.bounds);
	}

	protected override string GetShaderKeyword()
	{
		return "VLB_OCCLUSION_CLIPPING_PLANE";
	}

	protected override MaterialManager.SD.DynamicOcclusion GetDynamicOcclusionMode()
	{
		return MaterialManager.SD.DynamicOcclusion.ClippingPlane;
	}

	protected override void OnValidateProperties()
	{
		base.OnValidateProperties();
		minOccluderArea = Mathf.Max(minOccluderArea, 0f);
		fadeDistanceToSurface = Mathf.Max(fadeDistanceToSurface, 0f);
	}

	protected override void OnEnablePostValidate()
	{
		m_CurrentHit.SetNull();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		SetHitNull();
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			TriggerZone component = GetComponent<TriggerZone>();
			if ((bool)component)
			{
				m_RangeMultiplier = Mathf.Max(1f, component.rangeMultiplier);
			}
		}
	}

	private Vector3 GetRandomVectorAround(Vector3 direction, float angleDiff)
	{
		float num = angleDiff * 0.5f;
		return Quaternion.Euler(UnityEngine.Random.Range(0f - num, num), UnityEngine.Random.Range(0f - num, num), UnityEngine.Random.Range(0f - num, num)) * direction;
	}

	private HitResult GetBestHit(Vector3 rayPos, Vector3 rayDir)
	{
		if (dimensions != Dimensions.Dim2D)
		{
			return GetBestHit3D(rayPos, rayDir);
		}
		return GetBestHit2D(rayPos, rayDir);
	}

	private HitResult GetBestHit3D(Vector3 rayPos, Vector3 rayDir)
	{
		RaycastHit[] array = Physics.RaycastAll(rayPos, rayDir, raycastMaxDistance, layerMask.value, queryTriggerInteraction);
		int num = -1;
		float num2 = float.MaxValue;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].collider.gameObject != m_Master.gameObject && array[i].collider.bounds.GetMaxArea2D() >= minOccluderArea && array[i].distance < num2)
			{
				num2 = array[i].distance;
				num = i;
			}
		}
		if (num != -1)
		{
			return new HitResult(ref array[num]);
		}
		return default(HitResult);
	}

	private HitResult GetBestHit2D(Vector3 rayPos, Vector3 rayDir)
	{
		RaycastHit2D[] array = Physics2D.RaycastAll(new Vector2(rayPos.x, rayPos.y), new Vector2(rayDir.x, rayDir.y), raycastMaxDistance, layerMask.value);
		int num = -1;
		float num2 = float.MaxValue;
		for (int i = 0; i < array.Length; i++)
		{
			if ((considerTriggers || !array[i].collider.isTrigger) && array[i].collider.gameObject != m_Master.gameObject && array[i].collider.bounds.GetMaxArea2D() >= minOccluderArea && array[i].distance < num2)
			{
				num2 = array[i].distance;
				num = i;
			}
		}
		if (num != -1)
		{
			return new HitResult(ref array[num]);
		}
		return default(HitResult);
	}

	private uint GetDirectionCount()
	{
		if (dimensions != Dimensions.Dim2D)
		{
			return 4u;
		}
		return 2u;
	}

	private Vector3 GetDirection(uint dirInt)
	{
		dirInt %= GetDirectionCount();
		return dirInt switch
		{
			0u => m_Master.raycastGlobalUp, 
			3u => m_Master.raycastGlobalRight, 
			1u => -m_Master.raycastGlobalUp, 
			2u => -m_Master.raycastGlobalRight, 
			_ => Vector3.zero, 
		};
	}

	private bool IsHitValid(ref HitResult hit, Vector3 forwardVec)
	{
		if (hit.hasCollider)
		{
			return Vector3.Dot(hit.normal, -forwardVec) >= maxSurfaceDot;
		}
		return false;
	}

	protected override bool OnProcessOcclusion(ProcessOcclusionSource source)
	{
		Vector3 raycastGlobalForward = m_Master.raycastGlobalForward;
		HitResult hit = GetBestHit(base.transform.position, raycastGlobalForward);
		if (IsHitValid(ref hit, raycastGlobalForward))
		{
			if (minSurfaceRatio > 0.5f)
			{
				float raycastDistance = m_Master.raycastDistance;
				for (uint num = 0u; num < GetDirectionCount(); num++)
				{
					Vector3 vector = GetDirection(num + m_PrevNonSubHitDirectionId) * (minSurfaceRatio * 2f - 1f);
					vector.Scale(base.transform.localScale);
					Vector3 vector2 = base.transform.position + vector * m_Master.coneRadiusStart;
					Vector3 vector3 = base.transform.position + vector * m_Master.coneRadiusEnd + raycastGlobalForward * raycastDistance;
					HitResult hit2 = GetBestHit(vector2, (vector3 - vector2).normalized);
					if (IsHitValid(ref hit2, raycastGlobalForward))
					{
						if (hit2.distance > hit.distance)
						{
							hit = hit2;
						}
						continue;
					}
					m_PrevNonSubHitDirectionId = num;
					hit.SetNull();
					break;
				}
			}
		}
		else
		{
			hit.SetNull();
		}
		SetHit(ref hit);
		return hit.hasCollider;
	}

	private void SetHit(ref HitResult hit)
	{
		if (!hit.hasCollider)
		{
			SetHitNull();
			return;
		}
		PlaneAlignment planeAlignment = this.planeAlignment;
		if (planeAlignment != PlaneAlignment.Surface && planeAlignment == PlaneAlignment.Beam)
		{
			SetClippingPlane(new Plane(-m_Master.raycastGlobalForward, hit.point));
		}
		else
		{
			SetClippingPlane(new Plane(hit.normal, hit.point));
		}
		m_CurrentHit = hit;
	}

	private void SetHitNull()
	{
		SetClippingPlaneOff();
		m_CurrentHit.SetNull();
	}

	protected override void OnModifyMaterialCallback(MaterialModifier.Interface owner)
	{
		Plane plane = planeEquationWS;
		owner.SetMaterialProp(ShaderProperties.SD.DynamicOcclusionClippingPlaneWS, new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance));
		owner.SetMaterialProp(ShaderProperties.SD.DynamicOcclusionClippingPlaneProps, fadeDistanceToSurface);
	}

	private void SetClippingPlane(Plane planeWS)
	{
		planeWS = planeWS.TranslateCustom(planeWS.normal * planeOffset);
		SetPlaneWS(planeWS);
		m_Master._INTERNAL_SetDynamicOcclusionCallback(GetShaderKeyword(), m_MaterialModifierCallbackCached);
	}

	private void SetClippingPlaneOff()
	{
		SetPlaneWS(default(Plane));
		m_Master._INTERNAL_SetDynamicOcclusionCallback(GetShaderKeyword(), null);
	}

	private void SetPlaneWS(Plane planeWS)
	{
		planeEquationWS = planeWS;
	}
}
