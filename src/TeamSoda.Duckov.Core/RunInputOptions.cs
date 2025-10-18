using Duckov.Options;
using SodaCraft.Localizations;

public class RunInputOptions : OptionsProviderBase
{
	[LocalizationKey("Default")]
	public string holdModeKey = "RunInputMode_Hold";

	[LocalizationKey("Default")]
	public string switchModeKey = "RunInputMode_Switch";

	public override string Key => "RunInputModeSettings";

	public override string[] GetOptions()
	{
		return new string[2]
		{
			holdModeKey.ToPlainText(),
			switchModeKey.ToPlainText()
		};
	}

	public override string GetCurrentOption()
	{
		return OptionsManager.Load(Key, 0) switch
		{
			0 => holdModeKey.ToPlainText(), 
			1 => switchModeKey.ToPlainText(), 
			_ => holdModeKey.ToPlainText(), 
		};
	}

	public override void Set(int index)
	{
		switch (index)
		{
		case 0:
			InputManager.useRunInputBuffer = false;
			break;
		case 1:
			InputManager.useRunInputBuffer = true;
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
