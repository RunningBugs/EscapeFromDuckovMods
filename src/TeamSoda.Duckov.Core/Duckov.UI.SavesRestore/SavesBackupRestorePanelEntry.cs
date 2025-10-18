using Saves;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.UI.SavesRestore;

public class SavesBackupRestorePanelEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private TextMeshProUGUI timeText;

	private SavesBackupRestorePanel master;

	private SavesSystem.BackupInfo info;

	public SavesSystem.BackupInfo Info => info;

	public void OnPointerClick(PointerEventData eventData)
	{
		master.NotifyClicked(this);
	}

	internal void Setup(SavesBackupRestorePanel master, SavesSystem.BackupInfo info)
	{
		this.master = master;
		this.info = info;
		if (info.time_raw <= 0)
		{
			timeText.text = "???";
		}
		else
		{
			timeText.text = info.Time.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss");
		}
	}
}
