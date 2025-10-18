using UnityEngine;

namespace VLB;

public static class Consts
{
	public static class Help
	{
		public static class SD
		{
			public const string UrlBeam = "http://saladgamer.com/vlb-doc/comp-lightbeam-sd/";

			public const string UrlDynamicOcclusionRaycasting = "http://saladgamer.com/vlb-doc/comp-dynocclusion-sd-raycasting/";

			public const string UrlDynamicOcclusionDepthBuffer = "http://saladgamer.com/vlb-doc/comp-dynocclusion-sd-depthbuffer/";

			public const string UrlSkewingHandle = "http://saladgamer.com/vlb-doc/comp-skewinghandle-sd/";

			public const string AddComponentMenuSD = "VLB/SD/";

			public const string AddComponentMenuBeam = "VLB/SD/Volumetric Light Beam SD";

			public const string AddComponentMenuDynamicOcclusionRaycasting = "VLB/SD/Dynamic Occlusion (Raycasting)";

			public const string AddComponentMenuDynamicOcclusionDepthBuffer = "VLB/SD/Dynamic Occlusion (Depth Buffer)";
		}

		public static class HD
		{
			public const string UrlBeam = "http://saladgamer.com/vlb-doc/comp-lightbeam-hd/";

			public const string UrlShadow = "http://saladgamer.com/vlb-doc/comp-shadow-hd/";

			public const string UrlCookie = "http://saladgamer.com/vlb-doc/comp-cookie-hd/";

			public const string UrlTrackRealtimeChangesOnLight = "http://saladgamer.com/vlb-doc/comp-trackrealtimechanges-hd/";

			public const string AddComponentMenuHD = "VLB/HD/";

			public const string AddComponentMenuBeam3D = "VLB/HD/Volumetric Light Beam HD";

			public const string AddComponentMenuBeam2D = "VLB/HD/Volumetric Light Beam HD (2D)";

			public const string AddComponentMenuShadow = "VLB/HD/Volumetric Shadow HD";

			public const string AddComponentMenuCookie = "VLB/HD/Volumetric Cookie HD";

			public const string AddComponentMenuTrackRealtimeChangesOnLight = "VLB/HD/Track Realtime Changes On Light";
		}

		private const string UrlBase = "http://saladgamer.com/vlb-doc/";

		private const string UrlSuffix = "/";

		public const string UrlDustParticles = "http://saladgamer.com/vlb-doc/comp-dustparticles/";

		public const string UrlTriggerZone = "http://saladgamer.com/vlb-doc/comp-triggerzone/";

		public const string UrlEffectFlicker = "http://saladgamer.com/vlb-doc/comp-effect-flicker/";

		public const string UrlEffectPulse = "http://saladgamer.com/vlb-doc/comp-effect-pulse/";

		public const string UrlEffectFromProfile = "http://saladgamer.com/vlb-doc/comp-effect-from-profile/";

		public const string UrlLODBeamGroup = "http://saladgamer.com/vlb-doc/comp-lodbeamgroup/";

		public const string UrlConfig = "http://saladgamer.com/vlb-doc/config/";

		public const string AddComponentMenuBase = "VLB/";

		public const string AddComponentMenuCommon = "VLB/Common/";

		public const string AddComponentMenuDustParticles = "VLB/Common/Volumetric Dust Particles";

		public const string AddComponentMenuTriggerZone = "VLB/Common/Trigger Zone";

		public const string AddComponentMenuEffectFlicker = "VLB/Common/Effect Flicker";

		public const string AddComponentMenuEffectPulse = "VLB/Common/Effect Pulse";

		public const string AddComponentMenuEffectFromProfile = "VLB/Common/Effect From Profile";
	}

	public static class Internal
	{
		public static readonly bool ProceduralObjectsVisibleInEditor = true;

		public static HideFlags ProceduralObjectsHideFlags
		{
			get
			{
				if (!ProceduralObjectsVisibleInEditor)
				{
					return HideFlags.HideAndDontSave;
				}
				return HideFlags.DontSave | HideFlags.NotEditable;
			}
		}
	}

	public static class Beam
	{
		public static class SD
		{
			public const float FresnelPowMaxValue = 10f;

			public const float FresnelPow = 8f;

			public const float GlareFrontalDefault = 0.5f;

			public const float GlareBehindDefault = 0.5f;

			public const float GlareMin = 0f;

			public const float GlareMax = 1f;

			public static readonly Vector2 TiltDefault = Vector2.zero;

