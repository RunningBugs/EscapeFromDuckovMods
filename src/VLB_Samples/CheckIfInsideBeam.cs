using UnityEngine;
using VLB;

namespace VLB_Samples;

[RequireComponent(typeof(Collider), typeof(Rigidbody), typeof(MeshRenderer))]
public class CheckIfInsideBeam : MonoBehaviour
{
	private bool isInsideBeam;

	private Material m_Material;

	private Collider m_Collider;

	private void Start()
	{
		m_Collider = GetComponent<Collider>();
		MeshRenderer component = GetComponent<MeshRenderer>();
		if ((bool)component)
		{
			m_Material = component.material;
		}
	}

	private void Update()
	{
		if ((bool)m_Material)
		{
			m_Material.SetColor("_Color", isInsideBeam ? Color.green : Color.red);
		}
	}

	private void FixedUpdate()
	{
		isInsideBeam = false;
	}

	private void OnTriggerStay(Collider trigger)
	{
		DynamicOcclusionRaycasting component = trigger.GetComponent<DynamicOcclusionRaycasting>();
		if ((bool)component)
		{
			isInsideBeam = !component.IsColliderHiddenByDynamicOccluder(m_Collider);
		}
		else
		{
			isInsideBeam = true;
		}
	}
}
