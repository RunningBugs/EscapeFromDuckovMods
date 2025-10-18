using System;
using System.Collections.Generic;
using UnityEngine;

namespace Duckov.MiniGames;

public class MiniGame : MonoBehaviour
{
	public enum TickTiming
	{
		Manual,
		Update,
		FixedUpdate,
		LateUpdate
	}

	public enum Button
	{
		None,
		A,
		B,
		Start,
		Select,
		Left,
		Right,
		Up,
		Down
	}

	public class ButtonStatus
	{
		public bool pressed;

		public bool justPressed;

		public bool justReleased;
	}

	public struct MiniGameInputEventContext
	{
		public bool isButtonEvent;

		public Button button;

		public bool pressing;

		public bool buttonDown;

		public bool buttonUp;

		public bool isAxisEvent;

		public int axisIndex;

		public Vector2 axisValue;
	}

	[SerializeField]
	private string id;

	public TickTiming tickTiming;

	[SerializeField]
	private Camera camera;

	[SerializeField]
	private Camera uiCamera;

	[SerializeField]
	private RenderTexture renderTexture;

	public static Action<MiniGame, float> onUpdateLogic;

	private GamingConsole console;

	private Vector2 inputAxis_0;

	private Vector2 inputAxis_1;

	private Dictionary<Button, ButtonStatus> buttons = new Dictionary<Button, ButtonStatus>();

	public string ID => id;

	public Camera Camera => camera;

	public Camera UICamera => uiCamera;

	public RenderTexture RenderTexture => renderTexture;

	public GamingConsole Console => console;

	public static event Action<MiniGame, MiniGameInputEventContext> OnInput;

	public void SetRenderTexture(RenderTexture texture)
	{
		camera.targetTexture = texture;
		if ((bool)uiCamera)
		{
			uiCamera.targetTexture = texture;
		}
	}

	public RenderTexture CreateAndSetRenderTexture(int width, int height)
	{
		RenderTexture result = new RenderTexture(width, height, 32);
		SetRenderTexture(result);
		return result;
	}

	private void Awake()
	{
		if (renderTexture != null)
		{
			SetRenderTexture(renderTexture);
		}
	}

	public void SetInputAxis(Vector2 axis, int index = 0)
	{
		Vector2 vector = inputAxis_0;
		if (index == 0)
		{
			inputAxis_0 = axis;
		}
		if (index == 1)
		{
			inputAxis_1 = axis;
		}
		if (index == 0)
		{
			bool flag = axis.x < -0.1f;
			bool flag2 = axis.x > 0.1f;
			bool flag3 = axis.y > 0.1f;
			bool flag4 = axis.y < -0.1f;
			bool flag5 = vector.x < -0.1f;
			bool flag6 = vector.x > 0.1f;
			bool flag7 = vector.y > 0.1f;
			bool flag8 = vector.y < -0.1f;
			if (flag != flag5)
			{
				SetButton(Button.Left, flag);
			}
			if (flag2 != flag6)
			{
				SetButton(Button.Right, flag2);
			}
			if (flag3 != flag7)
			{
				SetButton(Button.Up, flag3);
			}
			if (flag4 != flag8)
			{
				SetButton(Button.Down, flag4);
			}
		}
		MiniGame.OnInput?.Invoke(this, new MiniGameInputEventContext
		{
			isAxisEvent = true,
			axisIndex = index,
			axisValue = axis
		});
	}

	public void SetButton(Button button, bool down)
	{
		if (!buttons.TryGetValue(button, out var value))
		{
			value = new ButtonStatus();
			buttons[button] = value;
		}
		if (down)
		{
			value.justPressed = true;
			value.pressed = true;
		}
		else
		{
			value.pressed = false;
			value.justReleased = true;
		}
		buttons[button] = value;
		MiniGame.OnInput?.Invoke(this, new MiniGameInputEventContext
		{
			isButtonEvent = true,
			button = button,
			pressing = value.pressed,
			buttonDown = value.justPressed,
			buttonUp = value.justReleased
		});
	}

	public bool GetButton(Button button)
	{
		if (!buttons.TryGetValue(button, out var value))
		{
			return false;
		}
		return value.pressed;
	}

	public bool GetButtonDown(Button button)
	{
		if (!buttons.TryGetValue(button, out var value))
		{
			return false;
		}
		return value.justPressed;
	}

	public bool GetButtonUp(Button button)
	{
		if (!buttons.TryGetValue(button, out var value))
		{
			return false;
		}
		return value.justReleased;
	}

	public Vector2 GetAxis(int index = 0)
	{
		return index switch
		{
			0 => inputAxis_0, 
			1 => inputAxis_1, 
			_ => default(Vector2), 
		};
	}

	private void Tick(float deltaTime)
	{
		UpdateLogic(deltaTime);
		Cleanup();
	}

	private void UpdateLogic(float deltaTime)
	{
		onUpdateLogic?.Invoke(this, deltaTime);
	}

	private void Cleanup()
	{
		foreach (ButtonStatus value in buttons.Values)
		{
			value.justPressed = false;
			value.justReleased = false;
		}
	}

	private void Update()
	{
		if (tickTiming == TickTiming.Update)
		{
			Tick(Time.deltaTime);
		}
	}

	private void FixedUpdate()
	{
		if (tickTiming == TickTiming.FixedUpdate)
		{
			Tick(Time.fixedDeltaTime);
		}
	}

	private void LateUpdate()
	{
		if (tickTiming == TickTiming.FixedUpdate)
		{
			Tick(Time.deltaTime);
		}
	}

	public void ClearInput()
	{
		foreach (ButtonStatus value in buttons.Values)
		{
			if (value.pressed)
			{
				value.justReleased = true;
			}
			value.pressed = false;
		}
		SetInputAxis(default(Vector2));
		SetInputAxis(default(Vector2), 1);
	}

	internal void SetConsole(GamingConsole console)
	{
		this.console = console;
	}
}
