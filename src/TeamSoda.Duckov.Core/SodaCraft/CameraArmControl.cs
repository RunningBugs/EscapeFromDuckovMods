using System;
using UnityEngine.Rendering;

namespace SodaCraft;

[Serializable]
[VolumeComponentMenu("SodaCraft/CameraArmControl")]
public class CameraArmControl : VolumeComponent
{
	public BoolParameter enable = new BoolParameter(value: false);

	public MinFloatParameter pitch = new MinFloatParameter(55f, 0f);

	public FloatParameter yaw = new FloatParameter(-30f);

	public MinFloatParameter distance = new MinFloatParameter(45f, 2f);

	public bool IsActive()
	{
		return enable.value;
	}

	public override void Override(VolumeComponent state, float interpFactor)
	{
		CameraArmControl obj = state as CameraArmControl;
		base.Override(state, interpFactor);
		CameraArm.globalPitch = obj.pitch.value;
		CameraArm.globalYaw = obj.yaw.value;
		CameraArm.globalDistance = obj.distance.value;
	}
}
