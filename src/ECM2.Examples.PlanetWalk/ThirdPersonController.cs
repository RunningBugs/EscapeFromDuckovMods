using ECM2.Examples.ThirdPerson;
using UnityEngine;

namespace ECM2.Examples.PlanetWalk;

public class ThirdPersonController : ECM2.Examples.ThirdPerson.ThirdPersonController
{
	private Vector3 _cameraForward = Vector3.forward;

	public override void AddControlYawInput(float value)
	{
		Vector3 up = followTarget.transform.up;
		_cameraForward = Quaternion.Euler(up * value) * _cameraForward;
	}

	protected override void UpdateCameraRotation()
	{
		Vector3 normal = followTarget.transform.up;
		Vector3.OrthoNormalize(ref normal, ref _cameraForward);
		_character.cameraTransform.rotation = Quaternion.LookRotation(_cameraForward, normal) * Quaternion.Euler(_cameraPitch, 0f, 0f);
	}
}
