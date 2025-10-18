using UnityEngine;
using UnityEngine.Rendering;

namespace VLB;

[AddComponentMenu("")]
[ExecuteInEditMode]
[HelpURL("http://saladgamer.com/vlb-doc/comp-lightbeam-hd/")]
public class BeamGeometryHD : BeamGeometryAbstractBase
{
	public enum InvalidTexture
	{
		Null,
		NoDepth
	}

	private VolumetricLightBeamHD m_Master;

	private VolumetricCookieHD m_Cookie;

	private VolumetricShadowHD m_Shadow;

	private Camera m_CurrentCameraRenderingSRP;

	private DirtyProps m_DirtyProps;

	public bool visible
	{
		set
		{
			if ((bool)base.meshRenderer)
			{
				base.meshRenderer.enabled = value;
			}
		}
	}

	public int sortingLayerID
	{
		set
		{
			if ((bool)base.meshRenderer)
			{
				base.meshRenderer.sortingLayerID = value;
			}
		}
	}

	public int sortingOrder
	{
		set
		{
			if ((bool)base.meshRenderer)
			{
				base.meshRenderer.sortingOrder = value;
			}
		}
	}

	public static bool isCustomRenderPipelineSupported => true;

	private bool shouldUseGPUInstancedMaterial
	{
		get
		{
			if (Config.Instance.GetActualRenderingMode(ShaderMode.HD) == RenderingMode.GPUInstancing)
			{
				if (m_Cookie == null)
				{
					return m_Shadow == null;
				}
				return false;
			}
			return false;
		}
	}

	private bool isNoiseEnabled
	{
		get
		{
			if (m_Master.isNoiseEnabled && m_Master.noiseIntensity > 0f)
			{
				return Noise3D.isSupported;
			}
			return false;
		}
	}

	protected override VolumetricLightBeamAbstractBase GetMaster()
	{
		return m_Master;
	}

	private void OnDisable()
	{
		SRPHelper.UnregisterOnBeginCameraRendering(OnBeginCameraRenderingSRP);
		m_CurrentCameraRenderingSRP = null;
	}

	private void OnEnable()
	{
		SRPHelper.RegisterOnBeginCameraRendering(OnBeginCameraRenderingSRP);
	}

	public void Initialize(VolumetricLightBeamHD master)
	{
		HideFlags proceduralObjectsHideFlags = Consts.Internal.ProceduralObjectsHideFlags;
		m_Master = master;
		base.transform.SetParent(master.transform, worldPositionStays: false);
		base.meshRenderer = base.gameObject.GetOrAddComponent<MeshRenderer>();
		base.meshRenderer.hideFlags = proceduralObjectsHideFlags;
		base.meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		base.meshRenderer.receiveShadows = false;
		base.meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		base.meshRenderer.lightProbeUsage = LightProbeUsage.Off;
		m_Cookie = m_Master.GetAdditionalComponentCookie();
		m_Shadow = m_Master.GetAdditionalComponentShadow();
		if (!shouldUseGPUInstancedMaterial)
		{
			m_CustomMaterial = Config.Instance.NewMaterialTransient(ShaderMode.HD, gpuInstanced: false);
			ApplyMaterial();
		}
		if (m_Master.DoesSupportSorting2D())
		{
			if (SortingLayer.IsValid(m_Master.GetSortingLayerID()))
			{
				sortingLayerID = m_Master.GetSortingLayerID();
			}
			else
			{
				Debug.LogError($"Beam '{Utils.GetPath(m_Master.transform)}' has an invalid sortingLayerID ({m_Master.GetSortingLayerID()}). Please fix it by setting a valid layer.");
			}
			sortingOrder = m_Master.GetSortingOrder();
		}
		base.meshFilter = base.gameObject.GetOrAddComponent<MeshFilter>();
		base.meshFilter.hideFlags = proceduralObjectsHideFlags;
		base.gameObject.hideFlags = proceduralObjectsHideFlags;
	}

