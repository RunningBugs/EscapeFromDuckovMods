using ECM2;
using UnityEngine;

public class Movement : MonoBehaviour
{
	public CharacterMainControl characterController;

	[SerializeField]
	private CharacterMovement characterMovement;

	public Vector3 targetAimDirection;

	private Vector3 moveInput;

	private bool running;

	private bool moving;

	private Vector3 currentMoveDirectionXZ;

	public bool forceMove;

	public Vector3 forceMoveVelocity;

	private const float movingInputThreshold = 0.02f;

	public float walkSpeed => characterController.CharacterWalkSpeed * (characterController.IsInAdsInput ? characterController.AdsWalkSpeedMultiplier : 1f);

	public float originWalkSpeed => characterController.CharacterOriginWalkSpeed;

	public float runSpeed => characterController.CharacterRunSpeed;

	public float walkAcc => characterController.CharacterWalkAcc;

	public float runAcc => characterController.CharacterRunAcc;

	public float turnSpeed => characterController.CharacterTurnSpeed;

	public float aimTurnSpeed => characterController.CharacterAimTurnSpeed;

	public Vector3 MoveInput => moveInput;

	public bool Running => running;

	public bool Moving => moving;

	public bool IsOnGround => characterMovement.isOnGround;

	public bool StandStill
	{
		get
		{
			if (!moving)
			{
				return characterMovement.velocity.magnitude < 0.1f;
			}
			return false;
		}
	}

	private bool checkCanMove => characterController.CanMove();

	private bool checkCanRun => characterController.CanRun();

	public Vector3 CurrentMoveDirectionXZ => currentMoveDirectionXZ;

	public Transform rotationRoot => characterController.modelRoot;

	public Vector3 Velocity => characterMovement.velocity;

	private void Awake()
	{
		characterMovement.constrainToGround = true;
	}

	public void SetMoveInput(Vector3 _moveInput)
	{
		_moveInput.y = 0f;
		moveInput = _moveInput;
		moving = false;
		if (checkCanMove && moveInput.magnitude > 0.02f)
		{
			moving = true;
		}
	}

	public void SetForceMoveVelocity(Vector3 _forceMoveVelocity)
	{
		forceMove = true;
		forceMoveVelocity = _forceMoveVelocity;
	}

	public void SetAimDirection(Vector3 _aimDirection)
	{
		targetAimDirection = _aimDirection;
		targetAimDirection.y = 0f;
		targetAimDirection.Normalize();
	}

	public void SetAimDirectionToTarget(Vector3 targetPoint, Transform aimHandler)
	{
		Vector3 position = base.transform.position;
		position.y = 0f;
		Vector3 position2 = aimHandler.position;
		position2.y = 0f;
		targetPoint.y = 0f;
		float num = Vector3.Distance(position, targetPoint);
		float num2 = Vector3.Distance(position, position2);
		if (!(num < num2 + 0.25f))
		{
			float num3 = Mathf.Asin(num2 / num) * 57.29578f;
			targetAimDirection = Quaternion.Euler(0f, 0f - num3, 0f) * (targetPoint - position).normalized;
		}
	}

	private void UpdateAiming()
	{
		Vector3 currentAimPoint = characterController.GetCurrentAimPoint();
		currentAimPoint.y = base.transform.position.y;
		if (Vector3.Distance(currentAimPoint, base.transform.position) > 0.6f && characterController.IsAiming() && characterController.CanControlAim())
		{
			SetAimDirectionToTarget(currentAimPoint, characterController.CurrentUsingAimSocket);
		}
		else if (Moving)
		{
			SetAimDirection(CurrentMoveDirectionXZ);
		}
	}

