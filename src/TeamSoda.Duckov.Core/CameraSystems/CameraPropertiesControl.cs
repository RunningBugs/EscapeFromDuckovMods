using Cinemachine;
using Cinemachine.PostFX;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace CameraSystems;

public class CameraPropertiesControl : MonoBehaviour
{
	private CinemachineVirtualCamera vCam;

	private CinemachineVolumeSettings volumeSettings;

	[SerializeField]
	private VolumeProfile volumeProfile;

	private void Awake()
	{
		vCam = GetComponent<CinemachineVirtualCamera>();
		volumeSettings = GetComponent<CinemachineVolumeSettings>();
	}

	private void Update()
	{
		_ = Gamepad.current.dpad.x.value;
		_ = 0f;
		if (Gamepad.current.dpad.y.value != 0f)
		{
			float num = 0f - Gamepad.current.dpad.y.value;
			if (Gamepad.current.rightShoulder.value > 0f)
			{
				num *= 10f;
			}
			vCam.m_Lens.FieldOfView = Mathf.Clamp(vCam.m_Lens.FieldOfView + num * 5f * Time.deltaTime, 8f, 100f);
		}
	}
}
