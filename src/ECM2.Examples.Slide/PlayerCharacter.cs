using UnityEngine;

namespace ECM2.Examples.Slide;

public class PlayerCharacter : Character
{
	private enum ECustomMovementMode
	{
		Sliding = 1
	}

	[Space(15f)]
	public float slideImpulse = 20f;

	public float slideDownAcceleration = 20f;

	public override float GetMaxSpeed()
	{
		if (!IsSliding())
		{
			return base.GetMaxSpeed();
		}
		return base.maxWalkSpeed;
	}

	public override float GetMaxAcceleration()
	{
		if (!IsSliding())
		{
			return base.GetMaxAcceleration();
		}
		return base.maxAcceleration * 0.1f;
	}

	public override bool IsWalking()
	{
		if (!IsSliding())
		{
			return base.IsWalking();
		}
		return true;
	}

	public bool IsSliding()
	{
		if (base.movementMode == MovementMode.Custom)
		{
			return base.customMovementMode == 1;
		}
		return false;
	}

	protected virtual bool CanSlide()
	{
		if (!IsGrounded())
		{
			return false;
		}
		float sqrMagnitude = base.velocity.sqrMagnitude;
		float num = base.maxWalkSpeedCrouched * base.maxWalkSpeedCrouched;
		return sqrMagnitude >= num * 1.02f;
	}

	protected virtual Vector3 CalcSlideDirection()
	{
		Vector3 vector = GetMovementDirection();
		if (vector.isZero())
		{
			vector = GetVelocity();
		}
		else if (vector.isZero())
		{
			vector = GetForwardVector();
		}
		return ConstrainInputVector(vector).normalized;
	}

	protected virtual void CheckSlideInput()
	{
		bool flag = IsSliding();
		bool flag2 = base.crouchInputPressed;
		if (!flag && flag2 && CanSlide())
		{
			SetMovementMode(MovementMode.Custom, 1);
		}
		else if (flag && (!flag2 || !CanSlide()))
		{
			SetMovementMode(MovementMode.Walking);
		}
	}

	protected override void OnMovementModeChanged(MovementMode prevMovementMode, int prevCustomMode)
	{
		base.OnMovementModeChanged(prevMovementMode, prevCustomMode);
		if (IsSliding())
		{
			Vector3 vector = CalcSlideDirection();
			base.characterMovement.velocity += vector * slideImpulse;
			SetRotationMode(RotationMode.None);
		}
		if (prevMovementMode == MovementMode.Custom && prevCustomMode == 1)
		{
			SetRotationMode(RotationMode.OrientRotationToMovement);
			if (IsFalling())
			{
				Vector3 onNormal = -GetGravityDirection();
				Vector3 vector2 = Vector3.Project(base.velocity, onNormal);
				Vector3 vector3 = Vector3.ClampMagnitude(base.velocity - vector2, base.maxWalkSpeed);
				base.characterMovement.velocity = vector3 + vector2;
			}
		}
	}

	protected override void OnBeforeSimulationUpdate(float deltaTime)
	{
		base.OnBeforeSimulationUpdate(deltaTime);
		CheckSlideInput();
	}

	protected virtual void SlidingMovementMode(float deltaTime)
	{
		Vector3 desiredVelocity = Vector3.Project(GetDesiredVelocity(), GetRightVector());
		base.characterMovement.velocity = CalcVelocity(base.characterMovement.velocity, desiredVelocity, base.groundFriction * 0.2f, isFluid: false, deltaTime);
		Vector3 normalized = Vector3.ProjectOnPlane(GetGravityDirection(), base.characterMovement.groundNormal).normalized;
		base.characterMovement.velocity += slideDownAcceleration * deltaTime * normalized;
		if (base.applyStandingDownwardForce)
		{
			ApplyDownwardsForce();
		}
	}

	protected override void CustomMovementMode(float deltaTime)
	{
		base.CustomMovementMode(deltaTime);
		if (base.customMovementMode == 1)
		{
			SlidingMovementMode(deltaTime);
		}
	}
}
