using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace VLB;

public static class MaterialManager
{
	public enum BlendingMode
	{
		Additive,
		SoftAdditive,
		TraditionalTransparency,
		Count
	}

	public enum ColorGradient
	{
		Off,
		MatrixLow,
		MatrixHigh,
		Count
	}

	public enum Noise3D
	{
		Off,
		On,
		Count
	}

	public static class SD
	{
		public enum DepthBlend
		{
			Off,
			On,
			Count
		}

		public enum DynamicOcclusion
		{
			Off,
			ClippingPlane,
			DepthTexture,
			Count
		}

		public enum MeshSkewing
		{
			Off,
			On,
			Count
		}

		public enum ShaderAccuracy
		{
			Fast,
			High,
			Count
		}
	}

	public static class HD
	{
		public enum Attenuation
		{
			Linear,
			Quadratic,
			Count
		}

		public enum Shadow
		{
			Off,
			On,
			Count
		}

		public enum Cookie
		{
			Off,
			SingleChannel,
			RGBA,
			Count
		}
	}

	private interface IStaticProperties
	{
		int GetPropertiesCount();

		int GetMaterialID();

		void ApplyToMaterial(Material mat);

		ShaderMode GetShaderMode();
	}

	public struct StaticPropertiesSD : IStaticProperties
	{
		public BlendingMode blendingMode;

		public Noise3D noise3D;

		public SD.DepthBlend depthBlend;

		public ColorGradient colorGradient;

		public SD.DynamicOcclusion dynamicOcclusion;

		public SD.MeshSkewing meshSkewing;

		public SD.ShaderAccuracy shaderAccuracy;

		public static int staticPropertiesCount => 432;

		private int blendingModeID => (int)blendingMode;

		private int noise3DID
		{
			get
			{
				if (!Config.Instance.featureEnabledNoise3D)
				{
					return 0;
				}
				return (int)noise3D;
			}
		}

		private int depthBlendID
		{
			get
			{
				if (!Config.Instance.featureEnabledDepthBlend)
				{
					return 0;
				}
				return (int)depthBlend;
			}
		}

		private int colorGradientID
		{
			get
			{
				if (Config.Instance.featureEnabledColorGradient == FeatureEnabledColorGradient.Off)
				{
					return 0;
				}
				return (int)colorGradient;
			}
		}

		private int dynamicOcclusionID
		{
			get
			{
				if (!Config.Instance.featureEnabledDynamicOcclusion)
				{
					return 0;
				}
				return (int)dynamicOcclusion;
			}
		}

		private int meshSkewingID
		{
			get
			{
				if (!Config.Instance.featureEnabledMeshSkewing)
				{
					return 0;
				}
				return (int)meshSkewing;
			}
		}

		private int shaderAccuracyID
		{
			get
			{
				if (!Config.Instance.featureEnabledShaderAccuracyHigh)
				{
					return 0;
				}
				return (int)shaderAccuracy;
			}
		}

		public ShaderMode GetShaderMode()
		{
			return ShaderMode.SD;
		}

		public int GetPropertiesCount()
		{
			return staticPropertiesCount;
		}

		public int GetMaterialID()
		{
			return (((((blendingModeID * 2 + noise3DID) * 2 + depthBlendID) * 3 + colorGradientID) * 3 + dynamicOcclusionID) * 2 + meshSkewingID) * 2 + shaderAccuracyID;
		}

		public void ApplyToMaterial(Material mat)
		{
			mat.SetKeywordEnabled("VLB_ALPHA_AS_BLACK", BlendingMode_AlphaAsBlack[(int)blendingMode]);
			mat.SetKeywordEnabled("VLB_COLOR_GRADIENT_MATRIX_LOW", colorGradient == ColorGradient.MatrixLow);
			mat.SetKeywordEnabled("VLB_COLOR_GRADIENT_MATRIX_HIGH", colorGradient == ColorGradient.MatrixHigh);
			mat.SetKeywordEnabled("VLB_DEPTH_BLEND", depthBlend == SD.DepthBlend.On);
			mat.SetKeywordEnabled("VLB_NOISE_3D", noise3D == Noise3D.On);
			mat.SetKeywordEnabled("VLB_OCCLUSION_CLIPPING_PLANE", dynamicOcclusion == SD.DynamicOcclusion.ClippingPlane);
			mat.SetKeywordEnabled("VLB_OCCLUSION_DEPTH_TEXTURE", dynamicOcclusion == SD.DynamicOcclusion.DepthTexture);
			mat.SetKeywordEnabled("VLB_MESH_SKEWING", meshSkewing == SD.MeshSkewing.On);
			mat.SetKeywordEnabled("VLB_SHADER_ACCURACY_HIGH", shaderAccuracy == SD.ShaderAccuracy.High);
			mat.SetBlendingMode(ShaderProperties.BlendSrcFactor, BlendingMode_SrcFactor[(int)blendingMode]);
			mat.SetBlendingMode(ShaderProperties.BlendDstFactor, BlendingMode_DstFactor[(int)blendingMode]);
			mat.SetZTest(ShaderProperties.ZTest, CompareFunction.LessEqual);
		}
	}

