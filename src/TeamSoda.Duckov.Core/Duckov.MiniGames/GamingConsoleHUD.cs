using Duckov.UI;
using Duckov.UI.Animations;
using UnityEngine;

namespace Duckov.MiniGames;

public class GamingConsoleHUD : View
{
	[SerializeField]
	private FadeGroup contentFadeGroup;

	private static GamingConsoleHUD _instance_cache;

	private static GamingConsoleHUD Instance
	{
		get
		{
			if (_instance_cache == null)
			{
				_instance_cache = View.GetViewInstance<GamingConsoleHUD>();
			}
			return _instance_cache;
		}
	}

	public static void Show()
	{
		if (!(Instance == null))
		{
			Instance.LocalShow();
		}
	}

	public static void Hide()
	{
		if (!(Instance == null))
		{
			Instance.LocalHide();
		}
	}

	private void LocalShow()
	{
		contentFadeGroup.Show();
	}

	private void LocalHide()
	{
		contentFadeGroup.Hide();
	}
}
