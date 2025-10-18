using UnityEngine;

namespace ECM2.Examples.Ladders;

public class LadderClimbAbility : MonoBehaviour
{
	public enum CustomMovementMode
	{
		Climbing = 1
	}

	public enum ClimbingState
	{
		None,
		Grabbing,
		Grabbed,
		Releasing
	}

	public float climbingSpeed = 5f;

	public float grabbingTime = 0.25f;

	public LayerMask ladderMask;

	private Character _character;

	private Ladder _activeLadder;

	private float _ladderPathPosition;

	private Vector3 _ladderStartPosition;

	private Vector3 _ladderTargetPosition;

	private Quaternion _ladderStartRotation;

	private Quaternion _ladderTargetRotation;

	private float _ladderTime;

	private ClimbingState _climbingState;

	private Character.RotationMode _previousRotationMode;

	public bool IsClimbing()
	{
		if (_character.movementMode == Character.MovementMode.Custom)
		{
			return _character.customMovementMode == 1;
		}
		return false;
	}

	private bool CanClimb()
	{
		if (_character.IsCrouched())
		{
			return false;
		}
		int overlapCount;
		Collider[] array = _character.characterMovement.OverlapTest(ladderMask, QueryTriggerInteraction.Collide, out overlapCount);
		if (overlapCount == 0)
		{
			return false;
		}
		if (!array[0].TryGetComponent<Ladder>(out var component))
		{
			return false;
		}
		_activeLadder = component;
		return true;
	}

	public void Climb()
	{
		if (!IsClimbing() && CanClimb())
		{
			_character.SetMovementMode(Character.MovementMode.Custom, 1);
			_ladderStartPosition = _character.GetPosition();
			_ladderTargetPosition = _activeLadder.ClosestPointOnPath(_ladderStartPosition, out _ladderPathPosition);
			_ladderStartRotation = _character.GetRotation();
			_ladderTargetRotation = _activeLadder.transform.rotation;
		}
	}

	public void StopClimbing()
	{
		if (IsClimbing() && _climbingState == ClimbingState.Grabbed)
		{
			_climbingState = ClimbingState.Releasing;
			_ladderStartPosition = _character.GetPosition();
			_ladderStartRotation = _character.GetRotation();
			_ladderTargetPosition = _ladderStartPosition;
			_ladderTargetRotation = _activeLadder.BottomPoint.rotation;
		}
	}

	private void ClimbingMovementMode(float deltaTime)
	{
		Vector3 velocity = Vector3.zero;
		switch (_climbingState)
		{
		case ClimbingState.Grabbing:
		case ClimbingState.Releasing:
			_ladderTime += deltaTime;
			if (_ladderTime <= grabbingTime)
			{
				velocity = (Vector3.Lerp(_ladderStartPosition, _ladderTargetPosition, _ladderTime / grabbingTime) - base.transform.position) / deltaTime;
				break;
			}
			_ladderTime = 0f;
			if (_climbingState == ClimbingState.Grabbing)
			{
				_climbingState = ClimbingState.Grabbed;
			}
			else if (_climbingState == ClimbingState.Releasing)
			{
				_character.SetMovementMode(Character.MovementMode.Falling);
			}
			break;
		case ClimbingState.Grabbed:
			_activeLadder.ClosestPointOnPath(_character.GetPosition(), out _ladderPathPosition);
			if (Mathf.Abs(_ladderPathPosition) < 0.05f)
			{
				Vector3 movementDirection = _character.GetMovementDirection();
				velocity = _activeLadder.transform.up * (movementDirection.z * climbingSpeed);
				break;
			}
			_climbingState = ClimbingState.Releasing;
			_ladderStartPosition = _character.GetPosition();
			_ladderStartRotation = _character.GetRotation();
			if (_ladderPathPosition > 0f)
			{
				_ladderTargetPosition = _activeLadder.TopPoint.position;
				_ladderTargetRotation = _activeLadder.TopPoint.rotation;
			}
			else if (_ladderPathPosition < 0f)
			{
				_ladderTargetPosition = _activeLadder.BottomPoint.position;
				_ladderTargetRotation = _activeLadder.BottomPoint.rotation;
			}
			break;
		}
		_character.SetVelocity(velocity);
	}

	private void OnMovementModeChanged(Character.MovementMode prevMovementMode, int prevCustomMovementMode)
	{
		if (IsClimbing())
		{
			_climbingState = ClimbingState.Grabbing;
			_character.StopJumping();
			_character.EnableGroundConstraint(enable: false);
			_previousRotationMode = _character.rotationMode;
			_character.SetRotationMode(Character.RotationMode.Custom);
		}
		if (prevMovementMode == Character.MovementMode.Custom && prevCustomMovementMode == 1)
		{
			_climbingState = ClimbingState.None;
			_character.EnableGroundConstraint(enable: true);
			_character.SetRotationMode(_previousRotationMode);
		}
	}

	private void OnCustomMovementModeUpdated(float deltaTime)
	{
		if (IsClimbing())
		{
			ClimbingMovementMode(deltaTime);
		}
	}

	private void OnCustomRotationModeUpdated(float deltaTime)
	{
		if (IsClimbing() && (_climbingState == ClimbingState.Grabbing || _climbingState == ClimbingState.Releasing))
		{
			Quaternion rotation = Quaternion.Slerp(_ladderStartRotation, _ladderTargetRotation, _ladderTime / grabbingTime);
			_character.SetRotation(rotation);
		}
	}

	private void Awake()
	{
		_character = GetComponent<Character>();
	}

	private void OnEnable()
	{
		_character.MovementModeChanged += OnMovementModeChanged;
		_character.CustomMovementModeUpdated += OnCustomMovementModeUpdated;
		_character.CustomRotationModeUpdated += OnCustomRotationModeUpdated;
	}

	private void OnDisable()
	{
		_character.MovementModeChanged -= OnMovementModeChanged;
		_character.CustomMovementModeUpdated -= OnCustomMovementModeUpdated;
		_character.CustomRotationModeUpdated -= OnCustomRotationModeUpdated;
	}
}
