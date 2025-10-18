using ECM2;
using UnityEngine;

namespace Duckov.MiniGames.Examples.FPS;

public class FPSMovement : Character
{
	[SerializeField]
	private MiniGame game;

	[SerializeField]
	private Vector2 lookSensitivity;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		if (game == null)
		{
			game.GetComponentInParent<MiniGame>();
		}
	}

	public void SetGame(MiniGame game)
	{
		this.game = game;
	}

	private void Update()
	{
		UpdateRotation();
		UpdateMovement();
		if (game.GetButtonDown(MiniGame.Button.B))
		{
			Jump();
		}
		else if (game.GetButtonUp(MiniGame.Button.B))
		{
			StopJumping();
		}
	}

	private void UpdateMovement()
	{
		Vector2 axis = game.GetAxis();
		Vector3 vector = Vector3.zero;
		vector += Vector3.right * axis.x;
		vector += Vector3.forward * axis.y;
		if ((bool)base.camera)
		{
			vector = vector.relativeTo(base.cameraTransform);
		}
		SetMovementDirection(vector);
	}

	private void UpdateRotation()
	{
		Vector2 axis = game.GetAxis(1);
		AddYawInput(axis.x * lookSensitivity.x);
		if (axis.y != 0f)
		{
			float num = MathLib.ClampAngle(0f - base.cameraTransform.localRotation.eulerAngles.x + axis.y * lookSensitivity.y, -80f, 80f);
			base.cameraTransform.localRotation = Quaternion.Euler(0f - num, 0f, 0f);
		}
	}

	public void AddControlYawInput(float value)
	{
		AddYawInput(value);
	}
}
