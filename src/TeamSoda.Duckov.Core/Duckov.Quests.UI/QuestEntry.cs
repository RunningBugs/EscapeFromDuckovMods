using System;
using Duckov.Utilities;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.Quests.UI;

public class QuestEntry : MonoBehaviour, IPoolable, IPointerClickHandler, IEventSystemHandler
{
	private ISingleSelectionMenu<QuestEntry> menu;

	private Quest target;

	[SerializeField]
	private GameObject selectionIndicator;

	[SerializeField]
	private TextMeshProUGUI displayName;

	[SerializeField]
	private TextMeshProUGUI locationName;

	[SerializeField]
	[LocalizationKey("Default")]
	private string anyLocationKey;

	[SerializeField]
	private GameObject redDot;

	[SerializeField]
	private GameObject claimableIndicator;

	[SerializeField]
	private TextMeshProUGUI questIDDisplay;

	public Quest Target => target;

	public bool Selected => menu.GetSelection() == this;

	public event Action<QuestEntry, PointerEventData> onClick;

	public void NotifyPooled()
	{
	}

	public void NotifyReleased()
	{
		UnregisterEvents();
		target = null;
	}

	private void OnDestroy()
	{
		UnregisterEvents();
	}

	internal void Setup(Quest quest)
	{
		UnregisterEvents();
		target = quest;
		RegisterEvents();
		Refresh();
	}

	internal void SetMenu(ISingleSelectionMenu<QuestEntry> menu)
	{
		this.menu = menu;
	}

	private void RegisterEvents()
	{
		if (target != null)
		{
			target.onStatusChanged += OnTargetStatusChanged;
			target.onNeedInspectionChanged += OnNeedInspectionChanged;
		}
	}

	private void UnregisterEvents()
	{
		if (target != null)
		{
			target.onStatusChanged -= OnTargetStatusChanged;
			target.onNeedInspectionChanged -= OnNeedInspectionChanged;
		}
	}

	private void OnNeedInspectionChanged(Quest obj)
	{
		Refresh();
	}

	private void OnTargetStatusChanged(Quest quest)
	{
		Refresh();
	}

	private void OnMasterSelectionChanged(QuestView view, Quest oldSelection, Quest newSelection)
	{
		Refresh();
	}

	private void Refresh()
	{
		selectionIndicator.SetActive(Selected);
		displayName.text = target.DisplayName;
		questIDDisplay.text = $"{target.ID:0000}";
		SceneInfoEntry requireSceneInfo = target.RequireSceneInfo;
		if (requireSceneInfo == null)
		{
			locationName.text = anyLocationKey.ToPlainText();
		}
		else
		{
			locationName.text = requireSceneInfo.DisplayName;
		}
		redDot.SetActive(target.NeedInspection);
		claimableIndicator.SetActive(target.Complete || target.AreTasksFinished());
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		this.onClick?.Invoke(this, eventData);
		menu.SetSelection(this);
	}

	public void NotifyRefresh()
	{
		Refresh();
	}
}
