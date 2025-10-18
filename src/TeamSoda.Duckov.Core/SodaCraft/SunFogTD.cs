using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SodaCraft;

[Serializable]
[VolumeComponentMenu("SodaCraft/SunFogTD")]
public class SunFogTD : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(value: false);

	public ColorParameter fogColor = new ColorParameter(new Color(0.68718916f, 1.070217f, 1.3615336f, 0f), hdr: true, showAlpha: true, showEyeDropper: false);

	public ColorParameter sunColor = new ColorParameter(new Color(4.061477f, 2.5092788f, 1.7816858f, 0f), hdr: true, showAlpha: true, showEyeDropper: false);

	public FloatRangeParameter clipPlanes = new FloatRangeParameter(new Vector2(41f, 72f), 0.3f, 1000f);

	public Vector2Parameter sunPoint = new Vector2Parameter(new Vector2(-2.63f, 1.23f));

	public FloatParameter sunSize = new ClampedFloatParameter(1.85f, 0f, 10f);

	public ClampedFloatParameter sunPower = new ClampedFloatParameter(1f, 0.1f, 10f);

	public ClampedFloatParameter sunAlphaGain = new ClampedFloatParameter(0.001f, 0f, 0.25f);

	private int fogColorHash = Shader.PropertyToID("SunFogColor");

	private int sunColorHash = Shader.PropertyToID("SunFogSunColor");

	private int nearDistanceHash = Shader.PropertyToID("SunFogNearDistance");

	private int farDistanceHash = Shader.PropertyToID("SunFogFarDistance");

	private int sunPointHash = Shader.PropertyToID("SunFogSunPoint");

	private int sunSizeHash = Shader.PropertyToID("SunFogSunSize");

	private int sunPowerHash = Shader.PropertyToID("SunFogSunPower");

	private int sunAlphaGainHash = Shader.PropertyToID("SunFogSunAplhaGain");

	public bool IsActive()
	{
		return enable.value;
	}

	public bool IsTileCompatible()
	{
		return false;
	}

	public override void Override(VolumeComponent state, float interpFactor)
	{
		SunFogTD sunFogTD = state as SunFogTD;
		base.Override(state, interpFactor);
		Shader.SetGlobalColor(fogColorHash, sunFogTD.fogColor.value);
		Shader.SetGlobalColor(sunColorHash, sunFogTD.sunColor.value);
		Shader.SetGlobalFloat(nearDistanceHash, sunFogTD.clipPlanes.value.x);
		Shader.SetGlobalFloat(farDistanceHash, sunFogTD.clipPlanes.value.y);
		Shader.SetGlobalFloat(sunSizeHash, sunFogTD.sunSize.value);
		Shader.SetGlobalFloat(sunPowerHash, sunFogTD.sunPower.value);
		Shader.SetGlobalVector(sunPointHash, sunFogTD.sunPoint.value);
		Shader.SetGlobalFloat(sunAlphaGainHash, sunFogTD.sunAlphaGain.value);
	}
}
