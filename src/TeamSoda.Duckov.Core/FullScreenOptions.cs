using Duckov.Options;

public class FullScreenOptions : OptionsProviderBase
{
	public override string Key => ResolutionSetter.Key_ScreenMode;

	public override string GetCurrentOption()
	{
		return ResolutionSetter.ScreenModeToName(OptionsManager.Load(Key, ResolutionSetter.screenModes.Borderless));
	}

	public override string[] GetOptions()
	{
		return ResolutionSetter.GetScreenModes();
	}

	public override void Set(int index)
	{
		OptionsManager.Save(Key, (ResolutionSetter.screenModes)index);
	}
}
