using UnityEngine;

namespace VLB;

public static class SpotLightHelper
{
	public static float GetIntensity(Light light)
	{
		if (!(light != null))
		{
			return 0f;
		}
		return light.intensity;
	}

	public static float GetSpotAngle(Light light)
	{
		if (!(light != null))
		{
			return 0f;
		}
		return light.spotAngle;
	}

	public static float GetFallOffEnd(Light light)
	{
		if (!(light != null))
		{
			return 0f;
		}
		return light.range;
	}
}
