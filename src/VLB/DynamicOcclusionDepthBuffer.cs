using UnityEngine;

namespace VLB;

[ExecuteInEditMode]
[HelpURL("http://saladgamer.com/vlb-doc/comp-dynocclusion-sd-depthbuffer/")]
[AddComponentMenu("VLB/SD/Dynamic Occlusion (Depth Buffer)")]
public class DynamicOcclusionDepthBuffer : DynamicOcclusionAbstractBase
{
	public new const string ClassName = "DynamicOcclusionDepthBuffer";

	public LayerMask layerMask = Consts.DynOcclusion.LayerMaskDefault;

	public bool useOcclusionCulling = true;

	public int depthMapResolution = 128;

	public float fadeDistanceToSurface;

	private Camera m_DepthCamera;

	private bool m_NeedToUpdateOcclusionNextFrame;

	protected override string GetShaderKeyword()
	{
		return "VLB_OCCLUSION_DEPTH_TEXTURE";
	}

	protected override MaterialManager.SD.DynamicOcclusion GetDynamicOcclusionMode()
	{
		return MaterialManager.SD.DynamicOcclusion.DepthTexture;
	}

	private void ProcessOcclusionInternal()
	{
		UpdateDepthCameraPropertiesAccordingToBeam();
		m_DepthCamera.Render();
	}

	protected override bool OnProcessOcclusion(ProcessOcclusionSource source)
	{
		if (SRPHelper.IsUsingCustomRenderPipeline())
		{
			m_NeedToUpdateOcclusionNextFrame = true;
		}
		else
		{
			ProcessOcclusionInternal();
		}
		return true;
	}

	private void Update()
	{
		if (m_NeedToUpdateOcclusionNextFrame && (bool)m_Master && (bool)m_DepthCamera && Time.frameCount > 1)
		{
			ProcessOcclusionInternal();
			m_NeedToUpdateOcclusionNextFrame = false;
		}
	}

	private void UpdateDepthCameraPropertiesAccordingToBeam()
	{
		Utils.SetupDepthCamera(m_DepthCamera, m_Master.coneApexOffsetZ, m_Master.maxGeometryDistance, m_Master.coneRadiusStart, m_Master.coneRadiusEnd, m_Master.beamLocalForward, m_Master.GetLossyScale(), m_Master.IsScalable(), m_Master.beamInternalLocalRotation, shouldScaleMinNearClipPlane: true);
	}

	public bool HasLayerMaskIssues()
	{
		if (Config.Instance.geometryOverrideLayer)
		{
			int num = 1 << Config.Instance.geometryLayerID;
			return (layerMask.value & num) == num;
		}
		return false;
	}

	protected override void OnValidateProperties()
	{
		base.OnValidateProperties();
		depthMapResolution = Mathf.Clamp(Mathf.NextPowerOfTwo(depthMapResolution), 8, 2048);
		fadeDistanceToSurface = Mathf.Max(fadeDistanceToSurface, 0f);
	}

	private void InstantiateOrActivateDepthCamera()
	{
		if (m_DepthCamera != null)
		{
			m_DepthCamera.gameObject.SetActive(value: true);
			return;
		}
		base.gameObject.ForeachComponentsInDirectChildrenOnly(delegate(Camera cam)
		{
			Object.DestroyImmediate(cam.gameObject);
		}, includeInactive: true);
		m_DepthCamera = Utils.NewWithComponent<Camera>("Depth Camera");
		if ((bool)m_DepthCamera && (bool)m_Master)
		{
			m_DepthCamera.enabled = false;
			m_DepthCamera.cullingMask = layerMask;
			m_DepthCamera.clearFlags = CameraClearFlags.Depth;
			m_DepthCamera.depthTextureMode = DepthTextureMode.Depth;
			m_DepthCamera.renderingPath = RenderingPath.VertexLit;
			m_DepthCamera.useOcclusionCulling = useOcclusionCulling;
			m_DepthCamera.gameObject.hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
			m_DepthCamera.transform.SetParent(base.transform, worldPositionStays: false);
			Config.Instance.SetURPScriptableRendererIndexToDepthCamera(m_DepthCamera);
			RenderTexture targetTexture = new RenderTexture(depthMapResolution, depthMapResolution, 16, RenderTextureFormat.Depth);
			m_DepthCamera.targetTexture = targetTexture;
			UpdateDepthCameraPropertiesAccordingToBeam();
		}
	}

	protected override void OnEnablePostValidate()
	{
		InstantiateOrActivateDepthCamera();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)m_DepthCamera)
		{
			m_DepthCamera.gameObject.SetActive(value: false);
		}
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DestroyDepthCamera();
	}

	private void DestroyDepthCamera()
	{
		if ((bool)m_DepthCamera)
		{
			if ((bool)m_DepthCamera.targetTexture)
			{
				m_DepthCamera.targetTexture.Release();
				Object.DestroyImmediate(m_DepthCamera.targetTexture);
				m_DepthCamera.targetTexture = null;
			}
			Object.DestroyImmediate(m_DepthCamera.gameObject);
			m_DepthCamera = null;
		}
	}

	protected override void OnModifyMaterialCallback(MaterialModifier.Interface owner)
	{
		owner.SetMaterialProp(ShaderProperties.SD.DynamicOcclusionDepthTexture, m_DepthCamera.targetTexture);
		Vector3 lossyScale = m_Master.GetLossyScale();
		owner.SetMaterialProp(ShaderProperties.SD.DynamicOcclusionDepthProps, new Vector4(Mathf.Sign(lossyScale.x) * Mathf.Sign(lossyScale.z), Mathf.Sign(lossyScale.y), fadeDistanceToSurface, m_DepthCamera.orthographic ? 0f : 1f));
	}
}
