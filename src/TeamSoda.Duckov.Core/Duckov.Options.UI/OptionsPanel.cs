using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.Options.UI;

public class OptionsPanel : UIPanel, ISingleSelectionMenu<OptionsPanel_TabButton>
{
	[SerializeField]
	private List<OptionsPanel_TabButton> tabButtons;

	private OptionsPanel_TabButton selection;

	private void Start()
	{
		Setup();
	}

	private void Setup()
	{
		foreach (OptionsPanel_TabButton tabButton in tabButtons)
		{
			tabButton.onClicked = (Action<OptionsPanel_TabButton, PointerEventData>)Delegate.Combine(tabButton.onClicked, new Action<OptionsPanel_TabButton, PointerEventData>(OnTabButtonClicked));
		}
		if (selection == null)
		{
			selection = tabButtons[0];
		}
		SetSelection(selection);
	}

	private void OnTabButtonClicked(OptionsPanel_TabButton button, PointerEventData data)
	{
		data.Use();
		SetSelection(button);
	}

	protected override void OnOpen()
	{
		base.OnOpen();
	}

	public OptionsPanel_TabButton GetSelection()
	{
		return selection;
	}

	public bool SetSelection(OptionsPanel_TabButton selection)
	{
		this.selection = selection;
		foreach (OptionsPanel_TabButton tabButton in tabButtons)
		{
			tabButton.NotifySelectionChanged(this, selection);
		}
		return true;
	}
}
