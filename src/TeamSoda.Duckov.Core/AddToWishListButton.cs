using Duckov;
using SodaCraft.Localizations;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;

public class AddToWishListButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private const string url = "https://store.steampowered.com/app/3167020/";

	private const string CNUrl = "https://game.bilibili.com/duckov/";

	private const string ENUrl = "https://www.duckov.com";

	private const uint appid = 3167020u;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			ShowPage();
		}
	}

	public static void ShowPage()
	{
		if (SteamManager.Initialized)
		{
			SteamFriends.ActivateGameOverlayToStore(new AppId_t(3167020u), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
		}
		else if (GameMetaData.Instance.Platform == Platform.Steam)
		{
			Application.OpenURL("https://store.steampowered.com/app/3167020/");
		}
		else if (LocalizationManager.CurrentLanguage == SystemLanguage.ChineseSimplified)
		{
			Application.OpenURL("https://game.bilibili.com/duckov/");
		}
		else
		{
			Application.OpenURL("https://www.duckov.com");
		}
	}

	private void Start()
	{
		if (!SteamManager.Initialized)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
