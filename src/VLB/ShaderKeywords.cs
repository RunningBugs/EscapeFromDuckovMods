namespace VLB;

public static class ShaderKeywords
{
	public static class SD
	{
		public const string DepthBlend = "VLB_DEPTH_BLEND";

		public const string OcclusionClippingPlane = "VLB_OCCLUSION_CLIPPING_PLANE";

		public const string OcclusionDepthTexture = "VLB_OCCLUSION_DEPTH_TEXTURE";

		public const string MeshSkewing = "VLB_MESH_SKEWING";

		public const string ShaderAccuracyHigh = "VLB_SHADER_ACCURACY_HIGH";
	}

	public static class HD
	{
		public const string AttenuationLinear = "VLB_ATTENUATION_LINEAR";

		public const string AttenuationQuad = "VLB_ATTENUATION_QUAD";

		public const string Shadow = "VLB_SHADOW";

		public const string CookieSingleChannel = "VLB_COOKIE_1CHANNEL";

		public const string CookieRGBA = "VLB_COOKIE_RGBA";

		public const string RaymarchingStepCount = "VLB_RAYMARCHING_STEP_COUNT";

		public static string GetRaymarchingQuality(int id)
		{
			return "VLB_RAYMARCHING_QUALITY_" + id;
		}
	}

	public const string AlphaAsBlack = "VLB_ALPHA_AS_BLACK";

	public const string ColorGradientMatrixLow = "VLB_COLOR_GRADIENT_MATRIX_LOW";

	public const string ColorGradientMatrixHigh = "VLB_COLOR_GRADIENT_MATRIX_HIGH";

	public const string Noise3D = "VLB_NOISE_3D";
}
