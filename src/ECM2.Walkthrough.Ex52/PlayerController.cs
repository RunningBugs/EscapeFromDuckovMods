using UnityEngine;

namespace ECM2.Walkthrough.Ex52;

public class PlayerController : MonoBehaviour
{
	private Character _character;

	protected void OnCollided(ref CollisionResult collisionResult)
	{
		Debug.Log("Collided with " + collisionResult.collider.name);
	}

	protected void OnFoundGround(ref FindGroundResult foundGround)
	{
		Debug.Log("Found " + foundGround.collider.name + " ground");
	}

	protected void OnLanded(Vector3 landingVelocity)
	{
		Debug.Log($"Landed with {landingVelocity:F4} landing velocity.");
	}

	protected void OnCrouched()
	{
		Debug.Log("Crouched");
	}

	protected void OnUnCrouched()
	{
		Debug.Log("UnCrouched");
	}

	protected void OnJumped()
	{
		Debug.Log("Jumped!");
		_character.notifyJumpApex = true;
	}

	protected void OnReachedJumpApex()
	{
		Debug.Log($"Apex reached {_character.GetVelocity():F4}");
	}

	private void Awake()
	{
		_character = GetComponent<Character>();
	}

	private void OnEnable()
	{
		_character.Collided += OnCollided;
		_character.FoundGround += OnFoundGround;
		_character.Landed += OnLanded;
		_character.Crouched += OnCrouched;
		_character.UnCrouched += OnUnCrouched;
		_character.Jumped += OnJumped;
		_character.ReachedJumpApex += OnReachedJumpApex;
	}

	private void OnDisable()
	{
		_character.Collided -= OnCollided;
		_character.FoundGround -= OnFoundGround;
		_character.Landed -= OnLanded;
		_character.Crouched -= OnCrouched;
		_character.UnCrouched -= OnUnCrouched;
		_character.Jumped -= OnJumped;
		_character.ReachedJumpApex -= OnReachedJumpApex;
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