	public void RegenerateMesh()
	{
		if (Config.Instance.geometryOverrideLayer)
		{
			base.gameObject.layer = Config.Instance.geometryLayerID;
		}
		else
		{
			base.gameObject.layer = m_Master.gameObject.layer;
		}
		base.gameObject.tag = Config.Instance.geometryTag;
		base.coneMesh = GlobalMeshHD.Get();
		base.meshFilter.sharedMesh = base.coneMesh;
		UpdateMaterialAndBounds();
	}

	private Vector3 ComputeLocalMatrix()
	{
		float num = Mathf.Max(m_Master.coneRadiusStart, m_Master.coneRadiusEnd);
		Vector3 vector = new Vector3(num, num, m_Master.maxGeometryDistance);
		if (!m_Master.scalable)
		{
			vector = vector.Divide(m_Master.GetLossyScale());
		}
		base.transform.localScale = vector;
		base.transform.localRotation = m_Master.beamInternalLocalRotation;
		return vector;
	}

	private MaterialManager.StaticPropertiesHD ComputeMaterialStaticProperties()
	{
		MaterialManager.ColorGradient colorGradient = MaterialManager.ColorGradient.Off;
		if (m_Master.colorMode == ColorMode.Gradient)
		{
			colorGradient = ((Utils.GetFloatPackingPrecision() != Utils.FloatPackingPrecision.High) ? MaterialManager.ColorGradient.MatrixLow : MaterialManager.ColorGradient.MatrixHigh);
		}
		return new MaterialManager.StaticPropertiesHD
		{
			blendingMode = (MaterialManager.BlendingMode)m_Master.blendingMode,
			attenuation = ((m_Master.attenuationEquation != AttenuationEquationHD.Linear) ? MaterialManager.HD.Attenuation.Quadratic : MaterialManager.HD.Attenuation.Linear),
			noise3D = (isNoiseEnabled ? MaterialManager.Noise3D.On : MaterialManager.Noise3D.Off),
			colorGradient = colorGradient,
			shadow = ((m_Shadow != null) ? MaterialManager.HD.Shadow.On : MaterialManager.HD.Shadow.Off),
			cookie = ((m_Cookie != null) ? ((m_Cookie.channel != CookieChannel.RGBA) ? MaterialManager.HD.Cookie.SingleChannel : MaterialManager.HD.Cookie.RGBA) : MaterialManager.HD.Cookie.Off),
			raymarchingQualityIndex = m_Master.raymarchingQualityIndex
		};
	}

	private bool ApplyMaterial()
	{
		MaterialManager.StaticPropertiesHD staticProps = ComputeMaterialStaticProperties();
		Material material = null;
		if (!shouldUseGPUInstancedMaterial)
		{
			material = m_CustomMaterial;
			if ((bool)material)
			{
				staticProps.ApplyToMaterial(material);
			}
		}
		else
		{
			material = MaterialManager.GetInstancedMaterial(m_Master._INTERNAL_InstancedMaterialGroupID, ref staticProps);
		}
		base.meshRenderer.material = material;
		return material != null;
	}

	public void SetMaterialProp(int nameID, float value)
	{
		if ((bool)m_CustomMaterial)
		{
			m_CustomMaterial.SetFloat(nameID, value);
		}
		else
		{
			MaterialManager.materialPropertyBlock.SetFloat(nameID, value);
		}
	}

	public void SetMaterialProp(int nameID, Vector4 value)
	{
		if ((bool)m_CustomMaterial)
		{
			m_CustomMaterial.SetVector(nameID, value);
		}
		else
		{
			MaterialManager.materialPropertyBlock.SetVector(nameID, value);
		}
	}

	public void SetMaterialProp(int nameID, Color value)
	{
		if ((bool)m_CustomMaterial)
		{
			m_CustomMaterial.SetColor(nameID, value);
		}
		else
		{
			MaterialManager.materialPropertyBlock.SetColor(nameID, value);
		}
	}

	public void SetMaterialProp(int nameID, Matrix4x4 value)
	{
		if ((bool)m_CustomMaterial)
		{
			m_CustomMaterial.SetMatrix(nameID, value);
		}
		else
		{
			MaterialManager.materialPropertyBlock.SetMatrix(nameID, value);
		}
	}

