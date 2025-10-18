using System;
using UnityEngine;

namespace VLB;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[SelectionBase]
[HelpURL("http://saladgamer.com/vlb-doc/comp-lightbeam-hd/")]
[AddComponentMenu("VLB/HD/Volumetric Light Beam HD")]
public class VolumetricLightBeamHD : VolumetricLightBeamAbstractBase
{
	public new const string ClassName = "VolumetricLightBeamHD";

	[SerializeField]
	private bool m_ColorFromLight = true;

	[SerializeField]
	private ColorMode m_ColorMode;

	[SerializeField]
	private Color m_ColorFlat = Consts.Beam.FlatColor;

	[SerializeField]
	private Gradient m_ColorGradient;

	[SerializeField]
	private BlendingMode m_BlendingMode;

	[SerializeField]
	private float m_Intensity = 1f;

	[SerializeField]
	private float m_IntensityMultiplier = 1f;

	[SerializeField]
	private float m_HDRPExposureWeight;

	[SerializeField]
	private float m_SpotAngle = 35f;

	[SerializeField]
	private float m_SpotAngleMultiplier = 1f;

	[SerializeField]
	private float m_ConeRadiusStart = 0.1f;

	[SerializeField]
	private bool m_Scalable = true;

	[SerializeField]
	private float m_FallOffStart;

	[SerializeField]
	private float m_FallOffEnd = 3f;

	[SerializeField]
	private float m_FallOffEndMultiplier = 1f;

	[SerializeField]
	private AttenuationEquationHD m_AttenuationEquation = AttenuationEquationHD.Quadratic;

	[SerializeField]
	private float m_SideSoftness = 1f;

	[SerializeField]
	private int m_RaymarchingQualityID = -1;

	[SerializeField]
	private float m_JitteringFactor;

	[SerializeField]
	private int m_JitteringFrameRate = 60;

	[MinMaxRange(0f, 1f)]
	[SerializeField]
	private MinMaxRangeFloat m_JitteringLerpRange = Consts.Beam.HD.JitteringLerpRange;

	[SerializeField]
	private NoiseMode m_NoiseMode;

	[SerializeField]
	private float m_NoiseIntensity = 0.5f;

	[SerializeField]
	private bool m_NoiseScaleUseGlobal = true;

	[SerializeField]
	private float m_NoiseScaleLocal = 0.5f;

	[SerializeField]
	private bool m_NoiseVelocityUseGlobal = true;

	[SerializeField]
	private Vector3 m_NoiseVelocityLocal = Consts.Beam.NoiseVelocityDefault;

	protected BeamGeometryHD m_BeamGeom;

	public bool colorFromLight
	{
		get
		{
			return m_ColorFromLight;
		}
		set
		{
			if (m_ColorFromLight != value)
			{
				m_ColorFromLight = value;
				ValidateProperties();
			}
		}
	}

