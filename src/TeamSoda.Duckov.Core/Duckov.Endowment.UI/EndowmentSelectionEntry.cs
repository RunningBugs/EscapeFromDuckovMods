using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.Endowment.UI;

public class EndowmentSelectionEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private GameObject selectedIndicator;

	[SerializeField]
	private GameObject lockedIndcator;

	[SerializeField]
	private TextMeshProUGUI requirementText;

	public Action<EndowmentSelectionEntry, PointerEventData> onClicked;

	public string DisplayName
	{
		get
		{
			if (Target == null)
			{
				return "-";
			}
			return Target.DisplayName;
		}
	}

	public string Description
	{
		get
		{
			if (Target == null)
			{
				return "-";
			}
			return Target.Description;
		}
	}

	public string DescriptionAndEffects
	{
		get
		{
			if (Target == null)
			{
				return "-";
			}
			return Target.DescriptionAndEffects;
		}
	}

	public EndowmentIndex Index
	{
		get
		{
			if (Target == null)
			{
				return EndowmentIndex.None;
			}
			return Target.Index;
		}
	}

	public EndowmentEntry Target { get; private set; }

	public bool Selected { get; private set; }

	public bool Unlocked => EndowmentManager.GetEndowmentUnlocked(Index);

	public bool Locked => !Unlocked;

	public void Setup(EndowmentEntry target)
	{
		Target = target;
		if (!(Target == null))
		{
			displayNameText.text = Target.DisplayName;
			icon.sprite = Target.Icon;
			requirementText.text = "- " + Target.RequirementText + " -";
			Refresh();
		}
	}

	private void Refresh()
	{
		if (!(Target == null))
		{
			selectedIndicator.SetActive(Selected);
			lockedIndcator.SetActive(Locked);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!Locked)
		{
			onClicked?.Invoke(this, eventData);
		}
	}

	internal void SetSelection(bool value)
	{
		Selected = value;
		Refresh();
	}
}
