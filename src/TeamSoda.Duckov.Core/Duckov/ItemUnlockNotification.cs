using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using Duckov.Utilities;
using ItemStatsSystem;
using LeTai.TrueShadow;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov;

public class ItemUnlockNotification : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private FadeGroup mainFadeGroup;

	[SerializeField]
	private FadeGroup contentFadeGroup;

	[SerializeField]
	private Image image;

	[SerializeField]
	private TrueShadow shadow;

	[SerializeField]
	private TextMeshProUGUI textMain;

	[SerializeField]
	private TextMeshProUGUI textSub;

	[SerializeField]
	private float contentDelay = 0.5f;

	[SerializeField]
	[LocalizationKey("Default")]
	private string mainTextFormatKey = "UI_ItemUnlockNotification";

	[SerializeField]
	[LocalizationKey("Default")]
	private string subTextFormatKey = "UI_ItemUnlockNotification_Sub";

	private static List<int> pending = new List<int>();

	private UniTask showingTask;

	private bool pointerClicked;

	public string MainTextFormat => mainTextFormatKey.ToPlainText();

	private string SubTextFormat => subTextFormatKey.ToPlainText();

	public static ItemUnlockNotification Instance { get; private set; }

	private bool showing => showingTask.Status == UniTaskStatus.Pending;

	public static bool Showing
	{
		get
		{
			if (Instance == null)
			{
				return false;
			}
			return Instance.showing;
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	private void Update()
	{
		if (!showing && pending.Count > 0)
		{
			BeginShow();
		}
	}

	private void BeginShow()
	{
		showingTask = ShowTask();
	}

	private async UniTask ShowTask()
	{
		await mainFadeGroup.ShowAndReturnTask();
		await UniTask.WaitForSeconds(contentDelay, ignoreTimeScale: true);
		while (pending.Count > 0)
		{
			int itemTypeID = pending[0];
			pending.RemoveAt(0);
			await DisplayContent(itemTypeID);
		}
		await mainFadeGroup.HideAndReturnTask();
	}

	private async UniTask DisplayContent(int itemTypeID)
	{
		Setup(itemTypeID);
		await contentFadeGroup.ShowAndReturnTask();
		pointerClicked = false;
		while (!pointerClicked)
		{
			await UniTask.NextFrame();
		}
		await contentFadeGroup.HideAndReturnTask();
	}

	private void Setup(int itemTypeID)
	{
		ItemMetaData metaData = ItemAssetsCollection.GetMetaData(itemTypeID);
		string displayName = metaData.DisplayName;
		Sprite icon = metaData.icon;
		image.sprite = icon;
		textMain.text = MainTextFormat.Format(new
		{
			itemDisplayName = displayName
		});
		textSub.text = SubTextFormat;
		DisplayQuality displayQuality = metaData.displayQuality;
		GameplayDataSettings.UIStyle.GetDisplayQualityLook(displayQuality).Apply(shadow);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		pointerClicked = true;
	}

	public static void Push(int itemTypeID)
	{
		pending.Add(itemTypeID);
	}
}
