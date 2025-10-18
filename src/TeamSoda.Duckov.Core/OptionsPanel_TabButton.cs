using System;
using Duckov.Options.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class OptionsPanel_TabButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private GameObject selectedIndicator;

	[SerializeField]
	private GameObject tab;

	public Action<OptionsPanel_TabButton, PointerEventData> onClicked;

	public void OnPointerClick(PointerEventData eventData)
	{
		onClicked?.Invoke(this, eventData);
	}

	internal void NotifySelectionChanged(OptionsPanel optionsPanel, OptionsPanel_TabButton selection)
	{
		bool active = selection == this;
		tab.SetActive(active);
		selectedIndicator.SetActive(active);
	}
}
