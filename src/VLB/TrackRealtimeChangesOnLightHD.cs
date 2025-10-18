using UnityEngine;

namespace VLB;

[DisallowMultipleComponent]
[RequireComponent(typeof(Light), typeof(VolumetricLightBeamHD))]
[HelpURL("http://saladgamer.com/vlb-doc/comp-trackrealtimechanges-hd/")]
[AddComponentMenu("VLB/HD/Track Realtime Changes On Light")]
public class TrackRealtimeChangesOnLightHD : MonoBehaviour
{
	public const string ClassName = "TrackRealtimeChangesOnLightHD";

	private VolumetricLightBeamHD m_Master;

	private void Awake()
	{
		m_Master = GetComponent<VolumetricLightBeamHD>();
	}

	private void Update()
	{
		if (m_Master.enabled)
		{
			m_Master.AssignPropertiesFromAttachedSpotLight();
		}
	}
}
