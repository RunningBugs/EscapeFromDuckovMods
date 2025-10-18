using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SodaCraft;

[Serializable]
[VolumeComponentMenu("SodaCraft/EdgeLight")]
public class EdgeLight : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(value: false);

	public Vector2Parameter direction = new Vector2Parameter(new Vector2(-1f, 1f));

	public ClampedFloatParameter edgeLightWidth = new ClampedFloatParameter(0.001f, 0f, 0.05f);

	public ClampedFloatParameter edgeLightFix = new ClampedFloatParameter(0.001f, 0f, 0.05f);

	public FloatParameter EdgeLightClampDistance = new ClampedFloatParameter(0.001f, 0.001f, 1f);

	public ColorParameter edgeLightColor = new ColorParameter(Color.white, hdr: true, showAlpha: false, showEyeDropper: false);

	public FloatParameter blendScreenColor = new ClampedFloatParameter(1f, 0f, 1f);

	private int edgeLightDirectionHash = Shader.PropertyToID("_EdgeLightDirection");

	private int widthHash = Shader.PropertyToID("_EdgeLightWidth");

	private int colorHash = Shader.PropertyToID("_EdgeLightColor");

	private int fixHash = Shader.PropertyToID("_EdgeLightFix");

	private int clampDistanceHash = Shader.PropertyToID("_EdgeLightClampDistance");

	private int edgeLightBlendScreenColorHash = Shader.PropertyToID("_EdgeLightBlendScreenColor");

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
		EdgeLight edgeLight = state as EdgeLight;
		base.Override(state, interpFactor);
		Shader.SetGlobalVector(edgeLightDirectionHash, edgeLight.direction.value);
		Shader.SetGlobalFloat(widthHash, edgeLight.edgeLightWidth.value);
		Shader.SetGlobalFloat(fixHash, edgeLight.edgeLightFix.value);
		Shader.SetGlobalFloat(clampDistanceHash, edgeLight.EdgeLightClampDistance.value);
		Shader.SetGlobalColor(colorHash, edgeLight.edgeLightColor.value);
		Shader.SetGlobalFloat(edgeLightBlendScreenColorHash, edgeLight.blendScreenColor.value);
	}
}
