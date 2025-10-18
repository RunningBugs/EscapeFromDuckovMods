using System;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.UI.Animations;
using ItemStatsSystem;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fishing.UI;

public class ConfirmPanel : MonoBehaviour
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	[LocalizationKey("Default")]
	private string succeedTextKey = "Fishing_Succeed";

	[SerializeField]
	[LocalizationKey("Default")]
	private string failedTextKey = "Fishing_Failed";

	[SerializeField]
	private ItemDisplay itemDisplay;

	[SerializeField]
	private Button continueButton;

	[SerializeField]
	private Button quitButton;

	private bool confirmed;

	private bool continueFishing;

	private void Awake()
	{
		continueButton.onClick.AddListener(OnContinueButtonClicked);
		quitButton.onClick.AddListener(OnQuitButtonClicked);
		itemDisplay.onPointerClick += OnItemDisplayClick;
	}

	private void OnItemDisplayClick(ItemDisplay display, PointerEventData data)
	{
		data.Use();
	}

	private void OnContinueButtonClicked()
	{
		confirmed = true;
		continueFishing = true;
	}

	private void OnQuitButtonClicked()
	{
		confirmed = true;
		continueFishing = false;
	}

	internal async UniTask DoConfirmDialogue(Item catchedItem, Action<bool> confirmCallback)
	{
		Setup(catchedItem);
		fadeGroup.Show();
		confirmed = false;
		while (base.gameObject.activeInHierarchy && !confirmed)
		{
			await UniTask.Yield();
		}
		confirmCallback(continueFishing);
		fadeGroup.Hide();
	}

	private void Setup(Item item)
	{
		if (item == null)
		{
			titleText.text = failedTextKey.ToPlainText();
			itemDisplay.gameObject.SetActive(value: false);
		}
		else
		{
			titleText.text = succeedTextKey.ToPlainText();
			itemDisplay.Setup(item);
			itemDisplay.gameObject.SetActive(value: true);
		}
	}

	internal void NotifyStop()
	{
		fadeGroup.Hide();
	}
}