	public void SetMaterialProp(int nameID, Texture value)
	{
		if ((bool)m_CustomMaterial)
		{
			m_CustomMaterial.SetTexture(nameID, value);
		}
	}

	public void SetMaterialProp(int nameID, InvalidTexture invalidTexture)
	{
		if ((bool)m_CustomMaterial)
		{
			Texture value = null;
			if (invalidTexture == InvalidTexture.NoDepth)
			{
				value = (SystemInfo.usesReversedZBuffer ? Texture2D.blackTexture : Texture2D.whiteTexture);
			}
			m_CustomMaterial.SetTexture(nameID, value);
		}
	}

	private void MaterialChangeStart()
	{
		if (m_CustomMaterial == null)
		{
			base.meshRenderer.GetPropertyBlock(MaterialManager.materialPropertyBlock);
		}
	}

	private void MaterialChangeStop()
	{
		if (m_CustomMaterial == null)
		{
			base.meshRenderer.SetPropertyBlock(MaterialManager.materialPropertyBlock);
		}
	}

	public void SetPropertyDirty(DirtyProps prop)
	{
		m_DirtyProps |= prop;
		if (prop.HasAtLeastOneFlag(DirtyProps.OnlyMaterialChangeOnly))
		{
			UpdateMaterialAndBounds();
		}
	}

	private void UpdateMaterialAndBounds()
	{
		if (ApplyMaterial())
		{
			MaterialChangeStart();
			m_DirtyProps = DirtyProps.All;
			if (isNoiseEnabled)
			{
				Noise3D.LoadIfNeeded();
			}
			ComputeLocalMatrix();
			UpdateMatricesPropertiesForGPUInstancingSRP();
			MaterialChangeStop();
		}
	}

	private void UpdateMatricesPropertiesForGPUInstancingSRP()
	{
		if (SRPHelper.IsUsingCustomRenderPipeline() && Config.Instance.GetActualRenderingMode(ShaderMode.HD) == RenderingMode.GPUInstancing)
		{
			SetMaterialProp(ShaderProperties.LocalToWorldMatrix, base.transform.localToWorldMatrix);
			SetMaterialProp(ShaderProperties.WorldToLocalMatrix, base.transform.worldToLocalMatrix);
		}
	}

	private void OnBeginCameraRenderingSRP(ScriptableRenderContext context, Camera cam)
	{
		m_CurrentCameraRenderingSRP = cam;
	}

	private void OnWillRenderObject()
	{
		Camera camera = null;
		camera = ((!SRPHelper.IsUsingCustomRenderPipeline()) ? Camera.current : m_CurrentCameraRenderingSRP);
		OnWillCameraRenderThisBeam(camera);
	}

	private void OnWillCameraRenderThisBeam(Camera cam)
	{
		if ((bool)m_Master && (bool)cam && cam.enabled)
		{
			UpdateMaterialPropertiesForCamera(cam);
			if ((bool)m_Shadow)
			{
				m_Shadow.OnWillCameraRenderThisBeam(cam, this);
			}
		}
	}

