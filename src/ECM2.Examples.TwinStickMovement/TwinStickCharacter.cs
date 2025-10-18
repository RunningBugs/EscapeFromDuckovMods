using UnityEngine;

namespace ECM2.Examples.TwinStickMovement;

public class TwinStickCharacter : Character
{
	private Vector3 _aimDirection;

	public virtual Vector3 GetAimDirection()
	{
		return _aimDirection;
	}

	public virtual void SetAimDirection(Vector3 worldDirection)
	{
		_aimDirection = worldDirection;
	}

	protected override void CustomRotationMode(float deltaTime)
	{
		base.CustomRotationMode(deltaTime);
		Vector3 worldDirection = (_aimDirection.isZero() ? GetMovementDirection() : GetAimDirection());
		RotateTowards(worldDirection, deltaTime);
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
		Vector3 aimDirection = Vector3.zero;
		if (Input.GetMouseButton(0) && Physics.Raycast(base.camera.ScreenPointToRay(Input.mousePosition), out var hitInfo, float.PositiveInfinity))
		{
			aimDirection = (hitInfo.point - GetPosition()).onlyXZ().normalized;
		}
		SetAimDirection(aimDirection);
	}
}