			public static readonly Vector3 SkewingLocalForwardDirectionDefault = Vector3.forward;

			public const Transform ClippingPlaneTransformDefault = null;
		}

		public static class HD
		{
			public const AttenuationEquationHD AttenuationEquationDefault = AttenuationEquationHD.Quadratic;

			public const float SideSoftnessDefault = 1f;

			public const float SideSoftnessMin = 0.0001f;

			public const float SideSoftnessMax = 10f;

			public const float JitteringFactorDefault = 0f;

			public const float JitteringFactorMin = 0f;

			public const int JitteringFrameRateDefault = 60;

			public const int JitteringFrameRateMin = 0;

			public const int JitteringFrameRateMax = 120;

			public static readonly MinMaxRangeFloat JitteringLerpRange = new MinMaxRangeFloat(0f, 0.33f);
		}

		public static readonly Color FlatColor = Color.white;

		public const ColorMode ColorModeDefault = ColorMode.Flat;

		public const float MultiplierDefault = 1f;

		public const float MultiplierMin = 0f;

		public const float IntensityDefault = 1f;

		public const float IntensityMin = 0f;

		public const float HDRPExposureWeightDefault = 0f;

		public const float HDRPExposureWeightMin = 0f;

		public const float HDRPExposureWeightMax = 1f;

		public const float SpotAngleDefault = 35f;

		public const float SpotAngleMin = 0.1f;

		public const float SpotAngleMax = 179.9f;

		public const float ConeRadiusStart = 0.1f;

		public const MeshType GeomMeshType = MeshType.Shared;

		public const int GeomSidesDefault = 18;

		public const int GeomSidesMin = 3;

		public const int GeomSidesMax = 256;

		public const int GeomSegmentsDefault = 5;

		public const int GeomSegmentsMin = 0;

		public const int GeomSegmentsMax = 64;

		public const bool GeomCap = false;

		public const bool ScalableDefault = true;

		public const AttenuationEquation AttenuationEquationDefault = AttenuationEquation.Quadratic;

		public const float AttenuationCustomBlendingDefault = 0.5f;

		public const float AttenuationCustomBlendingMin = 0f;

		public const float AttenuationCustomBlendingMax = 1f;

		public const float FallOffStart = 0f;

		public const float FallOffEnd = 3f;

		public const float FallOffDistancesMinThreshold = 0.01f;

		public const float DepthBlendDistance = 2f;

		public const float CameraClippingDistance = 0.5f;

		public const NoiseMode NoiseModeDefault = NoiseMode.Disabled;

		public const float NoiseIntensityMin = 0f;

		public const float NoiseIntensityMax = 1f;

		public const float NoiseIntensityDefault = 0.5f;

		public const float NoiseScaleMin = 0.01f;

		public const float NoiseScaleMax = 2f;

		public const float NoiseScaleDefault = 0.5f;

		public static readonly Vector3 NoiseVelocityDefault = new Vector3(0.07f, 0.18f, 0.05f);

		public const BlendingMode BlendingModeDefault = BlendingMode.Additive;

		public const ShaderAccuracy ShaderAccuracyDefault = ShaderAccuracy.Fast;

		public const float FadeOutBeginDefault = -150f;

		public const float FadeOutEndDefault = -200f;

		public const Dimensions DimensionsDefault = Dimensions.Dim3D;
	}

	public static class DustParticles
	{
		public const float AlphaDefault = 0.5f;

		public const float SizeDefault = 0.01f;

		public const ParticlesDirection DirectionDefault = ParticlesDirection.Random;

		public static readonly Vector3 VelocityDefault = new Vector3(0f, 0f, 0.03f);

		public const float DensityDefault = 5f;

		public const float DensityMin = 0f;

		public const float DensityMax = 1000f;

		public static readonly MinMaxRangeFloat SpawnDistanceRangeDefault = new MinMaxRangeFloat(0f, 0.7f);

		public const bool CullingEnabledDefault = false;

		public const float CullingMaxDistanceDefault = 10f;

		public const float CullingMaxDistanceMin = 1f;
	}

	public static class DynOcclusion
	{
		public static readonly LayerMask LayerMaskDefault = 1;

		public const DynamicOcclusionUpdateRate UpdateRateDefault = DynamicOcclusionUpdateRate.EveryXFrames;

		public const int WaitFramesCountDefault = 3;

		public const Dimensions RaycastingDimensionsDefault = Dimensions.Dim3D;

		public const bool RaycastingConsiderTriggersDefault = false;

