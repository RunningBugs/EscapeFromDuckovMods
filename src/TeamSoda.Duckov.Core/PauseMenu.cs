public class PauseMenu : UIPanel
{
	public static PauseMenu Instance => GameManager.PauseMenu;

	public bool Shown
	{
		get
		{
			if (fadeGroup == null)
			{
				return false;
			}
			return fadeGroup.IsShown;
		}
	}

	public static void Show()
	{
		Instance.Open();
	}

	public static void Hide()
	{
		Instance.Close();
	}

	public static void Toggle()
	{
		if (Instance.fadeGroup.IsShown)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}
}
