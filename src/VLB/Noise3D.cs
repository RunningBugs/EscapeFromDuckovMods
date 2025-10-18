using UnityEngine;

namespace VLB;

public static class Noise3D
{
	private static bool ms_IsSupportedChecked;

	private static bool ms_IsSupported;

	private static Texture3D ms_NoiseTexture;

	private const int kMinShaderLevel = 35;

	public static bool isSupported
	{
		get
		{
			if (!ms_IsSupportedChecked)
			{
				ms_IsSupported = SystemInfo.graphicsShaderLevel >= 35;
				if (!ms_IsSupported)
				{
					Debug.LogWarning(isNotSupportedString);
				}
				ms_IsSupportedChecked = true;
			}
			return ms_IsSupported;
		}
	}

	public static bool isProperlyLoaded => ms_NoiseTexture != null;

	public static string isNotSupportedString => $"3D Noise requires higher shader capabilities (Shader Model 3.5 / OpenGL ES 3.0), which are not available on the current platform: graphicsShaderLevel (current/required) = {SystemInfo.graphicsShaderLevel} / {35}";

	[RuntimeInitializeOnLoadMethod]
	private static void OnStartUp()
	{
		LoadIfNeeded();
	}

	public static void LoadIfNeeded()
	{
		if (isSupported && ms_NoiseTexture == null)
		{
			ms_NoiseTexture = Config.Instance.noiseTexture3D;
			Shader.SetGlobalTexture(ShaderProperties.GlobalNoiseTex3D, ms_NoiseTexture);
			Shader.SetGlobalFloat(ShaderProperties.GlobalNoiseCustomTime, -1f);
		}
	}
}
