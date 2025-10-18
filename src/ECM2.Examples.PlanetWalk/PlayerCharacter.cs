using UnityEngine;

namespace ECM2.Examples.PlanetWalk;

public class PlayerCharacter : Character
{
	[Space(15f)]
	public Transform planetTransform;

	protected override void UpdateRotation(float deltaTime)
	{
		base.UpdateRotation(deltaTime);
		SetGravityVector((planetTransform.position - GetPosition()).normalized * GetGravityMagnitude());
		Vector3 toDirection = GetGravityDirection() * -1f;
		Quaternion quaternion = Quaternion.FromToRotation(GetUpVector(), toDirection) * GetRotation();
		SetRotation(quaternion);
	}

	private void Update()
	{
		Vector2 vector = new Vector2
		{
			x = Input.GetAxisRaw("Horizontal"),
			y = Input.GetAxisRaw("Vertical")
		};
		Vector3 zero = Vector3.zero;
		zero += Vector3.right * vector.x;
		zero += Vector3.forward * vector.y;
		zero = zero.relativeTo(base.cameraTransform, GetUpVector());
		SetMovementDirection(zero);
		if (Input.GetButton("Jump"))
		{
			Jump();
		}
		else if (Input.GetButtonUp("Jump"))
		{
			StopJumping();
		}
	}
}
