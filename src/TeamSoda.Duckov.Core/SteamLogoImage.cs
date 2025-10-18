using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class SteamLogoImage : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private Sprite steamLogo;

	[SerializeField]
	private Sprite steamChinaLogo;

	private void Start()
	{
		Refresh();
	}

	private void Refresh()
	{
		if (!SteamManager.Initialized)
		{
			image.sprite = steamLogo;
		}
		else if (SteamUtils.IsSteamChinaLauncher())
		{
			image.sprite = steamChinaLogo;
		}
		else
		{
			image.sprite = steamLogo;
		}
	}
}
