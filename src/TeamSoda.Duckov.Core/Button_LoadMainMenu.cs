using Cysharp.Threading.Tasks;
using Saves;
using UnityEngine;
using UnityEngine.UI;

public class Button_LoadMainMenu : MonoBehaviour
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private ConfirmDialogue dialogue;

	private UniTask task;

	private void Awake()
	{
		button.onClick.AddListener(BeginQuitting);
		dialogue.SkipHide();
	}

	private void BeginQuitting()
	{
		if (task.Status != UniTaskStatus.Pending)
		{
			Debug.Log("Quitting");
			task = QuitTask();
		}
	}

	private async UniTask QuitTask()
	{
		if (LevelManager.Instance != null && LevelManager.Instance.IsBaseLevel)
		{
			LevelManager.Instance.SaveMainCharacter();
			SavesSystem.CollectSaveData();
			SavesSystem.SaveFile();
		}
		else if (!(await dialogue.Execute()))
		{
			return;
		}
		while (SavesSystem.IsSaving)
		{
			await UniTask.Yield();
		}
		SceneLoader.LoadMainMenu();
		PauseMenu.Hide();
	}
}