	public void UpdateMovement()
	{
		bool flag = checkCanRun;
		bool flag2 = checkCanMove;
		if (moveInput.magnitude <= 0.02f || !flag2)
		{
			moving = false;
			running = false;
		}
		else
		{
			moving = true;
		}
		if (!flag)
		{
			running = false;
		}
		if (moving && flag)
		{
			running = true;
		}
		if (!forceMove)
		{
			UpdateNormalMove();
		}
		else
		{
			UpdateForceMove();
			forceMove = false;
		}
		UpdateAiming();
		UpdateRotation(Time.deltaTime);
		characterMovement.velocity += Physics.gravity * Time.deltaTime;
		characterMovement.Move(characterMovement.velocity, Time.deltaTime);
	}

	private void Update()
	{
	}

	public void ForceSetPosition(Vector3 Pos)
	{
		characterMovement.PauseGroundConstraint(1f);
		characterMovement.SetPosition(Pos);
		characterMovement.velocity = Vector3.zero;
	}

	private void UpdateNormalMove()
	{
		Vector3 velocity = characterMovement.velocity;
		Vector3 target = Vector3.zero;
		float num = walkAcc;
		if (moving)
		{
			target = moveInput * (running ? runSpeed : walkSpeed);
			num = (running ? runAcc : walkAcc);
		}
		target.y = velocity.y;
		velocity = Vector3.MoveTowards(velocity, target, num * Time.deltaTime);
		Vector3 vector = velocity;
		vector.y = 0f;
		if (vector.magnitude > 0.02f)
		{
			currentMoveDirectionXZ = vector.normalized;
		}
		characterMovement.velocity = velocity;
	}

	private void UpdateForceMove()
	{
		Vector3 velocity = characterMovement.velocity;
		Vector3 vector = forceMoveVelocity;
		_ = walkAcc;
		vector.y = velocity.y;
		velocity = vector;
		Vector3 vector2 = velocity;
		vector2.y = 0f;
		if (vector2.magnitude > 0.02f)
		{
			currentMoveDirectionXZ = vector2.normalized;
		}
		characterMovement.velocity = velocity;
	}

	public void ForceTurnTo(Vector3 direction)
	{
		targetAimDirection = direction.normalized;
		Quaternion rotation = Quaternion.Euler(0f, Quaternion.LookRotation(targetAimDirection, Vector3.up).eulerAngles.y, 0f);
		rotationRoot.rotation = rotation;
	}

	private void UpdateRotation(float deltaTime)
	{
		if (targetAimDirection.magnitude < 0.1f)
		{
			targetAimDirection = rotationRoot.forward;
		}
		float num = turnSpeed;
		if (characterController.IsAiming() && characterController.IsMainCharacter)
		{
			num = aimTurnSpeed;
		}
		if (targetAimDirection.magnitude > 0.1f)
		{
			Quaternion to = Quaternion.Euler(0f, Quaternion.LookRotation(targetAimDirection, Vector3.up).eulerAngles.y, 0f);
			rotationRoot.rotation = Quaternion.RotateTowards(rotationRoot.rotation, to, num * deltaTime);
		}
	}

	public void ForceSetAimDirectionToAimPoint()
	{
		UpdateRotation(99999f);
	}

	public float GetMoveAnimationValue()
	{
		float num = 0f;
		float magnitude = characterMovement.velocity.magnitude;
		if (moving && running)
		{
			num = Mathf.InverseLerp(walkSpeed, runSpeed, magnitude) + 1f;
			num *= walkSpeed / originWalkSpeed;
		}
		else
		{
			num = Mathf.Clamp01(magnitude / walkSpeed);
			num *= walkSpeed / originWalkSpeed;
		}
		if (walkSpeed <= 0f)
		{
			num = 0f;
		}
		return num;
	}

	public Vector2 GetLocalMoveDirectionAnimationValue()
	{
		Vector2 up = Vector2.up;
		if (!StandStill)
		{
			Vector3 direction = currentMoveDirectionXZ;
			Vector3 vector = rotationRoot.InverseTransformDirection(direction);
			up.x = vector.x;
			up.y = vector.z;
		}
		return up;
	}

	private void FixedUpdate()
	{
	}
}
