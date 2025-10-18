using System;
using Duckov.Rules;
using Saves;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI.MainMenu;

public class SaveSlotSelectionButton : MonoBehaviour
{
	[SerializeField]
	private SaveSlotSelectionMenu menu;

	[SerializeField]
	private Button button;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private TextMeshProUGUI difficultyText;

	[SerializeField]
	private TextMeshProUGUI playTimeText;

	[SerializeField]
	private TextMeshProUGUI saveTimeText;

	[SerializeField]
	private string slotTextKey = "MainMenu_SaveSelection_Slot";

	[SerializeField]
	private string format = "{slotText} {index}";

	[LocalizationKey("Default")]
	[SerializeField]
	private string newGameTextKey = "NewGame";

	[SerializeField]
	private GameObject activeIndicator;

	[SerializeField]
	private GameObject oldSlotIndicator;

	[Min(1f)]
	[SerializeField]
	private int index;

	private void Awake()
	{
		button.onClick.AddListener(OnButtonClick);
	}

	private void OnDestroy()
	{
	}

	private void OnEnable()
	{
		SavesSystem.OnSetFile += Refresh;
		Refresh();
	}

	private void OnDisable()
	{
		SavesSystem.OnSetFile -= Refresh;
	}

	private void OnButtonClick()
	{
		SavesSystem.SetFile(index);
		menu.Finish();
	}

	private void OnValidate()
	{
		if (button == null)
		{
			button = GetComponent<Button>();
		}
		if (text == null)
		{
			text = GetComponentInChildren<TextMeshProUGUI>();
		}
		Refresh();
	}

	private void Refresh()
	{
		new ES3Settings(SavesSystem.GetFilePath(index)).location = ES3.Location.File;
		this.text.text = format.Format(new
		{
			slotText = slotTextKey.ToPlainText(),
			index = index
		});
		bool active = SavesSystem.CurrentSlot == index;
		activeIndicator?.SetActive(active);
		if (SavesSystem.IsOldGame(index))
		{
			difficultyText.text = GameRulesManager.GetRuleIndexDisplayNameOfSlot(index) ?? "";
			playTimeText.gameObject.SetActive(value: true);
			TimeSpan realTimePlayedOfSaveSlot = GameClock.GetRealTimePlayedOfSaveSlot(index);
			playTimeText.text = $"{Mathf.FloorToInt((float)realTimePlayedOfSaveSlot.TotalHours):00}:{realTimePlayedOfSaveSlot.Minutes:00}";
			bool active2 = SavesSystem.IsOldSave(index);
			oldSlotIndicator.SetActive(active2);
			long num = SavesSystem.Load<long>("SaveTime", index);
			string text = ((num > 0) ? DateTime.FromBinary(num).ToLocalTime().ToString("yyyy/MM/dd HH:mm") : "???");
			saveTimeText.text = text;
		}
		else
		{
			difficultyText.text = newGameTextKey.ToPlainText();
			playTimeText.gameObject.SetActive(value: false);
			oldSlotIndicator.SetActive(value: false);
			saveTimeText.text = "----/--/-- --:--";
		}
	}
}
