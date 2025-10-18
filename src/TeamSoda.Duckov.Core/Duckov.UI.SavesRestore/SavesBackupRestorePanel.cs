using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using Duckov.Utilities;
using Saves;
using TMPro;
using UnityEngine;

namespace Duckov.UI.SavesRestore;

public class SavesBackupRestorePanel : MonoBehaviour
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private FadeGroup confirmFadeGroup;

	[SerializeField]
	private FadeGroup resultFadeGroup;

	[SerializeField]
	private TextMeshProUGUI[] slotIndexTexts;

	[SerializeField]
	private TextMeshProUGUI[] backupTimeTexts;

	[SerializeField]
	private SavesBackupRestorePanelEntry template;

	[SerializeField]
	private GameObject noBackupIndicator;

	private PrefabPool<SavesBackupRestorePanelEntry> _pool;

	private int slot;

	private bool recovering;

	private bool confirm;

	private bool cancel;

	private PrefabPool<SavesBackupRestorePanelEntry> Pool
	{
		get
		{
			if (_pool == null)
			{
				_pool = new PrefabPool<SavesBackupRestorePanelEntry>(template);
			}
			return _pool;
		}
	}

	private void Awake()
	{
	}

	public void Open(int savesSlot)
	{
		slot = savesSlot;
		Refresh();
		fadeGroup.Show();
	}

	public void Close()
	{
		fadeGroup.Hide();
	}

	public void Confirm()
	{
		confirm = true;
	}

	public void Cancel()
	{
		cancel = true;
	}

	private void Refresh()
	{
		Pool.ReleaseAll();
		List<SavesSystem.BackupInfo> list = SavesSystem.GetBackupList(slot).ToList();
		list.Sort((SavesSystem.BackupInfo a, SavesSystem.BackupInfo b) => (a.Time < b.Time) ? 1 : (-1));
		int num = 0;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			SavesSystem.BackupInfo info = list[num2];
			if (info.exists)
			{
				Pool.Get().Setup(this, info);
				num++;
			}
		}
		noBackupIndicator.SetActive(num <= 0);
	}

	internal void NotifyClicked(SavesBackupRestorePanelEntry button)
	{
		if (!recovering)
		{
			SavesSystem.BackupInfo info = button.Info;
			if (info.exists)
			{
				RecoverTask(info).Forget();
			}
		}
	}

	private async UniTask RecoverTask(SavesSystem.BackupInfo info)
	{
		recovering = true;
		confirm = false;
		cancel = false;
		TextMeshProUGUI[] array = slotIndexTexts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].text = $"{info.slot}";
		}
		array = backupTimeTexts;
		foreach (TextMeshProUGUI textMeshProUGUI in array)
		{
			if (info.time_raw <= 0)
			{
				textMeshProUGUI.text = "???";
			}
			textMeshProUGUI.text = info.Time.ToLocalTime().ToString("yyyy/MM/dd HH:mm");
		}
		confirmFadeGroup.Show();
		while (!confirm && !cancel)
		{
			await UniTask.Yield();
		}
		if (cancel)
		{
			confirmFadeGroup.Hide();
			recovering = false;
			return;
		}
		SavesSystem.RestoreIndexedBackup(info.slot, info.index);
		confirmFadeGroup.Hide();
		confirm = false;
		resultFadeGroup.Show();
		while (!confirm)
		{
			await UniTask.Yield();
		}
		confirmFadeGroup.Hide();
		resultFadeGroup.Hide();
		recovering = false;
		Close();
	}
}
