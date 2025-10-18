using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class Soda_Joysticks : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IDragHandler
{
	public bool usable = true;

	private int verticalRes;

	[Range(0f, 0.5f)]
	public float joystickRangePercent = 0.3f;

	[Range(0f, 0.5f)]
	public float cancleRangePercent = 0.4f;

	public bool fixedPositon = true;

	public bool followFinger;

	public bool canCancle;

	private float joystickRangePixel;

	private float cancleRangePixel;

	[SerializeField]
	private Transform backGround;

	[SerializeField]
	private Image joyImage;

	[SerializeField]
	private CanvasGroup cancleRangeCanvasGroup;

	private bool holding;

	private Vector2 downPoint;

	private int currentPointerID;

	private Vector2 inputValue;

	[SerializeField]
	private float rotValue = 10f;

	[Range(0f, 1f)]
	public float deadZone;

	[Range(0f, 1f)]
	public float fullZone = 1f;

	public bool hideWhenNotTouch;

	public CanvasGroup canvasGroup;

	private bool triggeringCancle;

	public UnityEvent<Vector2, bool> UpdateValueEvent;

	public UnityEvent OnTouchEvent;

	public UnityEvent<bool> OnUpEvent;

	public bool Holding => holding;

	public Vector2 InputValue => inputValue;

	private void Start()
	{
		joyImage.gameObject.SetActive(value: false);
		if (hideWhenNotTouch)
		{
			canvasGroup.alpha = 0f;
		}
		if ((bool)cancleRangeCanvasGroup)
		{
			cancleRangeCanvasGroup.alpha = 0f;
		}
	}

	private void Update()
	{
		if (holding && !usable)
		{
			Revert();
		}
	}

	private void OnEnable()
	{
		if ((bool)cancleRangeCanvasGroup)
		{
			cancleRangeCanvasGroup.alpha = 0f;
		}
		triggeringCancle = false;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (usable && !holding)
		{
			holding = true;
			currentPointerID = eventData.pointerId;
			downPoint = eventData.position;
			verticalRes = Screen.height;
			joystickRangePixel = (float)verticalRes * joystickRangePercent;
			cancleRangePixel = (float)verticalRes * cancleRangePercent;
			if (!fixedPositon)
			{
				backGround.transform.position = downPoint;
			}
			joyImage.transform.position = backGround.transform.position;
			backGround.transform.rotation = Quaternion.Euler(Vector3.zero);
			joyImage.gameObject.SetActive(value: true);
			UpdateValueEvent?.Invoke(Vector2.zero, arg1: true);
			if (hideWhenNotTouch)
			{
				canvasGroup.alpha = 1f;
			}
			if (canCancle && (bool)cancleRangeCanvasGroup)
			{
				cancleRangeCanvasGroup.alpha = 0.12f;
			}
			triggeringCancle = false;
			OnTouchEvent?.Invoke();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (usable)
		{
			OnUpEvent?.Invoke(!triggeringCancle);
			UpdateValueEvent?.Invoke(Vector2.zero, arg1: false);
			if (holding && currentPointerID == eventData.pointerId)
			{
				Revert();
			}
		}
	}

	private void Revert()
	{
		UpdateValueEvent?.Invoke(Vector2.zero, arg1: false);
		if (holding)
		{
			OnUpEvent?.Invoke(arg0: false);
		}
		if (usable)
		{
			joyImage.transform.position = backGround.transform.position;
			inputValue = Vector2.zero;
			holding = false;
			backGround.transform.rotation = Quaternion.Euler(Vector3.zero);
			if (joyImage.gameObject.activeSelf)
			{
				joyImage.gameObject.SetActive(value: false);
			}
			if (hideWhenNotTouch)
			{
				canvasGroup.alpha = 0f;
			}
			if ((bool)cancleRangeCanvasGroup)
			{
				cancleRangeCanvasGroup.alpha = 0f;
			}
		}
	}

	public void CancleTouch()
	{
		Revert();
	}

	public void OnDisable()
	{
		Revert();
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!holding || eventData.pointerId != currentPointerID)
		{
			return;
		}
		Vector2 position = eventData.position;
		if (position == downPoint)
		{
			inputValue = Vector2.zero;
			return;
		}
		float num = Vector2.Distance(position, downPoint);
		float num2 = num;
		Vector2 normalized = (position - downPoint).normalized;
		if (num > joystickRangePixel)
		{
			if (followFinger)
			{
				downPoint += (num - joystickRangePixel) * normalized;
			}
			if (!fixedPositon && followFinger)
			{
				backGround.transform.position = downPoint;
			}
			num2 = joystickRangePixel;
		}
		position = downPoint + normalized * num2;
		Vector2 vector = Vector2.zero;
		if (joystickRangePixel > 0f)
		{
			vector = normalized * num2 / joystickRangePixel;
		}
		joyImage.transform.position = backGround.transform.position + (Vector3)normalized * num2;
		Vector3 zero = Vector3.zero;
		zero.y = 0f - vector.x;
		zero.x = vector.y;
		zero *= rotValue;
		backGround.transform.rotation = Quaternion.Euler(zero);
		float magnitude = vector.magnitude;
		magnitude = Mathf.InverseLerp(deadZone, fullZone, magnitude);
		inputValue = magnitude * normalized;
		UpdateValueEvent?.Invoke(inputValue, arg1: true);
		if (canCancle && (bool)cancleRangeCanvasGroup)
		{
			if (num >= cancleRangePixel)
			{
				cancleRangeCanvasGroup.alpha = 1f;
				triggeringCancle = true;
			}
			else
			{
				cancleRangeCanvasGroup.alpha = 0.12f;
				triggeringCancle = false;
			}
		}
	}
}
