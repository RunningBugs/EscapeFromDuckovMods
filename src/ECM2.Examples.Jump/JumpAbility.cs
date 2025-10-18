using UnityEngine;

namespace ECM2.Examples.Jump;

public class JumpAbility : MonoBehaviour
{
	[Space(15f)]
	[Tooltip("Is the character able to jump ?")]
	[SerializeField]
	private bool _canEverJump;

	[Tooltip("Can jump while crouching ?")]
	[SerializeField]
	private bool _jumpWhileCrouching;

	[Tooltip("The max number of jumps the Character can perform.")]
	[SerializeField]
	private int _jumpMaxCount;

	[Tooltip("Initial velocity (instantaneous vertical velocity) when jumping.")]
	[SerializeField]
	private float _jumpImpulse;

	[Tooltip("The maximum time (in seconds) to hold the jump. eg: Variable height jump.")]
	[SerializeField]
	private float _jumpMaxHoldTime;

	[Tooltip("How early before hitting the ground you can trigger a jump (in seconds).")]
	[SerializeField]
	private float _jumpPreGroundedTime;

	[Tooltip("How long after leaving the ground you can trigger a jump (in seconds).")]
	[SerializeField]
	private float _jumpPostGroundedTime;

	private Character _character;

	protected bool _jumpButtonPressed;

	protected float _jumpButtonHeldDownTime;

	protected float _jumpHoldTime;

	protected int _jumpCount;

	protected bool _isJumping;

	public bool canEverJump
	{
		get
		{
			return _canEverJump;
		}
		set
		{
			_canEverJump = value;
		}
	}

	public bool jumpWhileCrouching
	{
		get
		{
			return _jumpWhileCrouching;
		}
		set
		{
			_jumpWhileCrouching = value;
		}
	}

	public int jumpMaxCount
	{
		get
		{
			return _jumpMaxCount;
		}
		set
		{
			_jumpMaxCount = Mathf.Max(1, value);
		}
	}

	public float jumpImpulse
	{
		get
		{
			return _jumpImpulse;
		}
		set
		{
			_jumpImpulse = Mathf.Max(0f, value);
		}
	}

	public float jumpMaxHoldTime
	{
		get
		{
			return _jumpMaxHoldTime;
		}
		set
		{
			_jumpMaxHoldTime = Mathf.Max(0f, value);
		}
	}

	public float jumpPreGroundedTime
	{
		get
		{
			return _jumpPreGroundedTime;
		}
		set
		{
			_jumpPreGroundedTime = Mathf.Max(0f, value);
		}
	}

	public float jumpPostGroundedTime
	{
		get
		{
			return _jumpPostGroundedTime;
		}
		set
		{
			_jumpPostGroundedTime = Mathf.Max(0f, value);
		}
	}

	public bool jumpButtonPressed => _jumpButtonPressed;

	public float jumpButtonHeldDownTime => _jumpButtonHeldDownTime;

	public int jumpCount => _jumpCount;

	public float jumpHoldTime => _jumpHoldTime;

	public virtual bool IsJumping()
	{
		return _isJumping;
	}

	public void Jump()
	{
		_jumpButtonPressed = true;
	}

	public void StopJumping()
	{
		_jumpButtonPressed = false;
		_jumpButtonHeldDownTime = 0f;
		_isJumping = false;
		_jumpHoldTime = 0f;
	}

	public virtual int GetJumpCount()
	{
		return _jumpCount;
	}

	public virtual bool CanJump()
	{
		if (!canEverJump)
		{
			return false;
		}
		if (_character.IsCrouched() && !jumpWhileCrouching)
		{
			return false;
		}
		if (jumpMaxCount == 0 || _jumpCount >= jumpMaxCount)
		{
			return false;
		}
		if (_jumpCount == 0)
		{
			bool flag = _character.IsWalking() || (_character.IsFalling() && jumpPostGroundedTime > 0f && _character.fallingTime < jumpPostGroundedTime);
			if (_character.IsFalling() && !flag)
			{
				flag = jumpMaxCount > 1;
				if (flag)
				{
					_jumpCount++;
				}
			}
			return flag;
		}
		return _character.IsFalling();
	}

	protected virtual Vector3 CalcJumpImpulse()
	{
		Vector3 vector = -_character.GetGravityDirection();
		float num = Mathf.Max(Vector3.Dot(_character.GetVelocity(), vector), jumpImpulse);
		return vector * num;
	}

	protected virtual void DoJump(float deltaTime)
	{
		if (_jumpButtonPressed)
		{
			_jumpButtonHeldDownTime += deltaTime;
		}
		if (_jumpButtonPressed && !IsJumping() && (!(jumpPreGroundedTime > 0f) || _jumpButtonHeldDownTime <= jumpPreGroundedTime) && CanJump())
		{
			_character.SetMovementMode(Character.MovementMode.Falling);
			_character.PauseGroundConstraint();
			_character.LaunchCharacter(CalcJumpImpulse(), overrideVerticalVelocity: true);
			_jumpCount++;
			_isJumping = true;
		}
	}

	protected virtual void Jumping(float deltaTime)
	{
		if (!canEverJump)
		{
			if (IsJumping())
			{
				StopJumping();
			}
			return;
		}
		DoJump(deltaTime);
		if (IsJumping() && _jumpButtonPressed && jumpMaxHoldTime > 0f && _jumpHoldTime < jumpMaxHoldTime)
		{
			Vector3 gravityVector = _character.GetGravityVector();
			float magnitude = gravityVector.magnitude;
			Vector3 obj = ((magnitude > 0f) ? (gravityVector / magnitude) : Vector3.zero);
			float t = Mathf.InverseLerp(0f, jumpMaxHoldTime, _jumpHoldTime);
			float num = Mathf.LerpUnclamped(magnitude, 0f, t);
			Vector3 force = -obj * num;
			_character.AddForce(force);
			_jumpHoldTime += deltaTime;
		}
	}

	protected virtual void OnMovementModeChanged(Character.MovementMode prevMovementMode, int prevCustomMode)
	{
		if (_character.IsWalking())
		{
			_jumpCount = 0;
		}
		else if (_character.IsFlying() || _character.IsSwimming())
		{
			StopJumping();
		}
	}

	protected virtual void Reset()
	{
		_canEverJump = true;
		_jumpWhileCrouching = true;
		_jumpMaxCount = 1;
		_jumpImpulse = 5f;
	}

	protected virtual void OnValidate()
	{
		jumpMaxCount = _jumpMaxCount;
		jumpImpulse = _jumpImpulse;
		jumpMaxHoldTime = _jumpMaxHoldTime;
		jumpPreGroundedTime = _jumpPreGroundedTime;
		jumpPostGroundedTime = _jumpPostGroundedTime;
	}

	protected virtual void Awake()
	{
		_character = GetComponent<Character>();
	}

	protected virtual void OnEnable()
	{
		_character.MovementModeChanged += OnMovementModeChanged;
		_character.BeforeSimulationUpdated += Jumping;
	}

	protected virtual void OnDisable()
	{
		_character.BeforeSimulationUpdated -= Jumping;
		_character.MovementModeChanged -= OnMovementModeChanged;
	}

	protected virtual void Start()
	{
		_character.canEverJump = false;
	}
}
