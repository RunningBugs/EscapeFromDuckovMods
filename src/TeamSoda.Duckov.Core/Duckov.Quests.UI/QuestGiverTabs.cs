using System;
using System.Collections.Generic;
using UnityEngine;

namespace Duckov.Quests.UI;

public class QuestGiverTabs : MonoBehaviour, ISingleSelectionMenu<QuestGiverTabButton>
{
	[SerializeField]
	private List<QuestGiverTabButton> buttons = new List<QuestGiverTabButton>();

	private QuestGiverTabButton selectedButton;

	private bool initialized;

	public event Action<QuestGiverTabs> onSelectionChanged;

	public QuestGiverTabButton GetSelection()
	{
		return selectedButton;
	}

	public QuestStatus GetStatus()
	{
		if (!initialized)
		{
			Initialize();
		}
		return selectedButton.Status;
	}

	public bool SetSelection(QuestGiverTabButton selection)
	{
		selectedButton = selection;
		RefreshAllButtons();
		this.onSelectionChanged?.Invoke(this);
		return true;
	}

	private void Initialize()
	{
		foreach (QuestGiverTabButton button in buttons)
		{
			button.Setup(this);
		}
		if (buttons.Count > 0)
		{
			SetSelection(buttons[0]);
		}
		initialized = true;
	}

	private void Awake()
	{
		if (!initialized)
		{
			Initialize();
		}
	}

	private void RefreshAllButtons()
	{
		foreach (QuestGiverTabButton button in buttons)
		{
			button.Refresh();
		}
	}
}
