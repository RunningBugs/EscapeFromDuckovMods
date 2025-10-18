using Duckov.Options;
using SodaCraft.Localizations;

public class EdgeLightSettings : OptionsProviderBase
{
	[LocalizationKey("Default")]
	public string onKey = "Options_On";

	[LocalizationKey("Default")]
	public string offKey = "Options_Off";

	public override string Key => "EdgeLightSetting";

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
			_ => onKey.ToPlainText(), 
		};
	}

	public override void Set(int index)
	{
		switch (index)
		{
		case 0:
			EdgeLightEntry.SetEnabled(enabled: false);
			break;
		case 1:
			EdgeLightEntry.SetEnabled(enabled: true);
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
		int index = OptionsManager.Load(Key, 1);
		Set(index);
	}
}
