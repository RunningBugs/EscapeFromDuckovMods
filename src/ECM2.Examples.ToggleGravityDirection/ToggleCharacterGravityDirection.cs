using UnityEngine;

namespace ECM2.Examples.ToggleGravityDirection;

public class ToggleCharacterGravityDirection : MonoBehaviour
{
	private Character _character;

	private void RotateCharacterToGravity(float deltaTime)
	{
		Vector3 toDirection = -1f * _character.GetGravityDirection();
		Vector3 upVector = _character.GetUpVector();
		Quaternion rotation = _character.GetRotation();
		Quaternion to = Quaternion.FromToRotation(upVector, toDirection) * rotation;
		rotation = Quaternion.RotateTowards(rotation, to, _character.rotationRate * deltaTime);
		_character.SetRotation(rotation);
	}

	private void OnAfterSimulationUpdated(float deltaTime)
	{
		RotateCharacterToGravity(deltaTime);
	}

	private void Awake()
	{
		_character = GetComponent<Character>();
	}

	private void OnEnable()
	{
		_character.AfterSimulationUpdated += OnAfterSimulationUpdated;
	}

	private void OnDisable()
	{
		_character.AfterSimulationUpdated -= OnAfterSimulationUpdated;
	}

	private void Update()
	{
		if (_character.IsFalling() && Input.GetKeyDown(KeyCode.E))
		{
			_character.gravityScale *= -1f;
		}
	}
}
