using Duckov.MiniGames;
using Duckov.Options;
using SodaCraft.Localizations;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FsrSetting : OptionsProviderBase
{
	[LocalizationKey("Default")]
	public string offKey = "fsr_Off";

	[LocalizationKey("Default")]
	public string qualityKey = "fsr_Quality";

	[LocalizationKey("Default")]
	public string balancedKey = "fsr_Balanced";

	[LocalizationKey("Default")]
	public string performanceKey = "fsr_Performance";

	[LocalizationKey("Default")]
	public string ultraPerformanceKey = "fsr_UltraPerformance";

	private static bool gameOn;

	public override string Key => "FsrSetting";

	public override string[] GetOptions()
	{
		return new string[5]
		{
			offKey.ToPlainText(),
			qualityKey.ToPlainText(),
			balancedKey.ToPlainText(),
			performanceKey.ToPlainText(),
			ultraPerformanceKey.ToPlainText()
		};
	}

	public override string GetCurrentOption()
	{
		return OptionsManager.Load(Key, 0) switch
		{
			0 => offKey.ToPlainText(), 
			1 => qualityKey.ToPlainText(), 
			2 => balancedKey.ToPlainText(), 
			3 => performanceKey.ToPlainText(), 
			4 => ultraPerformanceKey.ToPlainText(), 
			_ => offKey.ToPlainText(), 
		};
	}

	public override void Set(int index)
	{
		UniversalRenderPipelineAsset universalRenderPipelineAsset = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
		int num = index;
		if (gameOn)
		{
			num = 0;
		}
		switch (num)
		{
		case 0:
			if (universalRenderPipelineAsset != null)
			{
				universalRenderPipelineAsset.renderScale = 1f;
				universalRenderPipelineAsset.upscalingFilter = UpscalingFilterSelection.Linear;
			}
			break;
		case 1:
			if (universalRenderPipelineAsset != null)
			{
				universalRenderPipelineAsset.renderScale = 0.67f;
				universalRenderPipelineAsset.upscalingFilter = UpscalingFilterSelection.FSR;
			}
			break;
		case 2:
			if (universalRenderPipelineAsset != null)
			{
				universalRenderPipelineAsset.renderScale = 0.58f;
				universalRenderPipelineAsset.upscalingFilter = UpscalingFilterSelection.FSR;
			}
			break;
		case 3:
			if (universalRenderPipelineAsset != null)
			{
				universalRenderPipelineAsset.renderScale = 0.5f;
				universalRenderPipelineAsset.upscalingFilter = UpscalingFilterSelection.FSR;
			}
			break;
		case 4:
			if (universalRenderPipelineAsset != null)
			{
				universalRenderPipelineAsset.renderScale = 0.33f;
				universalRenderPipelineAsset.upscalingFilter = UpscalingFilterSelection.FSR;
			}
			break;
		}
		OptionsManager.Save(Key, index);
	}

	private void Awake()
	{
		RefreshOnLevelInited();
		LevelManager.OnLevelInitialized += RefreshOnLevelInited;
		GamingConsole.OnGamingConsoleInteractChanged += OnGamingConsoleInteractChanged;
	}

	private void OnGamingConsoleInteractChanged(bool _gameOn)
	{
		gameOn = _gameOn;
		SyncSetting();
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelInitialized -= RefreshOnLevelInited;
	}

	private void SyncSetting()
	{
		int index = OptionsManager.Load(Key, 0);
		Set(index);
	}

	private void RefreshOnLevelInited()
	{
		SyncSetting();
	}
}
