using Cysharp.Threading.Tasks;
using Duckov.Scenes;
using Duckov.Utilities;
using Eflatun.SceneReference;
using Saves;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI.MainMenu;

public class ContinueButton : MonoBehaviour
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private TextMeshProUGUI text;

	[LocalizationKey("Default")]
	[SerializeField]
	private string text_NewGame = "新游戏";

	[LocalizationKey("Default")]
	[SerializeField]
	private string text_Continue = "继续";

	[SerializeField]
	private SceneReference overrideCurtainScene;

	[SerializeField]
	private string Text_NewGame => text_NewGame.ToPlainText();

	[SerializeField]
	private string Text_Continue => text_Continue.ToPlainText();

	private void Awake()
	{
		SavesSystem.OnSetFile += Refresh;
		SavesSystem.OnSaveDeleted += Refresh;
		button.onClick.AddListener(OnButtonClicked);
		LocalizationManager.OnSetLanguage += OnSetLanguage;
	}

	private void OnDestroy()
	{
		SavesSystem.OnSetFile -= Refresh;
		SavesSystem.OnSaveDeleted -= Refresh;
		LocalizationManager.OnSetLanguage -= OnSetLanguage;
	}

	private void OnSetLanguage(SystemLanguage language)
	{
		Refresh();
	}

	private void OnButtonClicked()
	{
		GameManager.newBoot = true;
		if (MultiSceneCore.GetVisited("Base"))
		{
			SceneLoader.Instance.LoadBaseScene().Forget();
			return;
		}
		SavesSystem.Save("CreatedWithVersion", GameMetaData.Instance.Version);
		SceneLoader.Instance.LoadScene(GameplayDataSettings.SceneManagement.PrologueScene, overrideCurtainScene).Forget();
	}

	private void Start()
	{
		Refresh();
	}

	private void Refresh()
	{
		bool flag = SavesSystem.IsOldGame();
		text.text = (flag ? Text_Continue : Text_NewGame);
	}
}
