using Duckov.Options;
using SodaCraft.Localizations;

public class MoveDirectionOptions : OptionsProviderBase
{
	[LocalizationKey("Default")]
	public string cameraModeKey = "MoveDirectionMode_Camera";

	[LocalizationKey("Default")]
	public string aimModeKey = "MoveDirectionMode_Aim";

	private static bool moveViaCharacterDirection;

	public override string Key => "MoveDirModeSettings";

	public static bool MoveViaCharacterDirection => moveViaCharacterDirection;

	public override string[] GetOptions()
	{
		return new string[2]
		{
			cameraModeKey.ToPlainText(),
			aimModeKey.ToPlainText()
		};
	}

	public override string GetCurrentOption()
	{
		return OptionsManager.Load(Key, 0) switch
		{
			0 => cameraModeKey.ToPlainText(), 
			1 => aimModeKey.ToPlainText(), 
			_ => cameraModeKey.ToPlainText(), 
		};
	}

	public override void Set(int index)
	{
		switch (index)
		{
		case 0:
			moveViaCharacterDirection = false;
			break;
		case 1:
			moveViaCharacterDirection = true;
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