	public struct StaticPropertiesHD : IStaticProperties
	{
		public BlendingMode blendingMode;

		public HD.Attenuation attenuation;

		public Noise3D noise3D;

		public ColorGradient colorGradient;

		public HD.Shadow shadow;

		public HD.Cookie cookie;

		public int raymarchingQualityIndex;

		public static int staticPropertiesCount => 216 * Config.Instance.raymarchingQualitiesCount;

		private int blendingModeID => (int)blendingMode;

		private int attenuationID => (int)attenuation;

		private int noise3DID
		{
			get
			{
				if (!Config.Instance.featureEnabledNoise3D)
				{
					return 0;
				}
				return (int)noise3D;
			}
		}

		private int colorGradientID
		{
			get
			{
				if (Config.Instance.featureEnabledColorGradient == FeatureEnabledColorGradient.Off)
				{
					return 0;
				}
				return (int)colorGradient;
			}
		}

		private int dynamicOcclusionID
		{
			get
			{
				if (!Config.Instance.featureEnabledShadow)
				{
					return 0;
				}
				return (int)shadow;
			}
		}

		private int cookieID
		{
			get
			{
				if (!Config.Instance.featureEnabledCookie)
				{
					return 0;
				}
				return (int)cookie;
			}
		}

		private int raymarchingQualityID => raymarchingQualityIndex;

		public ShaderMode GetShaderMode()
		{
			return ShaderMode.HD;
		}

		public int GetPropertiesCount()
		{
			return staticPropertiesCount;
		}

		public int GetMaterialID()
		{
			return (((((blendingModeID * 2 + attenuationID) * 2 + noise3DID) * 3 + colorGradientID) * 2 + dynamicOcclusionID) * 3 + cookieID) * Config.Instance.raymarchingQualitiesCount + raymarchingQualityID;
		}

		public void ApplyToMaterial(Material mat)
		{
			mat.SetKeywordEnabled("VLB_ALPHA_AS_BLACK", BlendingMode_AlphaAsBlack[(int)blendingMode]);
			mat.SetKeywordEnabled("VLB_ATTENUATION_LINEAR", attenuation == HD.Attenuation.Linear);
			mat.SetKeywordEnabled("VLB_ATTENUATION_QUAD", attenuation == HD.Attenuation.Quadratic);
			mat.SetKeywordEnabled("VLB_COLOR_GRADIENT_MATRIX_LOW", colorGradient == ColorGradient.MatrixLow);
			mat.SetKeywordEnabled("VLB_COLOR_GRADIENT_MATRIX_HIGH", colorGradient == ColorGradient.MatrixHigh);
			mat.SetKeywordEnabled("VLB_NOISE_3D", noise3D == Noise3D.On);
			mat.SetKeywordEnabled("VLB_SHADOW", shadow == HD.Shadow.On);
			mat.SetKeywordEnabled("VLB_COOKIE_1CHANNEL", cookie == HD.Cookie.SingleChannel);
			mat.SetKeywordEnabled("VLB_COOKIE_RGBA", cookie == HD.Cookie.RGBA);
			for (int i = 0; i < Config.Instance.raymarchingQualitiesCount; i++)
			{
				mat.SetKeywordEnabled(ShaderKeywords.HD.GetRaymarchingQuality(i), raymarchingQualityIndex == i);
			}
			mat.SetBlendingMode(ShaderProperties.BlendSrcFactor, BlendingMode_SrcFactor[(int)blendingMode]);
			mat.SetBlendingMode(ShaderProperties.BlendDstFactor, BlendingMode_DstFactor[(int)blendingMode]);
			mat.SetZTest(ShaderProperties.ZTest, CompareFunction.Always);
		}
	}

