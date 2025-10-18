using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace VLB;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[SelectionBase]
[HelpURL("http://saladgamer.com/vlb-doc/comp-lightbeam-sd/")]
[AddComponentMenu("VLB/SD/Volumetric Light Beam SD")]
public class VolumetricLightBeamSD : VolumetricLightBeamAbstractBase
{
	public delegate void OnWillCameraRenderCB(Camera cam);

	public delegate void OnBeamGeometryInitialized();

	public new const string ClassName = "VolumetricLightBeamSD";

	public bool colorFromLight = true;

	public ColorMode colorMode;

	[ColorUsage(false, true)]
	[FormerlySerializedAs("colorValue")]
	public Color color = Consts.Beam.FlatColor;

	public Gradient colorGradient;

	public bool intensityFromLight = true;

	public bool intensityModeAdvanced;

	[FormerlySerializedAs("alphaInside")]
	[Min(0f)]
	public float intensityInside = 1f;

	[FormerlySerializedAs("alphaOutside")]
	[FormerlySerializedAs("alpha")]
	[Min(0f)]
	public float intensityOutside = 1f;

	[Min(0f)]
	public float intensityMultiplier = 1f;

	[Range(0f, 1f)]
	public float hdrpExposureWeight;

	public BlendingMode blendingMode;

	[FormerlySerializedAs("angleFromLight")]
	public bool spotAngleFromLight = true;

	[Range(0.1f, 179.9f)]
	public float spotAngle = 35f;

	[Min(0f)]
	public float spotAngleMultiplier = 1f;

	[FormerlySerializedAs("radiusStart")]
	public float coneRadiusStart = 0.1f;

	public ShaderAccuracy shaderAccuracy;

	public MeshType geomMeshType;

	[FormerlySerializedAs("geomSides")]
	public int geomCustomSides = 18;

	public int geomCustomSegments = 5;

	public Vector3 skewingLocalForwardDirection = Consts.Beam.SD.SkewingLocalForwardDirectionDefault;

	public Transform clippingPlaneTransform;

	public bool geomCap;

	public AttenuationEquation attenuationEquation = AttenuationEquation.Quadratic;

	[Range(0f, 1f)]
	public float attenuationCustomBlending = 0.5f;

	[FormerlySerializedAs("fadeStart")]
	public float fallOffStart;

	[FormerlySerializedAs("fadeEnd")]
	public float fallOffEnd = 3f;

	[FormerlySerializedAs("fadeEndFromLight")]
	public bool fallOffEndFromLight = true;

	[Min(0f)]
	public float fallOffEndMultiplier = 1f;

	public float depthBlendDistance = 2f;

	public float cameraClippingDistance = 0.5f;

	[Range(0f, 1f)]
	public float glareFrontal = 0.5f;

	[Range(0f, 1f)]
	public float glareBehind = 0.5f;

	[FormerlySerializedAs("fresnelPowOutside")]
	public float fresnelPow = 8f;

	public NoiseMode noiseMode;

	[Range(0f, 1f)]
	public float noiseIntensity = 0.5f;

	public bool noiseScaleUseGlobal = true;

	[Range(0.01f, 2f)]
	public float noiseScaleLocal = 0.5f;

	public bool noiseVelocityUseGlobal = true;

	public Vector3 noiseVelocityLocal = Consts.Beam.NoiseVelocityDefault;

	public Dimensions dimensions;

	public Vector2 tiltFactor = Consts.Beam.SD.TiltDefault;

	private MaterialManager.SD.DynamicOcclusion m_INTERNAL_DynamicOcclusionMode;

	private bool m_INTERNAL_DynamicOcclusionMode_Runtime;

	private OnBeamGeometryInitialized m_OnBeamGeometryInitialized;

	[FormerlySerializedAs("trackChangesDuringPlaytime")]
	[SerializeField]
	private bool _TrackChangesDuringPlaytime;

