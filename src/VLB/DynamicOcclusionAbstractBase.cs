using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace VLB;

[AddComponentMenu("")]
[DisallowMultipleComponent]
[RequireComponent(typeof(VolumetricLightBeamSD))]
public abstract class DynamicOcclusionAbstractBase : MonoBehaviour
{
	protected enum ProcessOcclusionSource
	{
		RenderLoop,
		OnEnable,
		EditorUpdate,
		User
	}

	public const string ClassName = "DynamicOcclusionAbstractBase";

	public DynamicOcclusionUpdateRate updateRate = DynamicOcclusionUpdateRate.EveryXFrames;

	[FormerlySerializedAs("waitFrameCount")]
	public int waitXFrames = 3;

	public static bool _INTERNAL_ApplyRandomFrameOffset = true;

	private TransformUtils.Packed m_TransformPacked;

	private int m_LastFrameRendered = int.MinValue;

	protected VolumetricLightBeamSD m_Master;

	protected MaterialModifier.Callback m_MaterialModifierCallbackCached;

	public int _INTERNAL_LastFrameRendered => m_LastFrameRendered;

	public event Action onOcclusionProcessed;

	public void ProcessOcclusionManually()
	{
		ProcessOcclusion(ProcessOcclusionSource.User);
	}

	protected void ProcessOcclusion(ProcessOcclusionSource source)
	{
		if (Config.Instance.featureEnabledDynamicOcclusion && (m_LastFrameRendered != Time.frameCount || !Application.isPlaying || source != ProcessOcclusionSource.OnEnable))
		{
			bool flag = OnProcessOcclusion(source);
			if (this.onOcclusionProcessed != null)
			{
				this.onOcclusionProcessed();
			}
			if ((bool)m_Master)
			{
				m_Master._INTERNAL_SetDynamicOcclusionCallback(GetShaderKeyword(), flag ? m_MaterialModifierCallbackCached : null);
			}
			if (updateRate.HasFlag(DynamicOcclusionUpdateRate.OnBeamMove))
			{
				m_TransformPacked = base.transform.GetWorldPacked();
			}
			bool num = m_LastFrameRendered < 0;
			m_LastFrameRendered = Time.frameCount;
			if (num && _INTERNAL_ApplyRandomFrameOffset)
			{
				m_LastFrameRendered += UnityEngine.Random.Range(0, waitXFrames);
			}
		}
	}

	protected abstract string GetShaderKeyword();

	protected abstract MaterialManager.SD.DynamicOcclusion GetDynamicOcclusionMode();

	protected abstract bool OnProcessOcclusion(ProcessOcclusionSource source);

	protected abstract void OnModifyMaterialCallback(MaterialModifier.Interface owner);

	protected abstract void OnEnablePostValidate();

	protected virtual void OnValidateProperties()
	{
		waitXFrames = Mathf.Clamp(waitXFrames, 1, 60);
	}

	protected virtual void Awake()
	{
		m_Master = GetComponent<VolumetricLightBeamSD>();
		m_Master._INTERNAL_DynamicOcclusionMode = GetDynamicOcclusionMode();
	}

	protected virtual void OnDestroy()
	{
		m_Master._INTERNAL_DynamicOcclusionMode = MaterialManager.SD.DynamicOcclusion.Off;
		DisableOcclusion();
	}

	protected virtual void OnEnable()
	{
		m_MaterialModifierCallbackCached = OnModifyMaterialCallback;
		OnValidateProperties();
		OnEnablePostValidate();
		m_Master.onWillCameraRenderThisBeam += OnWillCameraRender;
		if (!updateRate.HasFlag(DynamicOcclusionUpdateRate.Never))
		{
			m_Master.RegisterOnBeamGeometryInitializedCallback(delegate
			{
				ProcessOcclusion(ProcessOcclusionSource.OnEnable);
			});
		}
	}

	protected virtual void OnDisable()
	{
		m_Master.onWillCameraRenderThisBeam -= OnWillCameraRender;
		DisableOcclusion();
	}

	private void OnWillCameraRender(Camera cam)
	{
		if (cam != null && cam.enabled && Time.frameCount != m_LastFrameRendered)
		{
			bool flag = false;
			if (!flag && updateRate.HasFlag(DynamicOcclusionUpdateRate.OnBeamMove) && !m_TransformPacked.IsSame(base.transform))
			{
				flag = true;
			}
			if (!flag && updateRate.HasFlag(DynamicOcclusionUpdateRate.EveryXFrames) && Time.frameCount >= m_LastFrameRendered + waitXFrames)
			{
				flag = true;
			}
			if (flag)
			{
				ProcessOcclusion(ProcessOcclusionSource.RenderLoop);
			}
		}
	}

	private void DisableOcclusion()
	{
		m_Master._INTERNAL_SetDynamicOcclusionCallback(GetShaderKeyword(), null);
	}
}
