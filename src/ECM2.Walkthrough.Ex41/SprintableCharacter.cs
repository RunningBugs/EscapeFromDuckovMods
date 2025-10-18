using UnityEngine;

namespace ECM2.Walkthrough.Ex41;

public class SprintableCharacter : Character
{
	[Space(15f)]
	public float maxSprintSpeed = 10f;

	private bool _isSprinting;

	private bool _sprintInputPressed;

	public void Sprint()
	{
		_sprintInputPressed = true;
	}

	public void StopSprinting()
	{
		_sprintInputPressed = false;
	}

	public bool IsSprinting()
	{
		return _isSprinting;
	}

	private bool CanSprint()
	{
		if (IsWalking())
		{
			return !IsCrouched();
		}
		return false;
	}

	private void CheckSprintInput()
	{
		if (!_isSprinting && _sprintInputPressed && CanSprint())
		{
			_isSprinting = true;
		}
		else if (_isSprinting && (!_sprintInputPressed || !CanSprint()))
		{
			_isSprinting = false;
		}
	}

	public override float GetMaxSpeed()
	{
		if (!_isSprinting)
		{
			return base.GetMaxSpeed();
		}
		return maxSprintSpeed;
	}

	protected override void OnBeforeSimulationUpdate(float deltaTime)
	{
		base.OnBeforeSimulationUpdate(deltaTime);
		CheckSprintInput();
	}

	private void Update()
	{
		Vector2 vector = new Vector2
		{
			x = Input.GetAxisRaw("Horizontal"),
			y = Input.GetAxisRaw("Vertical")
		};
		Vector3 vector2 = Vector3.zero;
		vector2 += Vector3.right * vector.x;
		vector2 += Vector3.forward * vector.y;
		if ((bool)base.camera)
		{
			vector2 = vector2.relativeTo(base.cameraTransform);
		}
		SetMovementDirection(vector2);
		if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
		{
			Crouch();
		}
		else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C))
		{
			UnCrouch();
		}
		if (Input.GetButtonDown("Jump"))
		{
			Jump();
		}
		else if (Input.GetButtonUp("Jump"))
		{
			StopJumping();
		}
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			Sprint();
		}
		else if (Input.GetKeyUp(KeyCode.LeftShift))
		{
			StopSprinting();
		}
	}
}
