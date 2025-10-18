using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class PointerDownUpEvents : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public UnityEvent<PointerEventData> onPointerDown;

	public UnityEvent<PointerEventData> onPointerUp;

	public void OnPointerDown(PointerEventData eventData)
	{
		onPointerDown?.Invoke(eventData);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		onPointerUp?.Invoke(eventData);
	}
}
