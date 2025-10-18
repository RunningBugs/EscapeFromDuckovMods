using Duckov.Options;
using SodaCraft.Localizations;
using Umbra;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ShadowSetting : OptionsProviderBase
{
	public UmbraProfile umbraProfile;

	public float onDistance = 100f;

	[LocalizationKey("Default")]
	public string highKey = "Options_High";

	[LocalizationKey("Default")]
	public string middleKey = "Options_Middle";

	[LocalizationKey("Default")]
	public string lowKey = "Options_Low";

	[LocalizationKey("Default")]
	public string offKey = "Options_Off";

	public override string Key => "ShadowSettings";

	public override string[] GetOptions()
	{
		return new string[4]
		{
			offKey.ToPlainText(),
			lowKey.ToPlainText(),
			middleKey.ToPlainText(),
			highKey.ToPlainText()
		};
	}

	public override string GetCurrentOption()
	{
		return OptionsManager.Load(Key, 2) switch
		{
			0 => offKey.ToPlainText(), 
			1 => lowKey.ToPlainText(), 
			2 => middleKey.ToPlainText(), 
			3 => highKey.ToPlainText(), 
			_ => highKey.ToPlainText(), 
		};
	}

	private void SetShadow(bool on, int res, float shadowDistance, bool softShadow, bool softShadowDownSample, bool contactShadow, int pointLightCount)
	{
		UniversalRenderPipelineAsset universalRenderPipelineAsset = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
		if (universalRenderPipelineAsset != null)
		{
			universalRenderPipelineAsset.shadowDistance = (on ? shadowDistance : 0f);
			universalRenderPipelineAsset.mainLightShadowmapResolution = res;
			universalRenderPipelineAsset.additionalLightsShadowmapResolution = res;
			universalRenderPipelineAsset.maxAdditionalLightsCount = pointLightCount;
		}
		if ((bool)umbraProfile)
		{
			umbraProfile.shadowSource = (softShadow ? ShadowSource.UmbraShadows : ShadowSource.UnityShadows);
			umbraProfile.downsample = softShadowDownSample;
			umbraProfile.contactShadows = contactShadow;
		}
	}

	public override void Set(int index)
	{
		switch (index)
		{
		case 0:
			SetShadow(on: false, 512, 0f, softShadow: false, softShadowDownSample: false, contactShadow: false, 0);
			break;
		case 1:
			SetShadow(on: true, 1024, 70f, softShadow: false, softShadowDownSample: false, contactShadow: false, 0);
			break;
		case 2:
			SetShadow(on: true, 2048, 80f, softShadow: true, softShadowDownSample: true, contactShadow: true, 5);
			break;
		case 3:
			SetShadow(on: true, 4096, 90f, softShadow: true, softShadowDownSample: false, contactShadow: true, 6);
			break;
		}
		OptionsManager.Save(Key, index);
	}

	private void Awake()
	{
		LevelManager.OnLevelInitialized += RefreshOnLevelInited;
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelInitialized -= RefreshOnLevelInited;
	}

	private void RefreshOnLevelInited()
	{
		int index = OptionsManager.Load(Key, 2);
		Set(index);
	}
}
