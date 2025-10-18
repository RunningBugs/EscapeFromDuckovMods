using UnityEngine;

namespace VLB;

public static class BatchingHelper
{
	public static bool forceEnableDepthBlend
	{
		get
		{
			RenderingMode actualRenderingMode = Config.Instance.GetActualRenderingMode(ShaderMode.SD);
			if (actualRenderingMode != RenderingMode.GPUInstancing)
			{
				return actualRenderingMode == RenderingMode.SRPBatcher;
			}
			return true;
		}
	}

	public static bool IsGpuInstancingEnabled(Material material)
	{
		return material.enableInstancing;
	}

	public static void SetMaterialProperties(Material material, bool enableGpuInstancing)
	{
		material.enableInstancing = enableGpuInstancing;
	}

	private static bool DoesRenderingModePreventBatching(ShaderMode shaderMode, ref string reasons)
	{
		RenderingMode actualRenderingMode = Config.Instance.GetActualRenderingMode(shaderMode);
		if (actualRenderingMode != RenderingMode.GPUInstancing && actualRenderingMode != RenderingMode.SRPBatcher)
		{
			reasons = $"Current Rendering Mode is '{actualRenderingMode}'. To enable batching, use '{RenderingMode.GPUInstancing}'";
			if (Config.Instance.renderPipeline != RenderPipeline.BuiltIn)
			{
				reasons += $" or '{RenderingMode.SRPBatcher}'";
			}
			return true;
		}
		return false;
	}

	public static bool CanBeBatched(VolumetricLightBeamSD beamA, VolumetricLightBeamSD beamB, ref string reasons)
	{
		if (DoesRenderingModePreventBatching(ShaderMode.SD, ref reasons))
		{
			return false;
		}
		bool flag = true;
		flag &= CanBeBatched(beamA, ref reasons);
		flag &= CanBeBatched(beamB, ref reasons);
		if (Config.Instance.featureEnabledDynamicOcclusion && beamA.GetComponent<DynamicOcclusionAbstractBase>() == null != (beamB.GetComponent<DynamicOcclusionAbstractBase>() == null))
		{
			AppendErrorMessage(ref reasons, $"{beamA.name}/{beamB.name}: dynamically occluded and non occluded beams cannot be batched together");
			flag = false;
		}
		if (Config.Instance.featureEnabledColorGradient != FeatureEnabledColorGradient.Off && beamA.colorMode != beamB.colorMode)
		{
			AppendErrorMessage(ref reasons, $"'Color Mode' mismatch: {beamA.colorMode} / {beamB.colorMode}");
			flag = false;
		}
		if (beamA.blendingMode != beamB.blendingMode)
		{
			AppendErrorMessage(ref reasons, $"'Blending Mode' mismatch: {beamA.blendingMode} / {beamB.blendingMode}");
			flag = false;
		}
		if (Config.Instance.featureEnabledNoise3D && beamA.isNoiseEnabled != beamB.isNoiseEnabled)
		{
			AppendErrorMessage(ref reasons, $"'3D Noise' enabled mismatch: {beamA.noiseMode} / {beamB.noiseMode}");
			flag = false;
		}
		if (Config.Instance.featureEnabledDepthBlend && !forceEnableDepthBlend && beamA.depthBlendDistance > 0f != beamB.depthBlendDistance > 0f)
		{
			AppendErrorMessage(ref reasons, $"'Opaque Geometry Blending' mismatch: {beamA.depthBlendDistance} / {beamB.depthBlendDistance}");
			flag = false;
		}
		if (Config.Instance.featureEnabledShaderAccuracyHigh && beamA.shaderAccuracy != beamB.shaderAccuracy)
		{
			AppendErrorMessage(ref reasons, $"'Shader Accuracy' mismatch: {beamA.shaderAccuracy} / {beamB.shaderAccuracy}");
			flag = false;
		}
		return flag;
	}

