using Duckov.Options;
using SodaCraft.Localizations;

public class DisableCameraOffset : OptionsProviderBase
{
	[LocalizationKey("Default")]
	public string onKey = "Options_On";

	[LocalizationKey("Default")]
	public string offKey = "Options_Off";

	public static bool disableCameraOffset;

	public override string Key => "DisableCameraOffset";

	public override string[] GetOptions()
	{
		return new string[2]
		{
			onKey.ToPlainText(),
			offKey.ToPlainText()
		};
	}

	public override string GetCurrentOption()
	{
		return OptionsManager.Load(Key, 1) switch
		{
			0 => onKey.ToPlainText(), 
			1 => offKey.ToPlainText(), 
			_ => offKey.ToPlainText(), 
		};
	}

	public override void Set(int index)
	{
		switch (index)
		{
		case 0:
			disableCameraOffset = true;
			break;
		case 1:
			disableCameraOffset = false;
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
