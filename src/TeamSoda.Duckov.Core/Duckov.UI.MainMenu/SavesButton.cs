using Cysharp.Threading.Tasks;
using Duckov.Rules;
using Duckov.UI.Animations;
using Saves;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI.MainMenu;

public class SavesButton : MonoBehaviour
{
	[SerializeField]
	private FadeGroup currentMenuFadeGroup;

	[SerializeField]
	private SaveSlotSelectionMenu selectionMenu;

	[SerializeField]
	private GameObject oldSaveIndicator;

	[SerializeField]
	private Button button;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	[LocalizationKey("Default")]
	private string textKey = "MainMenu_SaveSlot";

	[SerializeField]
	private string textFormat = "{text}: {slotNumber}";

	private bool executing;

	private void Awake()
	{
		button.onClick.AddListener(OnButtonClick);
		SavesSystem.OnSetFile += Refresh;
		LocalizationManager.OnSetLanguage += OnSetLanguage;
		SavesSystem.OnSaveDeleted += Refresh;
	}

	private void OnDestroy()
	{
		SavesSystem.OnSetFile -= Refresh;
		LocalizationManager.OnSetLanguage -= OnSetLanguage;
		SavesSystem.OnSaveDeleted -= Refresh;
	}

	private void OnSetLanguage(SystemLanguage language)
	{
		Refresh();
	}

	private void OnButtonClick()
	{
		if (!executing)
		{
			SavesSelectionTask().Forget();
		}
	}

	private async UniTask SavesSelectionTask()
	{
		executing = true;
		currentMenuFadeGroup.Hide();
		await selectionMenu.Execute();
		currentMenuFadeGroup.Show();
		executing = false;
	}

	private void Start()
	{
		Refresh();
	}

	private void Refresh()
	{
		bool num = SavesSystem.IsOldGame();
		string difficulty = (num ? GameRulesManager.Current.DisplayName : "");
		text.text = textFormat.Format(new
		{
			text = textKey.ToPlainText(),
			slotNumber = SavesSystem.CurrentSlot,
			difficulty = difficulty
		});
		bool active = num && SavesSystem.IsOldSave(SavesSystem.CurrentSlot);
		oldSaveIndicator.SetActive(active);
	}
}
