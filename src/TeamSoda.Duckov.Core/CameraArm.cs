using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraArm : MonoBehaviour
{
	public bool volumeInfluence;

	public float pitch;

	public float yaw;

	public float distance;

	public static float globalPitch = 55f;

	public static float globalYaw = -30f;

	public static float globalDistance = -45f;

	public Transform pitchRoot;

	public Transform yawRoot;

	public CinemachineVirtualCamera virtualCamera;

	public GameCamera gameCamera;

	private static bool topDownView = false;

	public Volume topDownViewVolume;

	private void Update()
	{
		if (volumeInfluence)
		{
			pitch = Mathf.Lerp(pitch, globalPitch, 5f * Time.deltaTime);
			yaw = Mathf.Lerp(yaw, globalYaw, 2f * Time.deltaTime);
			distance = Mathf.Lerp(distance, globalDistance, 2f * Time.deltaTime);
		}
		UpdateArm();
		if (topDownView != topDownViewVolume.enabled)
		{
			topDownViewVolume.enabled = topDownView;
		}
	}

	public static void ToggleView()
	{
		topDownView = !topDownView;
	}

	private void UpdateArm()
	{
		pitchRoot.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
		virtualCamera.transform.localPosition = new Vector3(0f, 0f, 0f - distance);
		virtualCamera.transform.localRotation = Quaternion.identity;
	}

	private void OnValidate()
	{
		UpdateArm();
	}
}
