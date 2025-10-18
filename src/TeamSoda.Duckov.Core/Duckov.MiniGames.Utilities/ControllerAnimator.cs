using DG.Tweening;
using UnityEngine;

namespace Duckov.MiniGames.Utilities;

public class ControllerAnimator : MonoBehaviour
{
	private GamingConsole master;

	public Transform mainTransform;

	public Transform btn_A;

	public Transform btn_B;

	public Transform btn_Start;

	public Transform btn_Select;

	public Transform btn_Axis;

	public Transform fxPos_Up;

	public Transform fxPos_Right;

	public Transform fxPos_Down;

	public Transform fxPos_Left;

	[SerializeField]
	private float transitionDuration = 0.2f;

	[SerializeField]
	private float axisAmp = 10f;

	[SerializeField]
	private float btnDepth = 0.003f;

	[SerializeField]
	private float torqueStrength = 5f;

	[SerializeField]
	private float torqueDuration = 0.5f;

	[SerializeField]
	private int torqueVibrato = 1;

	[SerializeField]
	private float torqueElasticity = 1f;

	[SerializeField]
	private ParticleSystem buttonPressFX;

	[SerializeField]
	private ParticleSystem buttonRestFX;

	private void OnEnable()
	{
		MiniGame.OnInput += OnMiniGameInput;
	}

	private void OnDisable()
	{
		MiniGame.OnInput -= OnMiniGameInput;
	}

	private void OnMiniGameInput(MiniGame game, MiniGame.MiniGameInputEventContext context)
	{
		if (!(master == null) && !(master.Game != game))
		{
			HandleInput(context);
		}
	}

	private void HandleInput(MiniGame.MiniGameInputEventContext context)
	{
		if (context.isButtonEvent)
		{
			HandleButtonEvent(context);
		}
		else if (context.isAxisEvent)
		{
			HandleAxisEvent(context);
		}
	}

	private void HandleAxisEvent(MiniGame.MiniGameInputEventContext context)
	{
		if (context.axisIndex == 0)
		{
			SetAxis(context.axisValue);
		}
	}

	private void HandleButtonEvent(MiniGame.MiniGameInputEventContext context)
	{
		switch (context.button)
		{
		case MiniGame.Button.A:
			HandleBtnPushRest(btn_A, context.pressing);
			break;
		case MiniGame.Button.B:
			HandleBtnPushRest(btn_B, context.pressing);
			break;
		case MiniGame.Button.Start:
			HandleBtnPushRest(btn_Start, context.pressing);
			break;
		case MiniGame.Button.Select:
			HandleBtnPushRest(btn_Select, context.pressing);
			break;
		case MiniGame.Button.Left:
		case MiniGame.Button.Right:
		case MiniGame.Button.Up:
		case MiniGame.Button.Down:
			PlayAxisPressReleaseFX(context.button, context.pressing);
			break;
		}
		if (context.pressing)
		{
			switch (context.button)
			{
			case MiniGame.Button.A:
				ApplyTorque(1f, -0.5f);
				break;
			case MiniGame.Button.B:
				ApplyTorque(1f, -0f);
				break;
			case MiniGame.Button.Start:
				ApplyTorque(0.5f, -0.5f);
				break;
			case MiniGame.Button.Select:
				ApplyTorque(-0.5f, -0.5f);
				break;
			case MiniGame.Button.Up:
				ApplyTorque(-1f, 0.5f);
				break;
			case MiniGame.Button.Right:
				ApplyTorque(-0.5f, 0f);
				break;
			case MiniGame.Button.Down:
				ApplyTorque(-1f, -0.5f);
				break;
			case MiniGame.Button.Left:
				ApplyTorque(-1f, 0f);
				break;
			case MiniGame.Button.None:
				break;
			}
		}
		else
		{
			ApplyTorque(Random.insideUnitCircle * 0.25f);
		}
	}

	private void PlayAxisPressReleaseFX(MiniGame.Button button, bool pressing)
	{
		Transform transform = null;
		switch (button)
		{
		case MiniGame.Button.Up:
			transform = fxPos_Up;
			break;
		case MiniGame.Button.Right:
			transform = fxPos_Right;
			break;
		case MiniGame.Button.Down:
			transform = fxPos_Down;
			break;
		case MiniGame.Button.Left:
			transform = fxPos_Left;
			break;
		}
		if (!(transform == null))
		{
			if (pressing)
			{
				FXPool.Play(buttonPressFX, transform.position, transform.rotation);
			}
			else
			{
				FXPool.Play(buttonRestFX, transform.position, transform.rotation);
			}
		}
	}

	private void ApplyTorque(float x, float y)
	{
		if (!(mainTransform == null))
		{
			mainTransform.DOKill();
			Vector3 punch = new Vector3(0f - y, 0f - x, 0f) * torqueStrength;
			mainTransform.localRotation = Quaternion.identity;
			mainTransform.DOPunchRotation(punch, torqueDuration, torqueVibrato, torqueElasticity);
		}
	}

	private void ApplyTorque(Vector2 torque)
	{
		ApplyTorque(torque.x, torque.y);
	}

	private void HandleBtnPushRest(Transform btnTrans, bool pressed)
	{
		if (pressed)
		{
			Push(btnTrans);
		}
		else
		{
			Rest(btnTrans);
		}
	}

	internal void SetConsole(GamingConsole master)
	{
		this.master = master;
		RefreshAll();
	}

	private void RefreshAll()
	{
		RestAll();
		if (master == null)
		{
			return;
		}
		MiniGame game = master.Game;
		if (!(game == null))
		{
			if (game.GetButton(MiniGame.Button.A))
			{
				Push(btn_A);
			}
			if (game.GetButton(MiniGame.Button.B))
			{
				Push(btn_B);
			}
			if (game.GetButton(MiniGame.Button.Select))
			{
				Push(btn_Select);
			}
			if (game.GetButton(MiniGame.Button.Start))
			{
				Push(btn_Start);
			}
			SetAxis(game.GetAxis());
		}
	}

	private void RestAll()
	{
		Rest(btn_A);
		Rest(btn_B);
		Rest(btn_Start);
		Rest(btn_Select);
		Rest(btn_Axis);
		SetAxis(Vector2.zero);
	}

	private void SetAxis(Vector2 axis)
	{
		if (!(btn_Axis == null))
		{
			axis = axis.normalized;
			Vector3 euler = new Vector3(0f, (0f - axis.x) * axisAmp, axis.y * axisAmp);
			Quaternion localRotation = btn_Axis.localRotation;
			Quaternion quaternion = Quaternion.Euler(euler);
			_ = quaternion * Quaternion.Inverse(localRotation);
			btn_Axis.localRotation = quaternion;
		}
	}

	private void Push(Transform btnTransform)
	{
		if (!(btnTransform == null))
		{
			btnTransform.DOKill();
			btnTransform.DOLocalMoveX(0f - btnDepth, transitionDuration).SetEase(Ease.OutElastic);
			if ((bool)buttonPressFX)
			{
				FXPool.Play(buttonPressFX, btnTransform.position, btnTransform.rotation);
			}
		}
	}

	private void Rest(Transform btnTransform)
	{
		if (!(btnTransform == null))
		{
			btnTransform.DOKill();
			btnTransform.DOLocalMoveX(0f, transitionDuration).SetEase(Ease.OutElastic);
			if ((bool)buttonRestFX)
			{
				FXPool.Play(buttonRestFX, btnTransform.position, btnTransform.rotation);
			}
		}
	}
}