	public static bool CanBeBatched(VolumetricLightBeamSD beam, ref string reasons)
	{
		bool result = true;
		if (Config.Instance.GetActualRenderingMode(ShaderMode.SD) == RenderingMode.GPUInstancing && beam.geomMeshType != MeshType.Shared)
		{
			AppendErrorMessage(ref reasons, $"{beam.name} is not using shared mesh");
			result = false;
		}
		if (Config.Instance.featureEnabledDynamicOcclusion && beam.GetComponent<DynamicOcclusionDepthBuffer>() != null)
		{
			AppendErrorMessage(ref reasons, $"{beam.name} is using the DynamicOcclusion DepthBuffer feature");
			result = false;
		}
		return result;
	}

	public static bool CanBeBatched(VolumetricLightBeamHD beamA, VolumetricLightBeamHD beamB, ref string reasons)
	{
		if (DoesRenderingModePreventBatching(ShaderMode.HD, ref reasons))
		{
			return false;
		}
		bool flag = true;
		flag &= CanBeBatched(beamA, ref reasons);
		flag &= CanBeBatched(beamB, ref reasons);
		if (Config.Instance.featureEnabledColorGradient != FeatureEnabledColorGradient.Off && beamA.colorMode != beamB.colorMode)
		{
			AppendErrorMessage(ref reasons, $"'Color Mode' mismatch: {beamA.colorMode} / {beamB.colorMode}");
			flag = false;
		}
		if (beamA.blendingMode != beamB.blendingMode)
		{
			AppendErrorMessage(ref reasons, $"'Blending Mode' mismatch: {beamA.blendingMode} / {beamB.blendingMode}");
			flag = false;
		}
		if (beamA.attenuationEquation != beamB.attenuationEquation)
		{
			AppendErrorMessage(ref reasons, $"'Attenuation Equation' mismatch: {beamA.attenuationEquation} / {beamB.attenuationEquation}");
			flag = false;
		}
		if (Config.Instance.featureEnabledNoise3D && beamA.isNoiseEnabled != beamB.isNoiseEnabled)
		{
			AppendErrorMessage(ref reasons, $"'3D Noise' enabled mismatch: {beamA.noiseMode} / {beamB.noiseMode}");
			flag = false;
		}
		if (beamA.raymarchingQualityID != beamB.raymarchingQualityID)
		{
			AppendErrorMessage(ref reasons, $"'Raymarching Quality' mismatch: {Config.Instance.GetRaymarchingQualityForUniqueID(beamA.raymarchingQualityID).name} / {Config.Instance.GetRaymarchingQualityForUniqueID(beamB.raymarchingQualityID).name}");
			flag = false;
		}
		return flag;
	}

	public static bool CanBeBatched(VolumetricLightBeamHD beam, ref string reasons)
	{
		bool result = true;
		if (Config.Instance.featureEnabledShadow && beam.GetAdditionalComponentShadow() != null)
		{
			AppendErrorMessage(ref reasons, $"{beam.name} is using the Shadow feature");
			result = false;
		}
		if (Config.Instance.featureEnabledCookie && beam.GetAdditionalComponentCookie() != null)
		{
			AppendErrorMessage(ref reasons, $"{beam.name} is using the Cookie feature");
			result = false;
		}
		return result;
	}

	public static bool CanBeBatched(VolumetricLightBeamAbstractBase beamA, VolumetricLightBeamAbstractBase beamB, ref string reasons)
	{
		if (beamA is VolumetricLightBeamSD beamA2 && beamB is VolumetricLightBeamSD beamB2)
		{
			return CanBeBatched(beamA2, beamB2, ref reasons);
		}
		if (beamA is VolumetricLightBeamHD beamA3 && beamB is VolumetricLightBeamHD beamB3)
		{
			return CanBeBatched(beamA3, beamB3, ref reasons);
		}
		return false;
	}

	private static void AppendErrorMessage(ref string message, string toAppend)
	{
		if (message != "")
		{
			message += "\n";
		}
		message = message + "- " + toAppend;
	}
}
