using UnityEngine;

namespace ECM2.Walkthrough.Ex22;

public class PlayerController : MonoBehaviour
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
		Vector3 vector2 = Vector3.zero;
		vector2 += Vector3.right * vector.x;
		vector2 += Vector3.forward * vector.y;
		if ((bool)_character.camera)
		{
			vector2 = vector2.relativeTo(_character.cameraTransform);
		}
		_character.SetMovementDirection(vector2);
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
