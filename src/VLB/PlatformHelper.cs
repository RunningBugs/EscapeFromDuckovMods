using UnityEngine;

namespace VLB;

public class PlatformHelper
{
	public static string GetCurrentPlatformSuffix()
	{
		return GetPlatformSuffix(Application.platform);
	}

	private static string GetPlatformSuffix(RuntimePlatform platform)
	{
		return platform.ToString();
	}
}
