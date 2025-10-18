using UnityEngine;

namespace ECM2.Examples.SideScrolling;

public class SideScrollingCharacter : Character
{
	protected override void Awake()
	{
		base.Awake();
		SetRotationMode(RotationMode.None);
	}

	private void Update()
	{
		float axisRaw = Input.GetAxisRaw("Horizontal");
		SetMovementDirection(Vector3.right * axisRaw);
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
		if (axisRaw != 0f)
		{
			SetYaw(axisRaw * 90f);
		}
	}
}