	private void UpdateDirtyMaterialProperties()
	{
		if (m_DirtyProps == DirtyProps.None)
		{
			return;
		}
		if (m_DirtyProps.HasFlag(DirtyProps.Intensity))
		{
			SetMaterialProp(ShaderProperties.HD.Intensity, m_Master.intensity);
		}
		if (m_DirtyProps.HasFlag(DirtyProps.HDRPExposureWeight) && Config.Instance.isHDRPExposureWeightSupported)
		{
			SetMaterialProp(ShaderProperties.HDRPExposureWeight, m_Master.hdrpExposureWeight);
		}
		if (m_DirtyProps.HasFlag(DirtyProps.SideSoftness))
		{
			SetMaterialProp(ShaderProperties.HD.SideSoftness, m_Master.sideSoftness);
		}
		if (m_DirtyProps.HasFlag(DirtyProps.Color))
		{
			if (m_Master.colorMode == ColorMode.Flat)
			{
				SetMaterialProp(ShaderProperties.ColorFlat, m_Master.colorFlat);
			}
			else
			{
				Utils.FloatPackingPrecision floatPackingPrecision = Utils.GetFloatPackingPrecision();
				m_ColorGradientMatrix = m_Master.colorGradient.SampleInMatrix((int)floatPackingPrecision);
			}
		}
		if (m_DirtyProps.HasFlag(DirtyProps.Cone))
		{
			SetMaterialProp(value: new Vector2(Mathf.Max(m_Master.coneRadiusStart, 0.0001f), Mathf.Max(m_Master.coneRadiusEnd, 0.0001f)), nameID: ShaderProperties.ConeRadius);
			float coneApexOffsetZ = m_Master.GetConeApexOffsetZ(counterApplyScaleForUnscalableBeam: false);
			float x = Mathf.Sign(coneApexOffsetZ) * Mathf.Max(Mathf.Abs(coneApexOffsetZ), 0.0001f);
			SetMaterialProp(ShaderProperties.ConeGeomProps, new Vector2(x, Config.Instance.sharedMeshSides));
			SetMaterialProp(ShaderProperties.DistanceFallOff, new Vector3(m_Master.fallOffStart, m_Master.fallOffEnd, m_Master.maxGeometryDistance));
			ComputeLocalMatrix();
		}
		if (m_DirtyProps.HasFlag(DirtyProps.Jittering))
		{
			SetMaterialProp(ShaderProperties.HD.Jittering, new Vector4(m_Master.jitteringFactor, m_Master.jitteringFrameRate, m_Master.jitteringLerpRange.minValue, m_Master.jitteringLerpRange.maxValue));
		}
		if (isNoiseEnabled)
		{
			if (m_DirtyProps.HasFlag(DirtyProps.NoiseMode) || m_DirtyProps.HasFlag(DirtyProps.NoiseIntensity))
			{
				SetMaterialProp(ShaderProperties.NoiseParam, new Vector2(m_Master.noiseIntensity, (m_Master.noiseMode == NoiseMode.WorldSpace) ? 0f : 1f));
			}
			if (m_DirtyProps.HasFlag(DirtyProps.NoiseVelocityAndScale))
			{
				Vector3 vector = (m_Master.noiseVelocityUseGlobal ? Config.Instance.globalNoiseVelocity : m_Master.noiseVelocityLocal);
				float w = (m_Master.noiseScaleUseGlobal ? Config.Instance.globalNoiseScale : m_Master.noiseScaleLocal);
				SetMaterialProp(ShaderProperties.NoiseVelocityAndScale, new Vector4(vector.x, vector.y, vector.z, w));
			}
		}
		if (m_DirtyProps.HasFlag(DirtyProps.CookieProps))
		{
			VolumetricCookieHD.ApplyMaterialProperties(m_Cookie, this);
		}
		if (m_DirtyProps.HasFlag(DirtyProps.ShadowProps))
		{
			VolumetricShadowHD.ApplyMaterialProperties(m_Shadow, this);
		}
		m_DirtyProps = DirtyProps.None;
	}

	private void UpdateMaterialPropertiesForCamera(Camera cam)
	{
		if ((bool)cam && (bool)m_Master)
		{
			MaterialChangeStart();
			SetMaterialProp(ShaderProperties.HD.TransformScale, m_Master.scalable ? m_Master.GetLossyScale() : Vector3.one);
			Vector3 normalized = base.transform.InverseTransformDirection(cam.transform.forward).normalized;
			SetMaterialProp(ShaderProperties.HD.CameraForwardOS, normalized);
			SetMaterialProp(ShaderProperties.HD.CameraForwardWS, cam.transform.forward);
			UpdateDirtyMaterialProperties();
			if (m_Master.colorMode == ColorMode.Gradient)
			{
				SetMaterialProp(ShaderProperties.ColorGradientMatrix, m_ColorGradientMatrix);
			}
			UpdateMatricesPropertiesForGPUInstancingSRP();
			MaterialChangeStop();
			cam.depthTextureMode |= DepthTextureMode.Depth;
		}
	}
}
