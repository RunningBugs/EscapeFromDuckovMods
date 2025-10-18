using Cinemachine.Utility;
using UnityEngine;

namespace Duckov.MiniMaps;

public class MiniMapCompass : MonoBehaviour
{
	[SerializeField]
	private Transform arrow;

	private void SetupRotation()
	{
		Vector3 vector = LevelManager.Instance.GameCamera.mainVCam.transform.up.ProjectOntoPlane(Vector3.up);
		Vector3 forward = Vector3.forward;
		float num = Vector3.SignedAngle(vector, forward, Vector3.up);
		arrow.localRotation = Quaternion.Euler(0f, 0f, 0f - num);
	}

	private void Update()
	{
		SetupRotation();
	}
}
