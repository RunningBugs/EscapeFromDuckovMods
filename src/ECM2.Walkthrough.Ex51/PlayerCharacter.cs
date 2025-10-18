using UnityEngine;

namespace ECM2.Walkthrough.Ex51;

public class PlayerCharacter : Character
{
	protected override void OnCollided(ref CollisionResult collisionResult)
	{
		base.OnCollided(ref collisionResult);
		Debug.Log("Collided with " + collisionResult.collider.name);
	}

	protected override void OnFoundGround(ref FindGroundResult foundGround)
	{
		base.OnFoundGround(ref foundGround);
		Debug.Log("Found " + foundGround.collider.name + " ground");
	}

	protected override void OnLanded(Vector3 landingVelocity)
	{
		base.OnLanded(landingVelocity);
		Debug.Log($"Landed with {landingVelocity:F4} landing velocity.");
	}

	protected override void OnCrouched()
	{
		base.OnCrouched();
		Debug.Log("Crouched");
	}

	protected override void OnUnCrouched()
	{
		base.OnUnCrouched();
		Debug.Log("UnCrouched");
	}

	protected override void OnJumped()
	{
		base.OnJumped();
		Debug.Log("Jumped!");
		base.notifyJumpApex = true;
	}

	protected override void OnReachedJumpApex()
	{
		base.OnReachedJumpApex();
		Debug.Log($"Apex reached {GetVelocity():F4}");
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
	}
}
