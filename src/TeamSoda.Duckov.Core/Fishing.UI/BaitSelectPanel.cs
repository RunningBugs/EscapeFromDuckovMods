using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.UI.Animations;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.UI;

namespace Fishing.UI;

public class BaitSelectPanel : MonoBehaviour, ISingleSelectionMenu<BaitSelectPanelEntry>
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private Button confirmButton;

	[SerializeField]
	private Button cancelButton;

	[SerializeField]
	private ItemDetailsDisplay details;

	[SerializeField]
	private FadeGroup detailsFadeGroup;

	[SerializeField]
	private BaitSelectPanelEntry entry;

	private PrefabPool<BaitSelectPanelEntry> _entryPool;

	private BaitSelectPanelEntry selectedEntry;

	private bool canceled;

	private bool confirmed;

	private PrefabPool<BaitSelectPanelEntry> EntryPool
	{
		get
		{
			if (_entryPool == null)
			{
				_entryPool = new PrefabPool<BaitSelectPanelEntry>(entry);
			}
			return _entryPool;
		}
	}

	private Item SelectedItem => selectedEntry?.Target;

	internal event Action onSetSelection;

	internal async UniTask DoBaitSelection(ICollection<Item> availableBaits, Func<Item, bool> baitSelectionResultCallback)
	{
		detailsFadeGroup.SkipHide();
		Setup(availableBaits);
		Open();
		baitSelectionResultCallback(await WaitForSelection());
		Close();
	}

	private void Open()
	{
		fadeGroup.Show();
	}

	private void Close()
	{
		fadeGroup.Hide();
	}

	private async UniTask<Item> WaitForSelection()
	{
		selectedEntry = null;
		canceled = false;
		confirmed = false;
		while (base.gameObject.activeInHierarchy && !confirmed && !canceled)
		{
			await UniTask.Yield();
		}
		if (canceled)
		{
			return null;
		}
		return SelectedItem;
	}

	private void Setup(ICollection<Item> availableBaits)
	{
		selectedEntry = null;
		EntryPool.ReleaseAll();
		foreach (Item availableBait in availableBaits)
		{
			EntryPool.Get().Setup(this, availableBait);
		}
	}

	internal void NotifyStop()
	{
		Close();
	}

	private void Awake()
	{
		confirmButton.onClick.AddListener(OnConfirmButtonClicked);
		cancelButton.onClick.AddListener(OnCancelButtonClicked);
	}

	private void OnConfirmButtonClicked()
	{
		if (SelectedItem == null)
		{
			NotificationText.Push("Fishing_PleaseSelectBait".ToPlainText());
		}
		else
		{
			confirmed = true;
		}
	}

	private void OnCancelButtonClicked()
	{
		canceled = true;
	}

	internal void NotifySelect(BaitSelectPanelEntry baitSelectPanelEntry)
	{
		SetSelection(baitSelectPanelEntry);
		if (SelectedItem != null)
		{
			details.Setup(SelectedItem);
			detailsFadeGroup.Show();
		}
		else
		{
			detailsFadeGroup.SkipHide();
		}
	}

	public BaitSelectPanelEntry GetSelection()
	{
		return selectedEntry;
	}

	public bool SetSelection(BaitSelectPanelEntry selection)
	{
		selectedEntry = selection;
		this.onSetSelection?.Invoke();
		return true;
	}
}
