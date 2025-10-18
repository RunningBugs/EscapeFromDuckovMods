using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollViewEventReceiver : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	[SerializeField]
	private ScrollRect scrollRect;

	private void Awake()
	{
		if (scrollRect == null)
		{
			scrollRect = GetComponent<ScrollRect>();
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
	}
}
