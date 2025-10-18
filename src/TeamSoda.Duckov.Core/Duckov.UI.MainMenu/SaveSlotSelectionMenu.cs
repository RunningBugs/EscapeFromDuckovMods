using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using Saves;
using UnityEngine;

namespace Duckov.UI.MainMenu;

public class SaveSlotSelectionMenu : MonoBehaviour
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private GameObject oldSaveIndicator;

	internal bool finished;

	private void OnEnable()
	{
		UIInputManager.OnCancel += OnCancel;
	}

	private void OnDisable()
	{
		UIInputManager.OnCancel -= OnCancel;
	}

	private void OnCancel(UIInputEventData data)
	{
		data.Use();
		Finish();
	}

	internal async UniTask Execute()
	{
		finished = false;
		oldSaveIndicator.SetActive(SavesSystem.IsOldSave(SavesSystem.CurrentSlot));
		await UniTask.WaitForSeconds(0.25f, ignoreTimeScale: true);
		fadeGroup.Show();
		while (!finished)
		{
			await UniTask.NextFrame();
		}
		oldSaveIndicator.SetActive(SavesSystem.IsOldSave(SavesSystem.CurrentSlot));
		await UniTask.WaitForSeconds(0.05f, ignoreTimeScale: true);
		fadeGroup.Hide();
		await UniTask.WaitForSeconds(0.25f, ignoreTimeScale: true);
	}

	public void Finish()
	{
		finished = true;
	}
}
