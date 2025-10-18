using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace VLB;

[AddComponentMenu("")]
[ExecuteInEditMode]
[HelpURL("http://saladgamer.com/vlb-doc/comp-lightbeam-sd/")]
public class BeamGeometrySD : BeamGeometryAbstractBase, MaterialModifier.Interface
{
	private VolumetricLightBeamSD m_Master;

	private MeshType m_CurrentMeshType;

	private MaterialModifier.Callback m_MaterialModifierCallback;

	private Coroutine m_CoFadeOut;

	private Camera m_CurrentCameraRenderingSRP;

	private bool visible
	{
		get
		{
			return base.meshRenderer.enabled;
		}
		set
		{
			base.meshRenderer.enabled = value;
		}
	}

	public int sortingLayerID
	{
		get
		{
			return base.meshRenderer.sortingLayerID;
		}
		set
		{
			base.meshRenderer.sortingLayerID = value;
		}
	}

	public int sortingOrder
	{
		get
		{
			return base.meshRenderer.sortingOrder;
		}
		set
		{
			base.meshRenderer.sortingOrder = value;
		}
	}

	public bool _INTERNAL_IsFadeOutCoroutineRunning => m_CoFadeOut != null;

	public static bool isCustomRenderPipelineSupported => true;

	private bool shouldUseGPUInstancedMaterial
	{
		get
		{
			if (m_Master._INTERNAL_DynamicOcclusionMode != MaterialManager.SD.DynamicOcclusion.DepthTexture)
			{
				return Config.Instance.GetActualRenderingMode(ShaderMode.SD) == RenderingMode.GPUInstancing;
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

	private bool isDepthBlendEnabled
	{
		get
		{
			if (!BatchingHelper.forceEnableDepthBlend)
			{
				return m_Master.depthBlendDistance > 0f;
			}
			return true;
		}
	}

	protected override VolumetricLightBeamAbstractBase GetMaster()
	{
		return m_Master;
	}

	private float ComputeFadeOutFactor(Transform camTransform)
	{
		if (m_Master.isFadeOutEnabled)
		{
			float value = Vector3.SqrMagnitude(base.meshRenderer.bounds.center - camTransform.position);
			return Mathf.InverseLerp(m_Master.fadeOutEnd * m_Master.fadeOutEnd, m_Master.fadeOutBegin * m_Master.fadeOutBegin, value);
		}
		return 1f;
	}

	private IEnumerator CoUpdateFadeOut()
	{
		while (m_Master.isFadeOutEnabled)
		{
			ComputeFadeOutFactor();
			yield return null;
		}
		SetFadeOutFactorProp(1f);
		m_CoFadeOut = null;
	}

	private void ComputeFadeOutFactor()
	{
		Transform fadeOutCameraTransform = Config.Instance.fadeOutCameraTransform;
		if ((bool)fadeOutCameraTransform)
		{
			float fadeOutFactorProp = ComputeFadeOutFactor(fadeOutCameraTransform);
			SetFadeOutFactorProp(fadeOutFactorProp);
		}
		else
		{
			SetFadeOutFactorProp(1f);
		}
	}

	private void SetFadeOutFactorProp(float value)
	{
		if (value > 0f)
		{
			base.meshRenderer.enabled = true;
			MaterialChangeStart();
			SetMaterialProp(ShaderProperties.SD.FadeOutFactor, value);
			MaterialChangeStop();
		}
		else
		{
			base.meshRenderer.enabled = false;
		}
	}

	private void StopFadeOutCoroutine()
	{
		if (m_CoFadeOut != null)
		{
			StopCoroutine(m_CoFadeOut);
			m_CoFadeOut = null;
		}
	}

	public void RestartFadeOutCoroutine()
	{
		StopFadeOutCoroutine();
		if ((bool)m_Master && m_Master.isFadeOutEnabled)
		{
			m_CoFadeOut = StartCoroutine(CoUpdateFadeOut());
		}
	}

	public void OnMasterEnable()
	{
		visible = true;
		RestartFadeOutCoroutine();
	}

	public void OnMasterDisable()
	{
		StopFadeOutCoroutine();
		visible = false;
	}

	private void OnDisable()
	{
		SRPHelper.UnregisterOnBeginCameraRendering(OnBeginCameraRenderingSRP);
		m_CurrentCameraRenderingSRP = null;
	}

	private void OnEnable()
	{
		RestartFadeOutCoroutine();
		SRPHelper.RegisterOnBeginCameraRendering(OnBeginCameraRenderingSRP);
	}

	public void Initialize(VolumetricLightBeamSD master)
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
		if (!shouldUseGPUInstancedMaterial)
		{
			m_CustomMaterial = Config.Instance.NewMaterialTransient(ShaderMode.SD, gpuInstanced: false);
			ApplyMaterial();
		}
		if (SortingLayer.IsValid(m_Master.sortingLayerID))
		{
			sortingLayerID = m_Master.sortingLayerID;
		}
		else
		{
			Debug.LogError($"Beam '{Utils.GetPath(m_Master.transform)}' has an invalid sortingLayerID ({m_Master.sortingLayerID}). Please fix it by setting a valid layer.");
		}
		sortingOrder = m_Master.sortingOrder;
		base.meshFilter = base.gameObject.GetOrAddComponent<MeshFilter>();
		base.meshFilter.hideFlags = proceduralObjectsHideFlags;
		base.gameObject.hideFlags = proceduralObjectsHideFlags;
		RestartFadeOutCoroutine();
	}

	public void RegenerateMesh(bool masterEnabled)
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
		if ((bool)base.coneMesh && m_CurrentMeshType == MeshType.Custom)
		{
			UnityEngine.Object.DestroyImmediate(base.coneMesh);
		}
		m_CurrentMeshType = m_Master.geomMeshType;
		switch (m_Master.geomMeshType)
		{
		case MeshType.Custom:
			base.coneMesh = MeshGenerator.GenerateConeZ_Radii(1f, 1f, 1f, m_Master.geomCustomSides, m_Master.geomCustomSegments, m_Master.geomCap, Config.Instance.SD_requiresDoubleSidedMesh);
			base.coneMesh.hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
			base.meshFilter.mesh = base.coneMesh;
			break;
		case MeshType.Shared:
			base.coneMesh = GlobalMeshSD.Get();
			base.meshFilter.sharedMesh = base.coneMesh;
			break;
		default:
			Debug.LogError("Unsupported MeshType");
			break;
		}
		UpdateMaterialAndBounds();
		visible = masterEnabled;
	}

	private Vector3 ComputeLocalMatrix()
	{
		float num = Mathf.Max(m_Master.coneRadiusStart, m_Master.coneRadiusEnd);
		base.transform.localScale = new Vector3(num, num, m_Master.maxGeometryDistance);
		base.transform.localRotation = m_Master.beamInternalLocalRotation;
		return base.transform.localScale;
	}

	private MaterialManager.StaticPropertiesSD ComputeMaterialStaticProperties()
	{
		MaterialManager.ColorGradient colorGradient = MaterialManager.ColorGradient.Off;
		if (m_Master.colorMode == ColorMode.Gradient)
		{
			colorGradient = ((Utils.GetFloatPackingPrecision() != Utils.FloatPackingPrecision.High) ? MaterialManager.ColorGradient.MatrixLow : MaterialManager.ColorGradient.MatrixHigh);
		}
		return new MaterialManager.StaticPropertiesSD
		{
			blendingMode = (MaterialManager.BlendingMode)m_Master.blendingMode,
			noise3D = (isNoiseEnabled ? MaterialManager.Noise3D.On : MaterialManager.Noise3D.Off),
			depthBlend = (isDepthBlendEnabled ? MaterialManager.SD.DepthBlend.On : MaterialManager.SD.DepthBlend.Off),
			colorGradient = colorGradient,
			dynamicOcclusion = m_Master._INTERNAL_DynamicOcclusionMode_Runtime,
			meshSkewing = (m_Master.hasMeshSkewing ? MaterialManager.SD.MeshSkewing.On : MaterialManager.SD.MeshSkewing.Off),
			shaderAccuracy = ((m_Master.shaderAccuracy != ShaderAccuracy.Fast) ? MaterialManager.SD.ShaderAccuracy.High : MaterialManager.SD.ShaderAccuracy.Fast)
		};
	}

	private bool ApplyMaterial()
	{
		MaterialManager.StaticPropertiesSD staticProps = ComputeMaterialStaticProperties();
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
		else
		{
			Debug.LogError("Setting a Texture property to a GPU instanced material is not supported");
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

	public void SetDynamicOcclusionCallback(string shaderKeyword, MaterialModifier.Callback cb)
	{
		m_MaterialModifierCallback = cb;
		if ((bool)m_CustomMaterial)
		{
			m_CustomMaterial.SetKeywordEnabled(shaderKeyword, cb != null);
			cb?.Invoke(this);
		}
		else
		{
			UpdateMaterialAndBounds();
		}
	}

	public void UpdateMaterialAndBounds()
	{
		if (!ApplyMaterial())
		{
			return;
		}
		MaterialChangeStart();
		if (m_CustomMaterial == null && m_MaterialModifierCallback != null)
		{
			m_MaterialModifierCallback(this);
		}
		float f = m_Master.coneAngle * (MathF.PI / 180f) / 2f;
		SetMaterialProp(ShaderProperties.SD.ConeSlopeCosSin, new Vector2(Mathf.Cos(f), Mathf.Sin(f)));
		SetMaterialProp(value: new Vector2(Mathf.Max(m_Master.coneRadiusStart, 0.0001f), Mathf.Max(m_Master.coneRadiusEnd, 0.0001f)), nameID: ShaderProperties.ConeRadius);
		float x = Mathf.Sign(m_Master.coneApexOffsetZ) * Mathf.Max(Mathf.Abs(m_Master.coneApexOffsetZ), 0.0001f);
		SetMaterialProp(ShaderProperties.ConeGeomProps, new Vector2(x, m_Master.geomSides));
		if (m_Master.usedColorMode == ColorMode.Flat)
		{
			SetMaterialProp(ShaderProperties.ColorFlat, m_Master.color);
		}
		else
		{
			Utils.FloatPackingPrecision floatPackingPrecision = Utils.GetFloatPackingPrecision();
			m_ColorGradientMatrix = m_Master.colorGradient.SampleInMatrix((int)floatPackingPrecision);
		}
		m_Master.GetInsideAndOutsideIntensity(out var inside, out var outside);
		SetMaterialProp(ShaderProperties.SD.AlphaInside, inside);
		SetMaterialProp(ShaderProperties.SD.AlphaOutside, outside);
		SetMaterialProp(ShaderProperties.SD.AttenuationLerpLinearQuad, m_Master.attenuationLerpLinearQuad);
		SetMaterialProp(ShaderProperties.DistanceFallOff, new Vector3(m_Master.fallOffStart, m_Master.fallOffEnd, m_Master.maxGeometryDistance));
		SetMaterialProp(ShaderProperties.SD.DistanceCamClipping, m_Master.cameraClippingDistance);
		SetMaterialProp(ShaderProperties.SD.FresnelPow, Mathf.Max(0.001f, m_Master.fresnelPow));
		SetMaterialProp(ShaderProperties.SD.GlareBehind, m_Master.glareBehind);
		SetMaterialProp(ShaderProperties.SD.GlareFrontal, m_Master.glareFrontal);
		SetMaterialProp(ShaderProperties.SD.DrawCap, m_Master.geomCap ? 1 : 0);
		SetMaterialProp(ShaderProperties.SD.TiltVector, m_Master.tiltFactor);
		SetMaterialProp(ShaderProperties.SD.AdditionalClippingPlaneWS, m_Master.additionalClippingPlane);
		if (Config.Instance.isHDRPExposureWeightSupported)
		{
			SetMaterialProp(ShaderProperties.HDRPExposureWeight, m_Master.hdrpExposureWeight);
		}
		if (isDepthBlendEnabled)
		{
			SetMaterialProp(ShaderProperties.SD.DepthBlendDistance, m_Master.depthBlendDistance);
		}
		if (isNoiseEnabled)
		{
			Noise3D.LoadIfNeeded();
			Vector3 vector = (m_Master.noiseVelocityUseGlobal ? Config.Instance.globalNoiseVelocity : m_Master.noiseVelocityLocal);
			float w = (m_Master.noiseScaleUseGlobal ? Config.Instance.globalNoiseScale : m_Master.noiseScaleLocal);
			SetMaterialProp(ShaderProperties.NoiseVelocityAndScale, new Vector4(vector.x, vector.y, vector.z, w));
			SetMaterialProp(ShaderProperties.NoiseParam, new Vector2(m_Master.noiseIntensity, (m_Master.noiseMode == NoiseMode.WorldSpace) ? 0f : 1f));
		}
		Vector3 vector2 = ComputeLocalMatrix();
		if (m_Master.hasMeshSkewing)
		{
			Vector3 skewingLocalForwardDirectionNormalized = m_Master.skewingLocalForwardDirectionNormalized;
			SetMaterialProp(ShaderProperties.SD.LocalForwardDirection, skewingLocalForwardDirectionNormalized);
			if (base.coneMesh != null)
			{
				Vector3 vector3 = skewingLocalForwardDirectionNormalized;
				vector3 /= vector3.z;
				vector3 *= m_Master.fallOffEnd;
				vector3.x /= vector2.x;
				vector3.y /= vector2.y;
				Bounds bounds = MeshGenerator.ComputeBounds(1f, 1f, 1f);
				Vector3 min = bounds.min;
				Vector3 max = bounds.max;
				if (vector3.x > 0f)
				{
					max.x += vector3.x;
				}
				else
				{
					min.x += vector3.x;
				}
				if (vector3.y > 0f)
				{
					max.y += vector3.y;
				}
				else
				{
					min.y += vector3.y;
				}
				bounds.min = min;
				bounds.max = max;
				base.coneMesh.bounds = bounds;
			}
		}
		UpdateMatricesPropertiesForGPUInstancingSRP();
		MaterialChangeStop();
	}

	private void UpdateMatricesPropertiesForGPUInstancingSRP()
	{
		if (SRPHelper.IsUsingCustomRenderPipeline() && Config.Instance.GetActualRenderingMode(ShaderMode.SD) == RenderingMode.GPUInstancing)
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
			UpdateCameraRelatedProperties(cam);
			m_Master._INTERNAL_OnWillCameraRenderThisBeam(cam);
		}
	}

	private void UpdateCameraRelatedProperties(Camera cam)
	{
		if ((bool)cam && (bool)m_Master)
		{
			MaterialChangeStart();
			Vector3 posOS = m_Master.transform.InverseTransformPoint(cam.transform.position);
			Vector3 normalized = base.transform.InverseTransformDirection(cam.transform.forward).normalized;
			float w = (cam.orthographic ? (-1f) : m_Master.GetInsideBeamFactorFromObjectSpacePos(posOS));
			SetMaterialProp(ShaderProperties.SD.CameraParams, new Vector4(normalized.x, normalized.y, normalized.z, w));
			UpdateMatricesPropertiesForGPUInstancingSRP();
			if (m_Master.usedColorMode == ColorMode.Gradient)
			{
				SetMaterialProp(ShaderProperties.ColorGradientMatrix, m_ColorGradientMatrix);
			}
			MaterialChangeStop();
			if (m_Master.depthBlendDistance > 0f)
			{
				cam.depthTextureMode |= DepthTextureMode.Depth;
			}
		}
	}
}
