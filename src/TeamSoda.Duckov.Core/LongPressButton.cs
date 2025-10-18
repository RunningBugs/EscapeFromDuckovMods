using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class LongPressButton : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerExitHandler
{
	[SerializeField]
	private Image fill;

	[SerializeField]
	private float pressTime = 1f;

	public UnityEvent onPressStarted;

	public UnityEvent onPressCanceled;

	public UnityEvent onPressFullfilled;

	private float timeWhenPressStarted;

	private bool pressed;

	private float TimeSincePressStarted => Time.unscaledTime - timeWhenPressStarted;

	private float Progress
	{
		get
		{
			if (!pressed)
			{
				return 0f;
			}
			return TimeSincePressStarted / pressTime;
		}
	}

	private void Update()
	{
		fill.fillAmount = Progress;
		if (pressed && Progress >= 1f)
		{
			onPressFullfilled?.Invoke();
			pressed = false;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		pressed = true;
		timeWhenPressStarted = Time.unscaledTime;
		onPressStarted?.Invoke();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (pressed)
		{
			pressed = false;
			onPressCanceled?.Invoke();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (pressed)
		{
			pressed = false;
			onPressCanceled?.Invoke();
		}
	}
}
