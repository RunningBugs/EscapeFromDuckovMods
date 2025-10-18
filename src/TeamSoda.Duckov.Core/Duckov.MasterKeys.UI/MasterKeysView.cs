using Duckov.UI;
using Duckov.UI.Animations;
using UnityEngine;

namespace Duckov.MasterKeys.UI;

public class MasterKeysView : View, ISingleSelectionMenu<MasterKeysIndexEntry>
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private MasterKeysIndexList listDisplay;

	[SerializeField]
	private MasterKeysIndexInspector inspector;

	public static MasterKeysView Instance => View.GetViewInstance<MasterKeysView>();

	protected override void Awake()
	{
		base.Awake();
		listDisplay.onEntryPointerClicked += OnEntryClicked;
	}

	private void OnEntryClicked(MasterKeysIndexEntry entry)
	{
		RefreshInspectorDisplay();
	}

	public MasterKeysIndexEntry GetSelection()
	{
		return listDisplay.GetSelection();
	}

	public bool SetSelection(MasterKeysIndexEntry selection)
	{
		listDisplay.GetSelection();
		return true;
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		fadeGroup.Show();
		SetSelection(null);
		RefreshListDisplay();
		RefreshInspectorDisplay();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	private void RefreshListDisplay()
	{
		listDisplay.Refresh();
	}

	private void RefreshInspectorDisplay()
	{
		MasterKeysIndexEntry selection = GetSelection();
		inspector.Setup(selection);
	}

	internal static void Show()
	{
		if (Instance == null)
		{
			Debug.Log(" Master keys view Instance is null");
		}
		else
		{
			Instance.Open();
		}
	}
}
