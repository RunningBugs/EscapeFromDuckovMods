using UnityEngine;

namespace ECM2.Examples.FirstPersonFly;

public class CharacterInput : MonoBehaviour
{
	private Character _character;

	private void Awake()
	{
		_character = GetComponent<Character>();
	}

	private void Update()
	{
		Vector2 vector = new Vector2
		{
			x = Input.GetAxisRaw("Horizontal"),
			y = Input.GetAxisRaw("Vertical")
		};
		Vector3 zero = Vector3.zero;
		if (_character.IsFlying())
		{
			zero += _character.GetRightVector() * vector.x;
			Vector3 vector2 = (_character.camera ? _character.cameraTransform.forward : _character.GetForwardVector());
			zero += vector2 * vector.y;
			if (_character.jumpInputPressed)
			{
				zero += Vector3.up;
			}
		}
		else
		{
			zero += _character.GetRightVector() * vector.x;
			zero += _character.GetForwardVector() * vector.y;
		}
		_character.SetMovementDirection(zero);
		if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
		{
			_character.Crouch();
		}
		else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C))
		{
			_character.UnCrouch();
		}
		if (Input.GetButtonDown("Jump"))
		{
			_character.Jump();
		}
		else if (Input.GetButtonUp("Jump"))
		{
			_character.StopJumping();
		}
	}
}
