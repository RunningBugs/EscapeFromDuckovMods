using System;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.UI.Animations;
using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.Endowment.UI;

public class EndowmentSelectionPanel : View
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private EndowmentSelectionEntry entryTemplate;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private Button confirmButton;

	[SerializeField]
	private Button cancelButton;

	private PrefabPool<EndowmentSelectionEntry> _pool;

	private bool confirmed;

	private bool canceled;

	private PrefabPool<EndowmentSelectionEntry> Pool
	{
		get
		{
			if (_pool == null)
			{
				_pool = new PrefabPool<EndowmentSelectionEntry>(entryTemplate, null, null, null, null, collectionCheck: true, 10, 10000, delegate(EndowmentSelectionEntry e)
				{
					e.onClicked = (Action<EndowmentSelectionEntry, PointerEventData>)Delegate.Combine(e.onClicked, new Action<EndowmentSelectionEntry, PointerEventData>(OnEntryClicked));
				});
			}
			return _pool;
		}
	}

	public EndowmentSelectionEntry Selection { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		confirmButton.onClick.AddListener(OnConfirmButtonClicked);
		cancelButton.onClick.AddListener(OnCancelButtonClicked);
	}

	protected override void OnCancel()
	{
		base.OnCancel();
		canceled = true;
	}

	private void OnCancelButtonClicked()
	{
		canceled = true;
	}

	private void OnConfirmButtonClicked()
	{
		confirmed = true;
	}

	private void OnEntryClicked(EndowmentSelectionEntry entry, PointerEventData data)
	{
		if (!entry.Locked)
		{
			Select(entry);
		}
	}

	private void Select(EndowmentSelectionEntry entry)
	{
		Selection = entry;
		foreach (EndowmentSelectionEntry activeEntry in Pool.ActiveEntries)
		{
			activeEntry.SetSelection(activeEntry == entry);
		}
		RefreshDescription();
	}

	public void Setup()
	{
		if (EndowmentManager.Instance == null)
		{
			return;
		}
		Pool.ReleaseAll();
		foreach (EndowmentEntry entry in EndowmentManager.Instance.Entries)
		{
			if (!(entry == null))
			{
				Pool.Get().Setup(entry);
			}
		}
		foreach (EndowmentSelectionEntry activeEntry in Pool.ActiveEntries)
		{
			if (activeEntry.Target.Index == EndowmentManager.SelectedIndex)
			{
				Select(activeEntry);
				break;
			}
		}
	}

	private void RefreshDescription()
	{
		if (Selection == null)
		{
			descriptionText.text = "-";
		}
		descriptionText.text = Selection.DescriptionAndEffects;
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		Execute().Forget();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	public async UniTask Execute()
	{
		Setup();
		await fadeGroup.ShowAndReturnTask();
		await WaitForConfirm();
		if (confirmed && Selection.Index != EndowmentManager.CurrentIndex)
		{
			EndowmentManager.Instance.SelectIndex(Selection.Index);
			SceneLoader.Instance.LoadBaseScene(null, doCircleFade: false).Forget();
		}
		Close();
	}

	private async UniTask WaitForConfirm()
	{
		confirmed = false;
		canceled = false;
		while ((!confirmed || !(Selection != null)) && !canceled)
		{
			await UniTask.Yield();
		}
	}

	internal void SkipHide()
	{
		if (fadeGroup != null)
		{
			fadeGroup.SkipHide();
		}
	}

	public static void Show()
	{
		EndowmentSelectionPanel viewInstance = View.GetViewInstance<EndowmentSelectionPanel>();
		if (!(viewInstance == null))
		{
			viewInstance.Open();
		}
	}
}
