using UnityEngine;

namespace ECM2.Walkthrough.Ex21;

public class PlayerCharacter : Character
{
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
	}
}
