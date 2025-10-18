using Duckov.Options;
using SodaCraft.Localizations;
using SymmetryBreakStudio.TastyGrassShader;
using UnityEngine.Rendering.Universal;

public class GrassOptions : OptionsProviderBase
{
	[LocalizationKey("Default")]
	public string offKey = "GrassOptions_Off";

	[LocalizationKey("Default")]
	public string onKey = "GrassOptions_On";

	public UniversalRendererData rendererData;

	public override string Key => "GrassSettings";

	public override string[] GetOptions()
	{
		return new string[2]
		{
			offKey.ToPlainText(),
			onKey.ToPlainText()
		};
	}

	public override string GetCurrentOption()
	{
		return OptionsManager.Load(Key, 1) switch
		{
			0 => offKey.ToPlainText(), 
			1 => onKey.ToPlainText(), 
			_ => offKey.ToPlainText(), 
		};
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
		int index = OptionsManager.Load(Key, 1);
		Set(index);
	}

	public override void Set(int index)
	{
		ScriptableRendererFeature scriptableRendererFeature = rendererData.rendererFeatures.Find((ScriptableRendererFeature e) => e is TastyGrassShaderGlobalSettings);
		if (scriptableRendererFeature != null)
		{
			TastyGrassShaderGlobalSettings tastyGrassShaderGlobalSettings = scriptableRendererFeature as TastyGrassShaderGlobalSettings;
			switch (index)
			{
			case 0:
				tastyGrassShaderGlobalSettings.SetActive(active: false);
				TgsManager.Enable = false;
				break;
			case 1:
				tastyGrassShaderGlobalSettings.SetActive(active: true);
				TgsManager.Enable = true;
				break;
			}
		}
		OptionsManager.Save(Key, index);
	}
}
