using System;
using Duckov.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

public class TaskSkipperUI : MonoBehaviour
{
	[SerializeField]
	private TaskList target;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Image fill;

	[SerializeField]
	private float totalTime = 2f;

	[SerializeField]
	private float hideAfterSeconds = 2f;

	private float pressTime;

	private float alpha;

	private float hideTimer;

	private bool show;

	private IDisposable anyButtonListener;

	private bool pressing;

	private bool skipped;

	private void Awake()
	{
		UIInputManager.OnInteractInputContext += OnInteractInputContext;
		anyButtonListener = InputSystem.onAnyButtonPress.Call(OnAnyButton);
		skipped = false;
		alpha = 0f;
	}

	private void OnAnyButton(InputControl control)
	{
		Show();
	}

	private void OnDestroy()
	{
		UIInputManager.OnInteractInputContext -= OnInteractInputContext;
		anyButtonListener.Dispose();
	}

	private void OnInteractInputContext(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			pressing = true;
		}
		if (context.canceled)
		{
			pressing = false;
		}
	}

	private void Update()
	{
		UpdatePressing();
		UpdateFill();
		UpdateCanvasGroup();
	}

	private void Show()
	{
		show = true;
		hideTimer = hideAfterSeconds;
	}

	private void UpdatePressing()
	{
		if (UIInputManager.Instance == null)
		{
			pressing = Keyboard.current.fKey.isPressed;
		}
		if (pressing && !skipped)
		{
			pressTime += Time.deltaTime;
			if (pressTime >= totalTime)
			{
				skipped = true;
				target.Skip();
			}
			Show();
		}
		else if (!skipped)
		{
			pressTime = Mathf.MoveTowards(pressTime, 0f, Time.deltaTime);
		}
	}

	private void UpdateFill()
	{
		float fillAmount = pressTime / totalTime;
		fill.fillAmount = fillAmount;
	}

	private void UpdateCanvasGroup()
	{
		if (show)
		{
			alpha = Mathf.MoveTowards(alpha, 1f, 10f * Time.deltaTime);
			hideTimer = Mathf.MoveTowards(hideTimer, 0f, Time.deltaTime);
			if (hideTimer < 0.01f)
			{
				show = false;
			}
		}
		else
		{
			alpha = Mathf.MoveTowards(alpha, 0f, 10f * Time.deltaTime);
		}
		canvasGroup.alpha = alpha;
	}
}
