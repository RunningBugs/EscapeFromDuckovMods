using System;
using Umbra;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SodaCraft;

[Serializable]
[VolumeComponentMenu("SodaCraft/LightControl")]
public class LightControl : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(value: false);

	public ColorParameter skyColor = new ColorParameter(Color.black, hdr: true, showAlpha: true, showEyeDropper: false);

	public ColorParameter equatorColor = new ColorParameter(Color.black, hdr: true, showAlpha: true, showEyeDropper: false);

	public ColorParameter groundColor = new ColorParameter(Color.black, hdr: true, showAlpha: true, showEyeDropper: false);

	public ColorParameter sunColor = new ColorParameter(Color.white, hdr: true, showAlpha: true, showEyeDropper: false);

	public ColorParameter fowColor = new ColorParameter(Color.white, hdr: true, showAlpha: true, showEyeDropper: false);

	public MinFloatParameter sunIntensity = new MinFloatParameter(1f, 0f);

	public ClampedFloatParameter sunShadowHardness = new ClampedFloatParameter(0.96f, 0f, 1f);

	public Vector3Parameter sunRotation = new Vector3Parameter(new Vector3(59f, 168f, 0f));

	public ColorParameter SodaLightTint = new ColorParameter(Color.white, hdr: true, showAlpha: true, showEyeDropper: false);

	private int SodaPointLight_EnviromentTintID = Shader.PropertyToID("SodaPointLight_EnviromentTint");

	private int fowColorID = Shader.PropertyToID("_SodaUnknowColor");

	private static Light light;

	private static UmbraSoftShadows lightShadows;

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
		LightControl lightControl = state as LightControl;
		base.Override(state, interpFactor);
		RenderSettings.ambientSkyColor = lightControl.skyColor.value;
		RenderSettings.ambientEquatorColor = lightControl.equatorColor.value;
		RenderSettings.ambientGroundColor = lightControl.groundColor.value;
		Shader.SetGlobalColor(fowColorID, lightControl.fowColor.value);
		Shader.SetGlobalColor(SodaPointLight_EnviromentTintID, lightControl.SodaLightTint.value);
		if (!light)
		{
			light = RenderSettings.sun;
		}
		if ((bool)light)
		{
			light.color = lightControl.sunColor.value;
			light.intensity = lightControl.sunIntensity.value;
			light.transform.rotation = Quaternion.Euler(lightControl.sunRotation.value);
			if (!lightShadows)
			{
				lightShadows = light.GetComponent<UmbraSoftShadows>();
			}
			if ((bool)lightShadows)
			{
				float value = lightControl.sunShadowHardness.value;
				lightShadows.profile.contactStrength = value;
			}
		}
	}
}
