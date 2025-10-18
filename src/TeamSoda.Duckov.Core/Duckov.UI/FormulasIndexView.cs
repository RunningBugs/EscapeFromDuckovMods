using Duckov.UI.Animations;
using Duckov.Utilities;
using UnityEngine;

namespace Duckov.UI;

public class FormulasIndexView : View, ISingleSelectionMenu<FormulasIndexEntry>
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private FormulasIndexEntry entryTemplate;

	[SerializeField]
	private FormulasDetailsDisplay detailsDisplay;

	private PrefabPool<FormulasIndexEntry> _pool;

	private FormulasIndexEntry selectedEntry;

	private PrefabPool<FormulasIndexEntry> Pool
	{
		get
		{
			if (_pool == null)
			{
				_pool = new PrefabPool<FormulasIndexEntry>(entryTemplate);
			}
			return _pool;
		}
	}

	public static FormulasIndexView Instance => View.GetViewInstance<FormulasIndexView>();

	public FormulasIndexEntry GetSelection()
	{
		return selectedEntry;
	}

	protected override void Awake()
	{
		base.Awake();
	}

	public static void Show()
	{
		if (!(Instance == null))
		{
			Instance.Open();
		}
	}

	public bool SetSelection(FormulasIndexEntry selection)
	{
		selectedEntry = selection;
		return true;
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		selectedEntry = null;
		Pool.ReleaseAll();
		foreach (CraftingFormula entry in CraftingFormulaCollection.Instance.Entries)
		{
			if (!entry.hideInIndex && (!GameMetaData.Instance.IsDemo || !entry.lockInDemo))
			{
				Pool.Get().Setup(this, entry);
			}
		}
		RefreshDetails();
		fadeGroup.Show();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	internal void OnEntryClicked(FormulasIndexEntry entry)
	{
		FormulasIndexEntry formulasIndexEntry = selectedEntry;
		selectedEntry = entry;
		selectedEntry.Refresh();
		if ((bool)formulasIndexEntry)
		{
			formulasIndexEntry.Refresh();
		}
		RefreshDetails();
	}

	private void RefreshDetails()
	{
		if ((bool)selectedEntry && selectedEntry.Valid)
		{
			detailsDisplay.Setup(selectedEntry.Formula);
		}
		else
		{
			detailsDisplay.Setup(null);
		}
	}
}
