using UnityEngine;
using UnityEngine.Rendering;

public class TimeOfDayVolumeControl : MonoBehaviour
{
	private VolumeProfile currentProfile;

	private VolumeProfile blendingTargetProfile;

	private VolumeProfile bufferTargetProfile;

	public Volume fromVolume;

	public Volume toVolume;

	private bool blending;

	private float blendTimer;

	public float blendTime = 2f;

	public AnimationCurve blendCurve;

	public VolumeProfile CurrentProfile => currentProfile;

	public VolumeProfile BufferTargetProfile => bufferTargetProfile;

	private void Update()
	{
		if (!blending && bufferTargetProfile != null)
		{
			StartBlendToBufferdTarget();
		}
		if (blending)
		{
			UpdateBlending(Time.deltaTime);
		}
		if (!blending && fromVolume.gameObject.activeSelf)
		{
			fromVolume.gameObject.SetActive(value: false);
		}
	}

	private void UpdateBlending(float deltaTime)
	{
		blendTimer += deltaTime;
		float num = blendTimer / blendTime;
		if (num > 1f)
		{
			num = 1f;
			blending = false;
		}
		toVolume.weight = blendCurve.Evaluate(num);
	}

	public void SetTargetProfile(VolumeProfile profile)
	{
		bufferTargetProfile = profile;
	}

	private void StartBlendToBufferdTarget()
	{
		blending = true;
		blendingTargetProfile = bufferTargetProfile;
		bufferTargetProfile = null;
		currentProfile = blendingTargetProfile;
		fromVolume.gameObject.SetActive(value: true);
		fromVolume.profile = toVolume.profile;
		fromVolume.weight = 1f;
		toVolume.profile = blendingTargetProfile;
		toVolume.weight = 0f;
		blendTimer = 0f;
	}

	public void ForceSetProfile(VolumeProfile profile)
	{
		bufferTargetProfile = profile;
		StartBlendToBufferdTarget();
		UpdateBlending(999f);
	}
}
