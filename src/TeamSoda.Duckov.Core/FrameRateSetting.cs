using Duckov.Options;
using SodaCraft.Localizations;
using UnityEngine;

public class FrameRateSetting : OptionsProviderBase
{
	[LocalizationKey("Default")]
	public string optionUnlimitKey = "FrameRateUnlimit";

	public override string Key => "FrameRateSetting";

	public override string[] GetOptions()
	{
		return new string[6]
		{
			"60",
			"90",
			"120",
			"144",
			"240",
			optionUnlimitKey.ToPlainText()
		};
	}

	public override string GetCurrentOption()
	{
		return OptionsManager.Load(Key, 0) switch
		{
			0 => "60", 
			1 => "90", 
			2 => "120", 
			3 => "144", 
			4 => "240", 
			5 => optionUnlimitKey.ToPlainText(), 
			_ => "60", 
		};
	}

	public override void Set(int index)
	{
		switch (index)
		{
		case 0:
			Application.targetFrameRate = 60;
			break;
		case 1:
			Application.targetFrameRate = 90;
			break;
		case 2:
			Application.targetFrameRate = 120;
			break;
		case 3:
			Application.targetFrameRate = 144;
			break;
		case 4:
			Application.targetFrameRate = 240;
			break;
		case 5:
			Application.targetFrameRate = 500;
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
		int index = OptionsManager.Load(Key, 0);
		Set(index);
	}
}
