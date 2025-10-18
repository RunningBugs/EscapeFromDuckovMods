using Duckov.Options;
using SodaCraft.Localizations;

public static class ScrollWheelBehaviour
{
	public enum Behaviour
	{
		AmmoAndInteract,
		Weapon
	}

	public static Behaviour CurrentBehaviour
	{
		get
		{
			return OptionsManager.Load("ScrollWheelBehaviour", Behaviour.AmmoAndInteract);
		}
		set
		{
			OptionsManager.Save("ScrollWheelBehaviour", value);
		}
	}

	public static string GetDisplayName(Behaviour behaviour)
	{
		return $"ScrollWheelBehaviour_{behaviour}".ToPlainText();
	}
}
