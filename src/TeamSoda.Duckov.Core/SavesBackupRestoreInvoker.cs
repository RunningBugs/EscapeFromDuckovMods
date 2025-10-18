using Duckov.UI.Animations;
using Duckov.UI.SavesRestore;
using Saves;
using UnityEngine;
using UnityEngine.UI;

public class SavesBackupRestoreInvoker : MonoBehaviour
{
	[SerializeField]
	private Button mainButton;

	[SerializeField]
	private FadeGroup menuFadeGroup;

	[SerializeField]
	private Button buttonSlot1;

	[SerializeField]
	private Button buttonSlot2;

	[SerializeField]
	private Button buttonSlot3;

	[SerializeField]
	private SavesBackupRestorePanel restorePanel;

	private void Awake()
	{
		mainButton.onClick.AddListener(OnMainButtonClicked);
		buttonSlot1.onClick.AddListener(delegate
		{
			OnButtonClicked(1);
		});
		buttonSlot2.onClick.AddListener(delegate
		{
			OnButtonClicked(2);
		});
		buttonSlot3.onClick.AddListener(delegate
		{
			OnButtonClicked(3);
		});
	}

	private void OnMainButtonClicked()
	{
		menuFadeGroup.Toggle();
	}

	private void OnButtonClicked(int index)
	{
		menuFadeGroup.Hide();
		SavesSystem.SetFile(index);
		restorePanel.Open(index);
	}
}