	private class MaterialsGroup
	{
		public Material[] materials;

		public MaterialsGroup(int count)
		{
			materials = new Material[count];
		}
	}

	private enum ZWrite
	{
		Off,
		On
	}

	public static MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();

	private static readonly BlendMode[] BlendingMode_SrcFactor = new BlendMode[3]
	{
		BlendMode.One,
		BlendMode.OneMinusDstColor,
		BlendMode.SrcAlpha
	};

	private static readonly BlendMode[] BlendingMode_DstFactor = new BlendMode[3]
	{
		BlendMode.One,
		BlendMode.One,
		BlendMode.OneMinusSrcAlpha
	};

	private static readonly bool[] BlendingMode_AlphaAsBlack = new bool[3] { true, true, false };

	private static Hashtable ms_MaterialsGroupSD = new Hashtable(1);

	private static Hashtable ms_MaterialsGroupHD = new Hashtable(1);

	public static Material NewMaterialPersistent(Shader shader, bool gpuInstanced)
	{
		if (!shader)
		{
			Debug.LogError("Invalid VLB Shader. Please try to reset the VLB Config asset or reinstall the plugin.");
			return null;
		}
		Material material = new Material(shader);
		BatchingHelper.SetMaterialProperties(material, gpuInstanced);
		return material;
	}

	public static Material GetInstancedMaterial(uint groupID, ref StaticPropertiesSD staticProps)
	{
		IStaticProperties staticProps2 = staticProps;
		return GetInstancedMaterial(ms_MaterialsGroupSD, groupID, ref staticProps2);
	}

	public static Material GetInstancedMaterial(uint groupID, ref StaticPropertiesHD staticProps)
	{
		IStaticProperties staticProps2 = staticProps;
		return GetInstancedMaterial(ms_MaterialsGroupHD, groupID, ref staticProps2);
	}

	private static Material GetInstancedMaterial(Hashtable groups, uint groupID, ref IStaticProperties staticProps)
	{
		MaterialsGroup materialsGroup = (MaterialsGroup)groups[groupID];
		if (materialsGroup == null)
		{
			materialsGroup = new MaterialsGroup(staticProps.GetPropertiesCount());
			groups[groupID] = materialsGroup;
		}
		int materialID = staticProps.GetMaterialID();
		Material material = materialsGroup.materials[materialID];
		if (material == null)
		{
			material = Config.Instance.NewMaterialTransient(staticProps.GetShaderMode(), gpuInstanced: true);
			if ((bool)material)
			{
				materialsGroup.materials[materialID] = material;
				staticProps.ApplyToMaterial(material);
			}
		}
		return material;
	}

	public static bool EnableGPUInstancing(ShaderMode shaderMode, bool enabled)
	{
		if (Config.Instance.GetActualRenderingMode(shaderMode) != RenderingMode.GPUInstancing)
		{
			Debug.LogErrorFormat("To change GPU Instancing at runtime, the VLB plugin's config must be configured to use the GPUInstancing RenderingMode.");
			return false;
		}
		Hashtable obj = ((shaderMode == ShaderMode.SD) ? ms_MaterialsGroupSD : ms_MaterialsGroupHD);
		bool result = false;
		foreach (MaterialsGroup value in obj.Values)
		{
			if (value == null)
			{
				continue;
			}
			Material[] materials = value.materials;
			foreach (Material material in materials)
			{
				if ((bool)material)
				{
					material.enableInstancing = enabled;
					result = true;
				}
			}
		}
		return result;
	}

	private static void SetBlendingMode(this Material mat, int nameID, BlendMode value)
	{
		mat.SetInt(nameID, (int)value);
	}

	private static void SetStencilRef(this Material mat, int nameID, int value)
	{
		mat.SetInt(nameID, value);
	}

	private static void SetStencilComp(this Material mat, int nameID, CompareFunction value)
	{
		mat.SetInt(nameID, (int)value);
	}

	private static void SetStencilOp(this Material mat, int nameID, StencilOp value)
	{
		mat.SetInt(nameID, (int)value);
	}

	private static void SetCull(this Material mat, int nameID, CullMode value)
	{
		mat.SetInt(nameID, (int)value);
	}

	private static void SetZWrite(this Material mat, int nameID, ZWrite value)
	{
		mat.SetInt(nameID, (int)value);
	}

	private static void SetZTest(this Material mat, int nameID, CompareFunction value)
	{
		mat.SetInt(nameID, (int)value);
	}
}