		public const float RaycastingMinOccluderAreaDefault = 0f;

		public const float RaycastingMinSurfaceRatioDefault = 0.5f;

		public const float RaycastingMinSurfaceRatioMin = 50f;

		public const float RaycastingMinSurfaceRatioMax = 100f;

		public const float RaycastingMaxSurfaceDotDefault = 0.25f;

		public const float RaycastingMaxSurfaceAngleMin = 45f;

		public const float RaycastingMaxSurfaceAngleMax = 90f;

		public const PlaneAlignment RaycastingPlaneAlignmentDefault = PlaneAlignment.Surface;

		public const float RaycastingPlaneOffsetDefault = 0.1f;

		public const float RaycastingFadeDistanceToSurfaceDefault = 0.25f;

		public const int DepthBufferDepthMapResolutionDefault = 128;

		public const bool DepthBufferOcclusionCullingDefault = true;

		public const float DepthBufferFadeDistanceToSurfaceDefault = 0f;
	}

	public static class Effects
	{
		public const EffectAbstractBase.ComponentsToChange ComponentsToChangeDefault = (EffectAbstractBase.ComponentsToChange)2147483647;

		public const bool RestoreIntensityOnDisableDefault = true;

		public const float FrequencyDefault = 10f;

		public const bool PerformPausesDefault = false;

		public const bool RestoreIntensityOnPauseDefault = false;

		public static readonly MinMaxRangeFloat FlickeringDurationDefault = new MinMaxRangeFloat(1f, 4f);

		public static readonly MinMaxRangeFloat PauseDurationDefault = new MinMaxRangeFloat(0f, 1f);

		public static readonly MinMaxRangeFloat IntensityAmplitudeDefault = new MinMaxRangeFloat(-1f, 1f);

		public const float SmoothingDefault = 0.05f;
	}

	public static class Shadow
	{
		public const float StrengthDefault = 1f;

		public const float StrengthMin = 0f;

		public const float StrengthMax = 1f;

		public static readonly LayerMask LayerMaskDefault = 1;

		public const ShadowUpdateRate UpdateRateDefault = ShadowUpdateRate.EveryXFrames;

		public const int WaitFramesCountDefault = 3;

		public const int DepthMapResolutionDefault = 128;

		public const bool OcclusionCullingDefault = true;

		public static string GetErrorChangeRuntimeDepthMapResolution(VolumetricShadowHD comp)
		{
			return $"Can't change {comp.name} Shadow.depthMapResolution property at runtime after DepthCamera initialization";
		}
	}

	public static class Cookie
	{
		public const float ContributionDefault = 1f;

		public const float ContributionMin = 0f;

		public const float ContributionMax = 1f;

		public const Texture CookieTextureDefault = null;

		public const CookieChannel ChannelDefault = CookieChannel.Alpha;

		public const bool NegativeDefault = false;

		public static readonly Vector2 TranslationDefault = Vector2.zero;

		public const float RotationDefault = 0f;

		public static readonly Vector2 ScaleDefault = Vector2.one;
	}

	public static class Config
	{
		public static class HD
		{
			public const RenderQueue GeometryRenderQueueDefault = (RenderQueue)3100;

			public const float CameraBlendingDistance = 0.5f;

			public const int RaymarchingQualitiesStepsMin = 2;
		}

		public const bool GeometryOverrideLayerDefault = true;

		public const int GeometryLayerIDDefault = 1;

		public const string GeometryTagDefault = "Untagged";

		public const string FadeOutCameraTagDefault = "MainCamera";

		public const RenderQueue GeometryRenderQueueDefault = RenderQueue.Transparent;

		public const RenderPipeline GeometryRenderPipelineDefault = RenderPipeline.BuiltIn;

		public const RenderingMode GeometryRenderingModeDefault = RenderingMode.Default;

		public const int Noise3DSizeDefault = 64;

		public const float DitheringFactor = 0f;

		public const bool UseLightColorTemperatureDefault = true;

		public const bool FeatureEnabledDefault = true;

		public const FeatureEnabledColorGradient FeatureEnabledColorGradientDefault = FeatureEnabledColorGradient.HighOnly;

		public const int SharedMeshSidesDefault = 24;

		public const int SharedMeshSidesMin = 3;

		public const int SharedMeshSidesMax = 256;

		public const int SharedMeshSegmentsDefault = 5;

		public const int SharedMeshSegmentsMin = 0;

		public const int SharedMeshSegmentsMax = 64;
	}

	public const string PluginFolder = "VolumetricLightBeam";
}
