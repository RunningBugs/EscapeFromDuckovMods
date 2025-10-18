using System.Collections.Generic;
using Duckov.UI.Animations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Duckov.UI;

public class GenericButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler
{
	public List<ToggleAnimation> toggleAnimations = new List<ToggleAnimation>();

	public UnityEvent onPointerClick;

	public UnityEvent onPointerDown;

	public UnityEvent onPointerUp;

	public void OnPointerClick(PointerEventData eventData)
	{
		onPointerClick?.Invoke();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		foreach (ToggleAnimation toggleAnimation in toggleAnimations)
		{
			toggleAnimation.SetToggle(value: true);
		}
		onPointerDown?.Invoke();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		foreach (ToggleAnimation toggleAnimation in toggleAnimations)
		{
			toggleAnimation.SetToggle(value: false);
		}
		onPointerUp?.Invoke();
	}
}