	[SerializeField]
	private int _SortingLayerID;

	[SerializeField]
	private int _SortingOrder;

	[FormerlySerializedAs("fadeOutBegin")]
	[SerializeField]
	private float _FadeOutBegin = -150f;

	[FormerlySerializedAs("fadeOutEnd")]
	[SerializeField]
	private float _FadeOutEnd = -200f;

	private BeamGeometrySD m_BeamGeom;

	private Coroutine m_CoPlaytimeUpdate;

	public ColorMode usedColorMode
	{
		get
		{
			if (Config.Instance.featureEnabledColorGradient == FeatureEnabledColorGradient.Off)
			{
				return ColorMode.Flat;
			}
			return colorMode;
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

	[Obsolete("Use 'intensityGlobal' or 'intensityInside' instead")]
	public float alphaInside
	{
		get
		{
			return intensityInside;
		}
		set
		{
			intensityInside = value;
		}
	}

	[Obsolete("Use 'intensityGlobal' or 'intensityOutside' instead")]
	public float alphaOutside
	{
		get
		{
			return intensityOutside;
		}
		set
		{
			intensityOutside = value;
		}
	}

	public float intensityGlobal
	{
		get
		{
			return intensityOutside;
		}
		set
		{
			intensityInside = value;
			intensityOutside = value;
		}
	}

	public bool useIntensityFromAttachedLightSpot
	{
		get
		{
			if (intensityFromLight)
			{
				return base.lightSpotAttached != null;
			}
			return false;
		}
	}

	public bool useSpotAngleFromAttachedLightSpot
	{
		get
		{
			if (spotAngleFromLight)
			{
				return base.lightSpotAttached != null;
			}
			return false;
		}
	}

	public float coneAngle => Mathf.Atan2(coneRadiusEnd - coneRadiusStart, maxGeometryDistance) * 57.29578f * 2f;

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

	public float coneApexOffsetZ
	{
		get
		{
			float num = coneRadiusStart / coneRadiusEnd;
			if (num != 1f)
			{
				return maxGeometryDistance * num / (1f - num);
			}
			return float.MaxValue;
		}
	}

	public Vector3 coneApexPositionLocal => new Vector3(0f, 0f, 0f - coneApexOffsetZ);

	public Vector3 coneApexPositionGlobal => base.transform.localToWorldMatrix.MultiplyPoint(coneApexPositionLocal);

	public int geomSides
	{
		get
		{
			if (geomMeshType != MeshType.Custom)
			{
				return Config.Instance.sharedMeshSides;
			}
			return geomCustomSides;
		}
		set
		{
			geomCustomSides = value;
			Debug.LogWarningFormat("The setter VLB.{0}.geomSides is OBSOLETE and has been renamed to geomCustomSides.", "VolumetricLightBeamSD");
		}
	}

	public int geomSegments
	{
		get
		{
			if (geomMeshType != MeshType.Custom)
			{
				return Config.Instance.sharedMeshSegments;
			}
			return geomCustomSegments;
		}
		set
		{
			geomCustomSegments = value;
			Debug.LogWarningFormat("The setter VLB.{0}.geomSegments is OBSOLETE and has been renamed to geomCustomSegments.", "VolumetricLightBeamSD");
		}
	}

	public Vector3 skewingLocalForwardDirectionNormalized
	{
		get
		{
			if (Mathf.Approximately(skewingLocalForwardDirection.z, 0f))
			{
				Debug.LogErrorFormat("Beam {0} has a skewingLocalForwardDirection with a null Z, which is forbidden", base.name);
				return Vector3.forward;
			}
			return skewingLocalForwardDirection.normalized;
		}
	}

	public bool canHaveMeshSkewing => geomMeshType == MeshType.Custom;

	public bool hasMeshSkewing
	{
		get
		{
			if (!Config.Instance.featureEnabledMeshSkewing)
			{
				return false;
			}
			if (!canHaveMeshSkewing)
			{
				return false;
			}
			if (Mathf.Approximately(Vector3.Dot(skewingLocalForwardDirectionNormalized, Vector3.forward), 1f))
			{
				return false;
			}
			return true;
		}
	}

	public Vector4 additionalClippingPlane
	{
		get
		{
			if (!(clippingPlaneTransform == null))
			{
				return Utils.PlaneEquation(clippingPlaneTransform.forward, clippingPlaneTransform.position);
			}
			return Vector4.zero;
		}
	}

	public float attenuationLerpLinearQuad
	{
		get
		{
			if (attenuationEquation == AttenuationEquation.Linear)
			{
				return 0f;
			}
			if (attenuationEquation == AttenuationEquation.Quadratic)
			{
				return 1f;
			}
			return attenuationCustomBlending;
		}
	}

	[Obsolete("Use 'fallOffStart' instead")]
	public float fadeStart
	{
		get
		{
			return fallOffStart;
		}
		set
		{
			fallOffStart = value;
		}
	}

	[Obsolete("Use 'fallOffEnd' instead")]
	public float fadeEnd
	{
		get
		{
			return fallOffEnd;
		}
		set
		{
			fallOffEnd = value;
		}
	}

	[Obsolete("Use 'fallOffEndFromLight' instead")]
	public bool fadeEndFromLight
	{
		get
		{
			return fallOffEndFromLight;
		}
		set
		{
			fallOffEndFromLight = value;
		}
	}

	public bool useFallOffEndFromAttachedLightSpot
	{
		get
		{
			if (fallOffEndFromLight)
			{
				return base.lightSpotAttached != null;
			}
			return false;
		}
	}

	public float maxGeometryDistance => fallOffEnd + Mathf.Max(Mathf.Abs(tiltFactor.x), Mathf.Abs(tiltFactor.y));

	public bool isNoiseEnabled => noiseMode != NoiseMode.Disabled;

	[Obsolete("Use 'noiseMode' instead")]
	public bool noiseEnabled
	{
		get
		{
			return isNoiseEnabled;
		}
		set
		{
			noiseMode = (value ? NoiseMode.WorldSpace : NoiseMode.Disabled);
		}
	}

	public float fadeOutBegin
	{
		get
		{
			return _FadeOutBegin;
		}
		set
		{
			SetFadeOutValue(ref _FadeOutBegin, value);
		}
	}

	public float fadeOutEnd
	{
		get
		{
			return _FadeOutEnd;
		}
		set
		{
			SetFadeOutValue(ref _FadeOutEnd, value);
		}
	}

	public bool isFadeOutEnabled
	{
		get
		{
			if (_FadeOutBegin >= 0f)
			{
				return _FadeOutEnd >= 0f;
			}
			return false;
		}
	}

	public bool isTilted => !tiltFactor.Approximately(Vector2.zero);

	public int sortingLayerID
	{
		get
		{
			return _SortingLayerID;
		}
		set
		{
			_SortingLayerID = value;
			if ((bool)m_BeamGeom)
			{
				m_BeamGeom.sortingLayerID = value;
			}
		}
	}

	public string sortingLayerName
	{
		get
		{
			return SortingLayer.IDToName(sortingLayerID);
		}
		set
		{
			sortingLayerID = SortingLayer.NameToID(value);
		}
	}

	public int sortingOrder
	{
		get
		{
			return _SortingOrder;
		}
		set
		{
			_SortingOrder = value;
			if ((bool)m_BeamGeom)
			{
				m_BeamGeom.sortingOrder = value;
			}
		}
	}

	public bool trackChangesDuringPlaytime
	{
		get
		{
			return _TrackChangesDuringPlaytime;
		}
		set
		{
			_TrackChangesDuringPlaytime = value;
			StartPlaytimeUpdateIfNeeded();
		}
	}

	public bool isCurrentlyTrackingChanges => m_CoPlaytimeUpdate != null;

	public int blendingModeAsInt => Mathf.Clamp((int)blendingMode, 0, Enum.GetValues(typeof(BlendingMode)).Length);

	public Quaternion beamInternalLocalRotation
	{
		get
		{
			if (dimensions != Dimensions.Dim3D)
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
			if (dimensions != Dimensions.Dim3D)
			{
				return Vector3.right;
			}
			return Vector3.forward;
		}
	}

	public Vector3 beamGlobalForward => base.transform.TransformDirection(beamLocalForward);

	public float raycastDistance
	{
		get
		{
			if (!hasMeshSkewing)
			{
				return maxGeometryDistance;
			}
			float z = skewingLocalForwardDirectionNormalized.z;
			if (!Mathf.Approximately(z, 0f))
			{
				return maxGeometryDistance / z;
			}
			return maxGeometryDistance;
		}
	}

	public Vector3 raycastGlobalForward => ComputeRaycastGlobalVector(hasMeshSkewing ? skewingLocalForwardDirectionNormalized : Vector3.forward);

	public Vector3 raycastGlobalUp => ComputeRaycastGlobalVector(Vector3.up);

	public Vector3 raycastGlobalRight => ComputeRaycastGlobalVector(Vector3.right);

	public MaterialManager.SD.DynamicOcclusion _INTERNAL_DynamicOcclusionMode
	{
		get
		{
			if (!Config.Instance.featureEnabledDynamicOcclusion)
			{
				return MaterialManager.SD.DynamicOcclusion.Off;
			}
			return m_INTERNAL_DynamicOcclusionMode;
		}
		set
		{
			m_INTERNAL_DynamicOcclusionMode = value;
		}
	}

	public MaterialManager.SD.DynamicOcclusion _INTERNAL_DynamicOcclusionMode_Runtime
	{
		get
		{
			if (!m_INTERNAL_DynamicOcclusionMode_Runtime)
			{
				return MaterialManager.SD.DynamicOcclusion.Off;
			}
			return _INTERNAL_DynamicOcclusionMode;
		}
	}

	public uint _INTERNAL_InstancedMaterialGroupID { get; protected set; }

	public string meshStats
	{
		get
		{
			Mesh mesh = (m_BeamGeom ? m_BeamGeom.coneMesh : null);
			if ((bool)mesh)
			{
				return $"Cone angle: {coneAngle:0.0} degrees\nMesh: {mesh.vertexCount} vertices, {mesh.triangles.Length / 3} triangles";
			}
			return "no mesh available";
		}
	}

	public int meshVerticesCount
	{
		get
		{
			if (!m_BeamGeom || !m_BeamGeom.coneMesh)
			{
				return 0;
			}
			return m_BeamGeom.coneMesh.vertexCount;
		}
	}

	public int meshTrianglesCount
	{
		get
		{
			if (!m_BeamGeom || !m_BeamGeom.coneMesh)
			{
				return 0;
			}
			return m_BeamGeom.coneMesh.triangles.Length / 3;
		}
	}

	public event OnWillCameraRenderCB onWillCameraRenderThisBeam;

	public void GetInsideAndOutsideIntensity(out float inside, out float outside)
	{
		if (intensityModeAdvanced)
		{
			inside = intensityInside;
			outside = intensityOutside;
		}
		else
		{
			inside = (outside = intensityOutside);
		}
	}

	public override bool IsScalable()
	{
		return true;
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
		if (dimensions != Dimensions.Dim3D)
		{
			return new Vector3(base.transform.lossyScale.z, base.transform.lossyScale.y, base.transform.lossyScale.x);
		}
		return base.transform.lossyScale;
	}

	private Vector3 ComputeRaycastGlobalVector(Vector3 localVec)
	{
		return base.transform.rotation * beamInternalLocalRotation * localVec;
	}

	public void _INTERNAL_SetDynamicOcclusionCallback(string shaderKeyword, MaterialModifier.Callback cb)
	{
		m_INTERNAL_DynamicOcclusionMode_Runtime = cb != null;
		if ((bool)m_BeamGeom)
		{
			m_BeamGeom.SetDynamicOcclusionCallback(shaderKeyword, cb);
		}
	}

	public void _INTERNAL_OnWillCameraRenderThisBeam(Camera cam)
	{
		if (this.onWillCameraRenderThisBeam != null)
		{
			this.onWillCameraRenderThisBeam(cam);
		}
	}

	public void RegisterOnBeamGeometryInitializedCallback(OnBeamGeometryInitialized cb)
	{
		m_OnBeamGeometryInitialized = (OnBeamGeometryInitialized)Delegate.Combine(m_OnBeamGeometryInitialized, cb);
		if ((bool)m_BeamGeom)
		{
			CallOnBeamGeometryInitializedCallback();
		}
	}

	private void CallOnBeamGeometryInitializedCallback()
	{
		if (m_OnBeamGeometryInitialized != null)
		{
			m_OnBeamGeometryInitialized();
			m_OnBeamGeometryInitialized = null;
		}
	}

	private void SetFadeOutValue(ref float propToChange, float value)
	{
		bool flag = isFadeOutEnabled;
		propToChange = value;
		if (isFadeOutEnabled != flag)
		{
			OnFadeOutStateChanged();
		}
	}

	private void OnFadeOutStateChanged()
	{
		if (isFadeOutEnabled && (bool)m_BeamGeom)
		{
			m_BeamGeom.RestartFadeOutCoroutine();
		}
	}

	public float GetInsideBeamFactor(Vector3 posWS)
	{
		return GetInsideBeamFactorFromObjectSpacePos(base.transform.InverseTransformPoint(posWS));
	}

	public float GetInsideBeamFactorFromObjectSpacePos(Vector3 posOS)
	{
		if (dimensions == Dimensions.Dim2D)
		{
			posOS = new Vector3(posOS.z, posOS.y, posOS.x);
		}
		if (posOS.z < 0f)
		{
			return -1f;
		}
		Vector2 vector = posOS.xy();
		if (hasMeshSkewing)
		{
			Vector3 aVector = skewingLocalForwardDirectionNormalized;
			vector -= aVector.xy() * (posOS.z / aVector.z);
		}
		Vector2 normalized = new Vector2(vector.magnitude, posOS.z + coneApexOffsetZ).normalized;
		return Mathf.Clamp((Mathf.Abs(Mathf.Sin(coneAngle * (MathF.PI / 180f) / 2f)) - Mathf.Abs(normalized.x)) / 0.1f, -1f, 1f);
	}

	[Obsolete("Use 'GenerateGeometry()' instead")]
	public void Generate()
	{
		GenerateGeometry();
	}

	public override void GenerateGeometry()
	{
		HandleBackwardCompatibility(pluginVersion, 20200);
		pluginVersion = 20200;
		ValidateProperties();
		if (m_BeamGeom == null)
		{
			m_BeamGeom = Utils.NewWithComponent<BeamGeometrySD>("Beam Geometry");
			m_BeamGeom.Initialize(this);
			CallOnBeamGeometryInitializedCallback();
		}
		m_BeamGeom.RegenerateMesh(base.enabled);
		base.GenerateGeometry();
	}

	public virtual void UpdateAfterManualPropertyChange()
	{
		ValidateProperties();
		if ((bool)m_BeamGeom)
		{
			m_BeamGeom.UpdateMaterialAndBounds();
		}
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
			m_BeamGeom.OnMasterEnable();
		}
		StartPlaytimeUpdateIfNeeded();
	}

	private void OnDisable()
	{
		if ((bool)m_BeamGeom)
		{
			m_BeamGeom.OnMasterDisable();
		}
		m_CoPlaytimeUpdate = null;
	}

	private void StartPlaytimeUpdateIfNeeded()
	{
		if (Application.isPlaying && trackChangesDuringPlaytime && m_CoPlaytimeUpdate == null)
		{
			m_CoPlaytimeUpdate = StartCoroutine(CoPlaytimeUpdate());
		}
	}

	private IEnumerator CoPlaytimeUpdate()
	{
		while (trackChangesDuringPlaytime && base.enabled)
		{
			UpdateAfterManualPropertyChange();
			yield return null;
		}
		m_CoPlaytimeUpdate = null;
	}

	private void AssignPropertiesFromAttachedSpotLight()
	{
		Light light = base.lightSpotAttached;
		if (!light)
		{
			return;
		}
		if (intensityFromLight)
		{
			intensityModeAdvanced = false;
			intensityGlobal = SpotLightHelper.GetIntensity(light) * intensityMultiplier;
		}
		if (fallOffEndFromLight)
		{
			fallOffEnd = SpotLightHelper.GetFallOffEnd(light) * fallOffEndMultiplier;
		}
		if (spotAngleFromLight)
		{
			spotAngle = Mathf.Clamp(SpotLightHelper.GetSpotAngle(light) * spotAngleMultiplier, 0.1f, 179.9f);
		}
		if (colorFromLight)
		{
			colorMode = ColorMode.Flat;
			if (useColorTemperatureFromAttachedLightSpot)
			{
				Color color = Mathf.CorrelatedColorTemperatureToRGB(light.colorTemperature);
				this.color = (light.color.linear * color).gamma;
			}
			else
			{
				this.color = light.color;
			}
		}
	}

	private void ClampProperties()
	{
		intensityInside = Mathf.Max(intensityInside, 0f);
		intensityOutside = Mathf.Max(intensityOutside, 0f);
		intensityMultiplier = Mathf.Max(intensityMultiplier, 0f);
		attenuationCustomBlending = Mathf.Clamp(attenuationCustomBlending, 0f, 1f);
		fallOffEnd = Mathf.Max(0.01f, fallOffEnd);
		fallOffStart = Mathf.Clamp(fallOffStart, 0f, fallOffEnd - 0.01f);
		fallOffEndMultiplier = Mathf.Max(fallOffEndMultiplier, 0f);
		spotAngle = Mathf.Clamp(spotAngle, 0.1f, 179.9f);
		spotAngleMultiplier = Mathf.Max(spotAngleMultiplier, 0f);
		coneRadiusStart = Mathf.Max(coneRadiusStart, 0f);
		depthBlendDistance = Mathf.Max(depthBlendDistance, 0f);
		cameraClippingDistance = Mathf.Max(cameraClippingDistance, 0f);
		geomCustomSides = Mathf.Clamp(geomCustomSides, 3, 256);
		geomCustomSegments = Mathf.Clamp(geomCustomSegments, 0, 64);
		fresnelPow = Mathf.Max(0f, fresnelPow);
		glareBehind = Mathf.Clamp(glareBehind, 0f, 1f);
		glareFrontal = Mathf.Clamp(glareFrontal, 0f, 1f);
		noiseIntensity = Mathf.Clamp(noiseIntensity, 0f, 1f);
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
			if (serializedVersion < 1301)
			{
				attenuationEquation = AttenuationEquation.Linear;
			}
			if (serializedVersion < 1501)
			{
				geomMeshType = MeshType.Custom;
				geomCustomSegments = 5;
			}
			if (serializedVersion < 1610)
			{
				intensityFromLight = false;
				intensityModeAdvanced = !Mathf.Approximately(intensityInside, intensityOutside);
			}
			if (serializedVersion < 1910 && !intensityModeAdvanced && !Mathf.Approximately(intensityInside, intensityOutside))
			{
				intensityInside = intensityOutside;
			}
			Utils.MarkCurrentSceneDirty();
		}
	}
}