	public ColorMode colorMode
	{
		get
		{
			if (Config.Instance.featureEnabledColorGradient == FeatureEnabledColorGradient.Off)
			{
				return ColorMode.Flat;
			}
			return m_ColorMode;
		}
		set
		{
			if (m_ColorMode != value)
			{
				m_ColorMode = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.ColorMode);
			}
		}
	}

	public Color colorFlat
	{
		get
		{
			return m_ColorFlat;
		}
		set
		{
			if (m_ColorFlat != value)
			{
				m_ColorFlat = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.Color);
			}
		}
	}

	public Gradient colorGradient
	{
		get
		{
			return m_ColorGradient;
		}
		set
		{
			if (m_ColorGradient != value)
			{
				m_ColorGradient = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.Color);
			}
		}
	}

	private bool useColorFromAttachedLightSpot
	{
		get
		{
			if (colorFromLight)
			{
				return base.lightSpotAttached != null;
			}
			return false;
		}
	}

	private bool useColorTemperatureFromAttachedLightSpot
	{
		get
		{
			if (useColorFromAttachedLightSpot && base.lightSpotAttached.useColorTemperature)
			{
				return Config.Instance.useLightColorTemperature;
			}
			return false;
		}
	}

	public float intensity
	{
		get
		{
			return m_Intensity;
		}
		set
		{
			if (m_Intensity != value)
			{
				m_Intensity = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.Intensity);
			}
		}
	}

	public float intensityMultiplier
	{
		get
		{
			return m_IntensityMultiplier;
		}
		set
		{
			if (m_IntensityMultiplier != value)
			{
				m_IntensityMultiplier = value;
				ValidateProperties();
			}
		}
	}

	public bool useIntensityFromAttachedLightSpot
	{
		get
		{
			if (intensityMultiplier >= 0f)
			{
				return base.lightSpotAttached != null;
			}
			return false;
		}
		set
		{
			intensityMultiplier = (value ? 1f : (-1f)) * Mathf.Abs(intensityMultiplier);
		}
	}

	public float hdrpExposureWeight
	{
		get
		{
			return m_HDRPExposureWeight;
		}
		set
		{
			if (m_HDRPExposureWeight != value)
			{
				m_HDRPExposureWeight = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.HDRPExposureWeight);
			}
		}
	}

	public BlendingMode blendingMode
	{
		get
		{
			return m_BlendingMode;
		}
		set
		{
			if (m_BlendingMode != value)
			{
				m_BlendingMode = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.BlendingMode);
			}
		}
	}

	public float spotAngle
	{
		get
		{
			return m_SpotAngle;
		}
		set
		{
			if (m_SpotAngle != value)
			{
				m_SpotAngle = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.Cone);
			}
		}
	}

	public float spotAngleMultiplier
	{
		get
		{
			return m_SpotAngleMultiplier;
		}
		set
		{
			if (m_SpotAngleMultiplier != value)
			{
				m_SpotAngleMultiplier = value;
				ValidateProperties();
			}
		}
	}

	public bool useSpotAngleFromAttachedLightSpot
	{
		get
		{
			if (spotAngleMultiplier >= 0f)
			{
				return base.lightSpotAttached != null;
			}
			return false;
		}
		set
		{
			spotAngleMultiplier = (value ? 1f : (-1f)) * Mathf.Abs(spotAngleMultiplier);
		}
	}

	public float coneAngle => Mathf.Atan2(coneRadiusEnd - coneRadiusStart, maxGeometryDistance) * 57.29578f * 2f;

	public float coneRadiusStart
	{
		get
		{
			return m_ConeRadiusStart;
		}
		set
		{
			if (m_ConeRadiusStart != value)
			{
				m_ConeRadiusStart = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.Cone);
			}
		}
	}

	public float coneRadiusEnd
	{
		get
		{
			return Utils.ComputeConeRadiusEnd(maxGeometryDistance, spotAngle);
		}
		set
		{
			spotAngle = Utils.ComputeSpotAngle(maxGeometryDistance, value);
		}
	}

	public float coneVolume
	{
		get
		{
			float num = coneRadiusStart;
			float num2 = coneRadiusEnd;
			return MathF.PI / 3f * (num * num + num * num2 + num2 * num2) * fallOffEnd;
		}
	}

	public bool scalable
	{
		get
		{
			return m_Scalable;
		}
		set
		{
			if (m_Scalable != value)
			{
				m_Scalable = value;
				SetPropertyDirty(DirtyProps.Attenuation);
			}
		}
	}

	public AttenuationEquationHD attenuationEquation
	{
		get
		{
			return m_AttenuationEquation;
		}
		set
		{
			if (m_AttenuationEquation != value)
			{
				m_AttenuationEquation = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.Attenuation);
			}
		}
	}

	public float fallOffStart
	{
		get
		{
			return m_FallOffStart;
		}
		set
		{
			if (m_FallOffStart != value)
			{
				m_FallOffStart = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.Cone);
			}
		}
	}

	public float fallOffEnd
	{
		get
		{
			return m_FallOffEnd;
		}
		set
		{
			if (m_FallOffEnd != value)
			{
				m_FallOffEnd = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.Cone);
			}
		}
	}

	public float maxGeometryDistance => fallOffEnd;

	public float fallOffEndMultiplier
	{
		get
		{
			return m_FallOffEndMultiplier;
		}
		set
		{
			if (m_FallOffEndMultiplier != value)
			{
				m_FallOffEndMultiplier = value;
				ValidateProperties();
			}
		}
	}

	public bool useFallOffEndFromAttachedLightSpot
	{
		get
		{
			if (fallOffEndMultiplier >= 0f)
			{
				return base.lightSpotAttached != null;
			}
			return false;
		}
		set
		{
			fallOffEndMultiplier = (value ? 1f : (-1f)) * Mathf.Abs(fallOffEndMultiplier);
		}
	}

	public float sideSoftness
	{
		get
		{
			return m_SideSoftness;
		}
		set
		{
			if (m_SideSoftness != value)
			{
				m_SideSoftness = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.SideSoftness);
			}
		}
	}

	public float jitteringFactor
	{
		get
		{
			return m_JitteringFactor;
		}
		set
		{
			if (m_JitteringFactor != value)
			{
				m_JitteringFactor = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.Jittering);
			}
		}
	}

	public int jitteringFrameRate
	{
		get
		{
			return m_JitteringFrameRate;
		}
		set
		{
			if (m_JitteringFrameRate != value)
			{
				m_JitteringFrameRate = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.Jittering);
			}
		}
	}

	public MinMaxRangeFloat jitteringLerpRange
	{
		get
		{
			return m_JitteringLerpRange;
		}
		set
		{
			if (m_JitteringLerpRange != value)
			{
				m_JitteringLerpRange = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.Jittering);
			}
		}
	}

	public NoiseMode noiseMode
	{
		get
		{
			return m_NoiseMode;
		}
		set
		{
			if (m_NoiseMode != value)
			{
				m_NoiseMode = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.NoiseMode);
			}
		}
	}

	public bool isNoiseEnabled => noiseMode != NoiseMode.Disabled;

	public float noiseIntensity
	{
		get
		{
			return m_NoiseIntensity;
		}
		set
		{
			if (m_NoiseIntensity != value)
			{
				m_NoiseIntensity = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.NoiseIntensity);
			}
		}
	}

	public bool noiseScaleUseGlobal
	{
		get
		{
			return m_NoiseScaleUseGlobal;
		}
		set
		{
			if (m_NoiseScaleUseGlobal != value)
			{
				m_NoiseScaleUseGlobal = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.NoiseVelocityAndScale);
			}
		}
	}

	public float noiseScaleLocal
	{
		get
		{
			return m_NoiseScaleLocal;
		}
		set
		{
			if (m_NoiseScaleLocal != value)
			{
				m_NoiseScaleLocal = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.NoiseVelocityAndScale);
			}
		}
	}

	public bool noiseVelocityUseGlobal
	{
		get
		{
			return m_NoiseVelocityUseGlobal;
		}
		set
		{
			if (m_NoiseVelocityUseGlobal != value)
			{
				m_NoiseVelocityUseGlobal = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.NoiseVelocityAndScale);
			}
		}
	}

	public Vector3 noiseVelocityLocal
	{
		get
		{
			return m_NoiseVelocityLocal;
		}
		set
		{
			if (m_NoiseVelocityLocal != value)
			{
				m_NoiseVelocityLocal = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.NoiseVelocityAndScale);
			}
		}
	}

	public int raymarchingQualityID
	{
		get
		{
			return m_RaymarchingQualityID;
		}
		set
		{
			if (m_RaymarchingQualityID != value)
			{
				m_RaymarchingQualityID = value;
				ValidateProperties();
				SetPropertyDirty(DirtyProps.RaymarchingQuality);
			}
		}
	}

	public int raymarchingQualityIndex
	{
		get
		{
			return Config.Instance.GetRaymarchingQualityIndexForUniqueID(raymarchingQualityID);
		}
		set
		{
			raymarchingQualityID = Config.Instance.GetRaymarchingQualityForIndex(raymarchingQualityIndex).uniqueID;
		}
	}

	public int blendingModeAsInt => Mathf.Clamp((int)blendingMode, 0, Enum.GetValues(typeof(BlendingMode)).Length);

	public Quaternion beamInternalLocalRotation
	{
		get
		{
			if (GetDimensions() != Dimensions.Dim3D)
			{
				return Quaternion.LookRotation(Vector3.right, Vector3.up);
			}
			return Quaternion.identity;
		}
	}

	public Vector3 beamLocalForward
	{
		get
		{
			if (GetDimensions() != Dimensions.Dim3D)
			{
				return Vector3.right;
			}
			return Vector3.forward;
		}
	}

	public Vector3 beamGlobalForward => base.transform.TransformDirection(beamLocalForward);

	public uint _INTERNAL_InstancedMaterialGroupID { get; protected set; }

	public float GetConeApexOffsetZ(bool counterApplyScaleForUnscalableBeam)
	{
		float num = coneRadiusStart / coneRadiusEnd;
		if (num == 1f)
		{
			return float.MaxValue;
		}
		float num2 = maxGeometryDistance * num / (1f - num);
		if (counterApplyScaleForUnscalableBeam && !scalable)
		{
			num2 /= GetLossyScale().z;
		}
		return num2;
	}

	public override bool IsScalable()
	{
		return scalable;
	}

	public override BeamGeometryAbstractBase GetBeamGeometry()
	{
		return m_BeamGeom;
	}

	protected override void SetBeamGeometryNull()
	{
		m_BeamGeom = null;
	}

	public override Vector3 GetLossyScale()
	{
		if (GetDimensions() != Dimensions.Dim3D)
		{
			return new Vector3(base.transform.lossyScale.z, base.transform.lossyScale.y, base.transform.lossyScale.x);
		}
		return base.transform.lossyScale;
	}

	public VolumetricCookieHD GetAdditionalComponentCookie()
	{
		return GetComponent<VolumetricCookieHD>();
	}

	public VolumetricShadowHD GetAdditionalComponentShadow()
	{
		return GetComponent<VolumetricShadowHD>();
	}

	public void SetPropertyDirty(DirtyProps flags)
	{
		if ((bool)m_BeamGeom)
		{
			m_BeamGeom.SetPropertyDirty(flags);
		}
	}

	public virtual Dimensions GetDimensions()
	{
		return Dimensions.Dim3D;
	}

	public virtual bool DoesSupportSorting2D()
	{
		return false;
	}

	public virtual int GetSortingLayerID()
	{
		return 0;
	}

	public virtual int GetSortingOrder()
	{
		return 0;
	}

	public float GetInsideBeamFactor(Vector3 posWS)
	{
		return GetInsideBeamFactorFromObjectSpacePos(base.transform.InverseTransformPoint(posWS));
	}

	public float GetInsideBeamFactorFromObjectSpacePos(Vector3 posOS)
	{
		if (GetDimensions() == Dimensions.Dim2D)
		{
			posOS = new Vector3(posOS.z, posOS.y, posOS.x);
		}
		if (posOS.z < 0f)
		{
			return -1f;
		}
		Vector2 normalized = new Vector2(posOS.xy().magnitude, posOS.z + GetConeApexOffsetZ(counterApplyScaleForUnscalableBeam: true)).normalized;
		return Mathf.Clamp((Mathf.Abs(Mathf.Sin(coneAngle * (MathF.PI / 180f) / 2f)) - Mathf.Abs(normalized.x)) / 0.1f, -1f, 1f);
	}

	public override void GenerateGeometry()
	{
		if (pluginVersion == -1)
		{
			raymarchingQualityID = Config.Instance.defaultRaymarchingQualityUniqueID;
		}
		if (!Config.Instance.IsRaymarchingQualityUniqueIDValid(raymarchingQualityID))
		{
			Debug.LogErrorFormat(base.gameObject, "HD Beam '{0}': fallback to default quality '{1}'", base.name, Config.Instance.GetRaymarchingQualityForUniqueID(Config.Instance.defaultRaymarchingQualityUniqueID).name);
			raymarchingQualityID = Config.Instance.defaultRaymarchingQualityUniqueID;
			Utils.MarkCurrentSceneDirty();
		}
		HandleBackwardCompatibility(pluginVersion, 20200);
		pluginVersion = 20200;
		ValidateProperties();
		if (m_BeamGeom == null)
		{
			m_BeamGeom = Utils.NewWithComponent<BeamGeometryHD>("Beam Geometry");
			m_BeamGeom.Initialize(this);
		}
		m_BeamGeom.RegenerateMesh();
		m_BeamGeom.visible = base.enabled;
		base.GenerateGeometry();
	}

	public virtual void UpdateAfterManualPropertyChange()
	{
		ValidateProperties();
		SetPropertyDirty(DirtyProps.All);
	}

	private void Start()
	{
		InitLightSpotAttachedCached();
		GenerateGeometry();
	}

	private void OnEnable()
	{
		if ((bool)m_BeamGeom)
		{
			m_BeamGeom.visible = true;
		}
	}

	private void OnDisable()
	{
		if ((bool)m_BeamGeom)
		{
			m_BeamGeom.visible = false;
		}
	}

	private void OnDidApplyAnimationProperties()
	{
		AssignPropertiesFromAttachedSpotLight();
		UpdateAfterManualPropertyChange();
	}

	public void AssignPropertiesFromAttachedSpotLight()
	{
		Light light = base.lightSpotAttached;
		if (!light)
		{
			return;
		}
		if (useIntensityFromAttachedLightSpot)
		{
			intensity = SpotLightHelper.GetIntensity(light) * intensityMultiplier;
		}
		if (useFallOffEndFromAttachedLightSpot)
		{
			fallOffEnd = SpotLightHelper.GetFallOffEnd(light) * fallOffEndMultiplier;
		}
		if (useSpotAngleFromAttachedLightSpot)
		{
			spotAngle = Mathf.Clamp(SpotLightHelper.GetSpotAngle(light) * spotAngleMultiplier, 0.1f, 179.9f);
		}
		if (m_ColorFromLight)
		{
			colorMode = ColorMode.Flat;
			if (useColorTemperatureFromAttachedLightSpot)
			{
				Color color = Mathf.CorrelatedColorTemperatureToRGB(light.colorTemperature);
				colorFlat = (light.color.linear * color).gamma;
			}
			else
			{
				colorFlat = light.color;
			}
		}
	}

	private void ClampProperties()
	{
		m_Intensity = Mathf.Max(m_Intensity, 0f);
		m_FallOffEnd = Mathf.Max(0.01f, m_FallOffEnd);
		m_FallOffStart = Mathf.Clamp(m_FallOffStart, 0f, m_FallOffEnd - 0.01f);
		m_SpotAngle = Mathf.Clamp(m_SpotAngle, 0.1f, 179.9f);
		m_ConeRadiusStart = Mathf.Max(m_ConeRadiusStart, 0f);
		m_SideSoftness = Mathf.Clamp(m_SideSoftness, 0.0001f, 10f);
		m_JitteringFactor = Mathf.Max(m_JitteringFactor, 0f);
		m_JitteringFrameRate = Mathf.Clamp(m_JitteringFrameRate, 0, 120);
		m_NoiseIntensity = Mathf.Clamp(m_NoiseIntensity, 0f, 1f);
	}

	private void ValidateProperties()
	{
		AssignPropertiesFromAttachedSpotLight();
		ClampProperties();
	}

	private void HandleBackwardCompatibility(int serializedVersion, int newVersion)
	{
		if (serializedVersion != -1 && serializedVersion != newVersion)
		{
			Utils.MarkCurrentSceneDirty();
		}
	}
}
