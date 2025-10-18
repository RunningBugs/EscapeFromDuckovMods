using Cysharp.Threading.Tasks;
using Saves;
using UnityEngine;
using UnityEngine.UI;

public class Button_QuitGame : MonoBehaviour
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private ConfirmDialogue dialogue;

	private UniTask task;

	private void Awake()
	{
		button.onClick.AddListener(BeginQuitting);
		if ((bool)dialogue)
		{
			dialogue.SkipHide();
		}
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
		else if ((bool)dialogue && !(await dialogue.Execute()))
		{
			return;
		}
		while (SavesSystem.IsSaving)
		{
			await UniTask.Yield();
		}
		if (Application.isEditor)
		{
			Debug.Log("即将调用Application.Quit()。但因为是Editor，不会真的退出。");
		}
		Application.Quit();
	}
}
