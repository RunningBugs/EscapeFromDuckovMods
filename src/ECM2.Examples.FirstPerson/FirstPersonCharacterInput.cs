using UnityEngine;

namespace ECM2.Examples.FirstPerson;

public class FirstPersonCharacterInput : MonoBehaviour
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
		zero += _character.GetRightVector() * vector.x;
		zero += _character.GetForwardVector() * vector.y;
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
