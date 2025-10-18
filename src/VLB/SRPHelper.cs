using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace VLB;

public static class SRPHelper
{
	private static bool m_IsRenderPipelineCached;

	private static RenderPipeline m_RenderPipelineCached;

	public static string renderPipelineScriptingDefineSymbolAsString => "VLB_URP";

	public static RenderPipeline projectRenderPipeline
	{
		get
		{
			if (!m_IsRenderPipelineCached)
			{
				m_RenderPipelineCached = ComputeRenderPipeline();
				m_IsRenderPipelineCached = true;
			}
			return m_RenderPipelineCached;
		}
	}

	private static RenderPipeline ComputeRenderPipeline()
	{
		RenderPipelineAsset defaultRenderPipeline = GraphicsSettings.defaultRenderPipeline;
		if ((bool)defaultRenderPipeline)
		{
			string text = defaultRenderPipeline.GetType().ToString();
			if (text.Contains("Universal"))
			{
				return RenderPipeline.URP;
			}
			if (text.Contains("Lightweight"))
			{
				return RenderPipeline.URP;
			}
			if (text.Contains("HD"))
			{
				return RenderPipeline.HDRP;
			}
		}
		return RenderPipeline.BuiltIn;
	}

	public static bool IsUsingCustomRenderPipeline()
	{
		if (RenderPipelineManager.currentPipeline == null)
		{
			return GraphicsSettings.defaultRenderPipeline != null;
		}
		return true;
	}

	public static void RegisterOnBeginCameraRendering(Action<ScriptableRenderContext, Camera> cb)
	{
		if (IsUsingCustomRenderPipeline())
		{
			RenderPipelineManager.beginCameraRendering -= cb;
			RenderPipelineManager.beginCameraRendering += cb;
		}
	}

	public static void UnregisterOnBeginCameraRendering(Action<ScriptableRenderContext, Camera> cb)
	{
		if (IsUsingCustomRenderPipeline())
		{
			RenderPipelineManager.beginCameraRendering -= cb;
		}
	}
}
