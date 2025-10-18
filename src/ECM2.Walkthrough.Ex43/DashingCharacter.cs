using UnityEngine;

namespace ECM2.Walkthrough.Ex43;

public class DashingCharacter : Character
{
	public enum ECustomMovementMode
	{
		None,
		Dashing
	}

	[Space(15f)]
	[Tooltip("Is the character able to Dash?")]
	public bool canEverDash = true;

	[Tooltip("Dash initial impulse.")]
	public float dashImpulse = 20f;

	[Tooltip("Dash duration in seconds.")]
	public float dashDuration = 0.15f;

	protected float _dashingTime;

	protected bool _dashInputPressed;

	protected bool dashInputPressed => _dashInputPressed;

	public bool IsDashing()
	{
		if (base.movementMode == MovementMode.Custom)
		{
			return base.customMovementMode == 1;
		}
		return false;
	}

	public void Dash()
	{
		_dashInputPressed = true;
	}

	public void StopDashing()
	{
		_dashInputPressed = false;
	}

	public bool IsDashAllowed()
	{
		if (IsCrouched())
		{
			return false;
		}
		if (canEverDash)
		{
			if (!IsWalking())
			{
				return IsFalling();
			}
			return true;
		}
		return false;
	}

	protected virtual void DoDash()
	{
		Vector3 vector = GetMovementDirection();
		if (vector.isZero())
		{
			vector = GetForwardVector();
		}
		Vector3 normalized = vector.onlyXZ().normalized;
		SetVelocity(normalized * dashImpulse);
		SetMovementMode(MovementMode.Custom, 1);
		if (base.rotationMode == RotationMode.OrientRotationToMovement)
		{
			SetRotation(Quaternion.LookRotation(normalized));
		}
	}

	protected virtual void ResetDashState()
	{
		_dashingTime = 0f;
		_dashInputPressed = false;
		SetVelocity(Vector3.zero);
		SetMovementMode(MovementMode.Falling);
	}

	protected virtual void DashingMovementMode(float deltaTime)
	{
		SetMovementDirection(Vector3.zero);
		_dashingTime += deltaTime;
		if (_dashingTime >= dashDuration)
		{
			ResetDashState();
		}
	}

	protected override void OnBeforeSimulationUpdate(float deltaTime)
	{
		base.OnBeforeSimulationUpdate(deltaTime);
		if (!IsDashing() && dashInputPressed && IsDashAllowed())
		{
			DoDash();
		}
	}

	protected override void CustomMovementMode(float deltaTime)
	{
		base.CustomMovementMode(deltaTime);
		if (base.customMovementMode == 1)
		{
			DashingMovementMode(deltaTime);
		}
	}
}
