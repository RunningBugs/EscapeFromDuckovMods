using Duckov.Options;
using LeTai.TrueShadow;
using SodaCraft.Localizations;

public class UIShadowOptions : OptionsProviderBase
{
	private const string key = "UIShadow";

	public override string Key => "UIShadow";

	public static bool Active
	{
		get
		{
			return OptionsManager.Load("UIShadow", defaultValue: true);
		}
		set
		{
			OptionsManager.Save("UIShadow", value);
		}
	}

	public string ActiveText => "Options_On".ToPlainText();

	public string InactiveText => "Options_Off".ToPlainText();

	public static void Apply()
	{
		TrueShadow.ExternalActive = Active;
	}

	public override string GetCurrentOption()
	{
		if (Active)
		{
			return ActiveText;
		}
		return InactiveText;
	}

	public override string[] GetOptions()
	{
		return new string[2] { InactiveText, ActiveText };
	}

	public override void Set(int index)
	{
		if (index <= 0)
		{
			Active = false;
		}
		else
		{
			Active = true;
		}
	}
}
