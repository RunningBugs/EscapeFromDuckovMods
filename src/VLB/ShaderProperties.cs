using UnityEngine;

namespace VLB;

public static class ShaderProperties
{
	public static class SD
	{
		public static readonly int FadeOutFactor = Shader.PropertyToID("_FadeOutFactor");

		public static readonly int ConeSlopeCosSin = Shader.PropertyToID("_ConeSlopeCosSin");

		public static readonly int AlphaInside = Shader.PropertyToID("_AlphaInside");

		public static readonly int AlphaOutside = Shader.PropertyToID("_AlphaOutside");

		public static readonly int AttenuationLerpLinearQuad = Shader.PropertyToID("_AttenuationLerpLinearQuad");

		public static readonly int DistanceCamClipping = Shader.PropertyToID("_DistanceCamClipping");

		public static readonly int FresnelPow = Shader.PropertyToID("_FresnelPow");

		public static readonly int GlareBehind = Shader.PropertyToID("_GlareBehind");

		public static readonly int GlareFrontal = Shader.PropertyToID("_GlareFrontal");

		public static readonly int DrawCap = Shader.PropertyToID("_DrawCap");

		public static readonly int DepthBlendDistance = Shader.PropertyToID("_DepthBlendDistance");

		public static readonly int CameraParams = Shader.PropertyToID("_CameraParams");

		public static readonly int DynamicOcclusionClippingPlaneWS = Shader.PropertyToID("_DynamicOcclusionClippingPlaneWS");

		public static readonly int DynamicOcclusionClippingPlaneProps = Shader.PropertyToID("_DynamicOcclusionClippingPlaneProps");

		public static readonly int DynamicOcclusionDepthTexture = Shader.PropertyToID("_DynamicOcclusionDepthTexture");

		public static readonly int DynamicOcclusionDepthProps = Shader.PropertyToID("_DynamicOcclusionDepthProps");

		public static readonly int LocalForwardDirection = Shader.PropertyToID("_LocalForwardDirection");

		public static readonly int TiltVector = Shader.PropertyToID("_TiltVector");

		public static readonly int AdditionalClippingPlaneWS = Shader.PropertyToID("_AdditionalClippingPlaneWS");
	}

	public static class HD
	{
		public static readonly int Intensity = Shader.PropertyToID("_Intensity");

		public static readonly int SideSoftness = Shader.PropertyToID("_SideSoftness");

		public static readonly int CameraForwardOS = Shader.PropertyToID("_CameraForwardOS");

		public static readonly int CameraForwardWS = Shader.PropertyToID("_CameraForwardWS");

		public static readonly int TransformScale = Shader.PropertyToID("_TransformScale");

		public static readonly int ShadowDepthTexture = Shader.PropertyToID("_ShadowDepthTexture");

		public static readonly int ShadowProps = Shader.PropertyToID("_ShadowProps");

		public static readonly int Jittering = Shader.PropertyToID("_Jittering");

		public static readonly int CookieTexture = Shader.PropertyToID("_CookieTexture");

		public static readonly int CookieProperties = Shader.PropertyToID("_CookieProperties");

		public static readonly int CookiePosAndScale = Shader.PropertyToID("_CookiePosAndScale");

		public static readonly int GlobalCameraBlendingDistance = Shader.PropertyToID("_VLB_CameraBlendingDistance");

		public static readonly int GlobalJitteringNoiseTex = Shader.PropertyToID("_VLB_JitteringNoiseTex");
	}

	public static readonly int ConeRadius = Shader.PropertyToID("_ConeRadius");

	public static readonly int ConeGeomProps = Shader.PropertyToID("_ConeGeomProps");

	public static readonly int ColorFlat = Shader.PropertyToID("_ColorFlat");

	public static readonly int DistanceFallOff = Shader.PropertyToID("_DistanceFallOff");

	public static readonly int NoiseVelocityAndScale = Shader.PropertyToID("_NoiseVelocityAndScale");

	public static readonly int NoiseParam = Shader.PropertyToID("_NoiseParam");

	public static readonly int ColorGradientMatrix = Shader.PropertyToID("_ColorGradientMatrix");

	public static readonly int LocalToWorldMatrix = Shader.PropertyToID("_LocalToWorldMatrix");

	public static readonly int WorldToLocalMatrix = Shader.PropertyToID("_WorldToLocalMatrix");

	public static readonly int BlendSrcFactor = Shader.PropertyToID("_BlendSrcFactor");

	public static readonly int BlendDstFactor = Shader.PropertyToID("_BlendDstFactor");

	public static readonly int ZTest = Shader.PropertyToID("_ZTest");

	public static readonly int ParticlesTintColor = Shader.PropertyToID("_TintColor");

	public static readonly int HDRPExposureWeight = Shader.PropertyToID("_HDRPExposureWeight");

	public static readonly int GlobalUsesReversedZBuffer = Shader.PropertyToID("_VLB_UsesReversedZBuffer");

	public static readonly int GlobalNoiseTex3D = Shader.PropertyToID("_VLB_NoiseTex3D");

	public static readonly int GlobalNoiseCustomTime = Shader.PropertyToID("_VLB_NoiseCustomTime");

	public static readonly int GlobalDitheringFactor = Shader.PropertyToID("_VLB_DitheringFactor");

	public static readonly int GlobalDitheringNoiseTex = Shader.PropertyToID("_VLB_DitheringNoiseTex");
}
