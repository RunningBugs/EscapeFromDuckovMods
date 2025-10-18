using UnityEngine;

namespace ECM2.Examples.FirstPersonFly;

public class FlyAbility : MonoBehaviour
{
	public bool canEverFly = true;

	private Character _character;

	private bool IsFlyAllowed()
	{
		if (canEverFly)
		{
			return _character.IsFalling();
		}
		return false;
	}

	protected virtual bool CanFly()
	{
		bool flag = IsFlyAllowed();
		if (flag)
		{
			Vector3 rhs = -_character.GetGravityDirection();
			flag = Vector3.Dot(_character.GetVelocity(), rhs) < 0f;
		}
		return flag;
	}

	private void OnCollided(ref CollisionResult collisionResult)
	{
		if (_character.IsFlying() && collisionResult.isWalkable)
		{
			_character.SetMovementMode(Character.MovementMode.Falling);
		}
	}

	private void OnBeforeSimulationUpdated(float deltaTime)
	{
		bool num = _character.IsFlying();
		bool jumpInputPressed = _character.jumpInputPressed;
		if (!num && jumpInputPressed && CanFly())
		{
			_character.SetMovementMode(Character.MovementMode.Flying);
		}
	}

	private void Awake()
	{
		_character = GetComponent<Character>();
	}

	private void OnEnable()
	{
		_character.Collided += OnCollided;
		_character.BeforeSimulationUpdated += OnBeforeSimulationUpdated;
	}

	private void OnDisable()
	{
		_character.Collided -= OnCollided;
		_character.BeforeSimulationUpdated -= OnBeforeSimulationUpdated;
	}
}
