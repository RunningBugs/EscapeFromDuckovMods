using UnityEngine;
using VLB;

namespace VLB_Samples;

[RequireComponent(typeof(Camera))]
public class CameraToggleBeamVisibility : MonoBehaviour
{
	[SerializeField]
	private KeyCode m_KeyCode = KeyCode.Space;

	private void Update()
	{
		if (Input.GetKeyDown(m_KeyCode))
		{
			Camera component = GetComponent<Camera>();
			int geometryLayerID = Config.Instance.geometryLayerID;
			int num = 1 << geometryLayerID;
			if ((component.cullingMask & num) == num)
			{
				component.cullingMask &= ~num;
			}
			else
			{
				component.cullingMask |= num;
			}
		}
	}
}
