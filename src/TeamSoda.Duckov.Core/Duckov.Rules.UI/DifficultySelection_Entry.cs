using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.Rules.UI;

public class DifficultySelection_Entry : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private GameObject recommendationIndicator;

	[SerializeField]
	private GameObject selectedIndicator;

	[SerializeField]
	private GameObject lockedIndicator;

	internal Action<DifficultySelection_Entry> onPointerEnter;

	internal Action<DifficultySelection_Entry> onPointerExit;

	private bool locked;

	public DifficultySelection Master { get; private set; }

	public DifficultySelection.SettingEntry Setting { get; private set; }

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!locked)
		{
			Master.NotifySelected(this);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Master?.NotifyEntryPointerEnter(this);
		onPointerEnter?.Invoke(this);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Master?.NotifyEntryPointerExit(this);
		onPointerExit?.Invoke(this);
	}

	internal void Refresh()
	{
		if (!(Master == null))
		{
			selectedIndicator.SetActive(Master.SelectedRuleIndex == Setting.ruleIndex);
		}
	}

	internal void Setup(DifficultySelection master, DifficultySelection.SettingEntry setting, bool locked)
	{
		Master = master;
		Setting = setting;
		title.text = setting.Title;
		icon.sprite = setting.icon;
		recommendationIndicator.SetActive(setting.recommended);
		this.locked = locked;
		lockedIndicator.SetActive(locked);
		Refresh();
	}
}
