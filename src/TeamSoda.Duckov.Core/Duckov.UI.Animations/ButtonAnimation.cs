using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.UI.Animations;

public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private GameObject hoveringIndicator;

	[SerializeField]
	private List<ToggleAnimation> toggles = new List<ToggleAnimation>();

	[SerializeField]
	private bool mute;

	private void Awake()
	{
		SetAll(value: false);
		if ((bool)hoveringIndicator)
		{
			hoveringIndicator.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		SetAll(value: false);
	}

	private void OnDisable()
	{
		if ((bool)hoveringIndicator)
		{
			hoveringIndicator.SetActive(value: false);
		}
	}

	private void SetAll(bool value)
	{
		foreach (ToggleAnimation toggle in toggles)
		{
			if (!(toggle == null))
			{
				toggle.SetToggle(value);
			}
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		SetAll(value: true);
		if (!mute)
		{
			AudioManager.Post("UI/click");
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		SetAll(value: false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if ((bool)hoveringIndicator)
		{
			hoveringIndicator.SetActive(value: true);
		}
		if (!mute)
		{
			AudioManager.Post("UI/hover");
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if ((bool)hoveringIndicator)
		{
			hoveringIndicator.SetActive(value: false);
		}
	}
}
