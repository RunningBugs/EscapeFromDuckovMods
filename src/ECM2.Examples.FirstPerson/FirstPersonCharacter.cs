using UnityEngine;

namespace ECM2.Examples.FirstPerson;

public class FirstPersonCharacter : Character
{
	[Tooltip("The first person camera parent.")]
	public GameObject cameraParent;

	private float _cameraPitch;

	public virtual void AddControlYawInput(float value)
	{
		if (value != 0f)
		{
			AddYawInput(value);
		}
	}

	public virtual void AddControlPitchInput(float value, float minPitch = -80f, float maxPitch = 80f)
	{
		if (value != 0f)
		{
			_cameraPitch = MathLib.ClampAngle(_cameraPitch + value, minPitch, maxPitch);
		}
	}

	protected virtual void UpdateCameraParentRotation()
	{
		cameraParent.transform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
	}

	protected virtual void LateUpdate()
	{
		UpdateCameraParentRotation();
	}

	protected override void Reset()
	{
		base.Reset();
		SetRotationMode(RotationMode.None);
	}
}
