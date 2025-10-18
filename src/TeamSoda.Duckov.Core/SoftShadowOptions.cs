using Duckov.Options;
using SodaCraft.Localizations;

public class SoftShadowOptions : OptionsProviderBase
{
	[LocalizationKey("Default")]
	public string offKey = "SoftShadowOptions_Off";

	[LocalizationKey("Default")]
	public string onKey = "SoftShadowOptions_On";

	public override string Key => "SoftShadowSettings";

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
	}
}
