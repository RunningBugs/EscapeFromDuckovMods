using UnityEngine;

namespace FOW.Demos;

public class FowCharacterDemo : MonoBehaviour
{
	public float WalkingSpeed = 5f;

	public float RunningMultiplier = 1.65f;

	public float Acceleration = 25f;

	private float yRot;

	private CharacterController cc;

	private bool CursorLocked;

	private Vector2 inputDirection = Vector2.zero;

	private Vector2 velocityXZ = Vector2.zero;

	private Vector3 velocity = Vector3.zero;

	private float speedTarget;

	private void Awake()
	{
		cc = GetComponent<CharacterController>();
		CursorLocked = true;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			CursorLocked = !CursorLocked;
			if (CursorLocked)
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
			else
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}
		}
		if (CursorLocked)
		{
			base.transform.Rotate(0f, Input.GetAxis("Mouse X"), 0f);
			yRot -= Input.GetAxis("Mouse Y");
		}
		yRot = Mathf.Clamp(yRot, -80f, 80f);
		setInput();
		move();
	}

	public void setInput()
	{
		bool[] obj = new bool[5]
		{
			Input.GetKey(KeyCode.W),
			Input.GetKey(KeyCode.A),
			Input.GetKey(KeyCode.S),
			Input.GetKey(KeyCode.D),
			Input.GetKey(KeyCode.LeftShift)
		};
		speedTarget = 0f;
		inputDirection = Vector2.zero;
		if (obj[0])
		{
			inputDirection.y += 1f;
			speedTarget = WalkingSpeed;
		}
		if (obj[1])
		{
			inputDirection.x -= 1f;
			speedTarget = WalkingSpeed;
		}
		if (obj[2])
		{
			inputDirection.y -= 1f;
			speedTarget = WalkingSpeed;
		}
		if (obj[3])
		{
			inputDirection.x += 1f;
			speedTarget = WalkingSpeed;
		}
		if (obj[4])
		{
			speedTarget *= RunningMultiplier;
		}
	}

	private void move()
	{
		if (cc.isGrounded)
		{
			velocity.y = 0f;
		}
		Vector2 vector = new Vector2(base.transform.forward.x, base.transform.forward.z);
		Vector2 vector2 = Vector3.Normalize(new Vector2(base.transform.right.x, base.transform.right.z) * inputDirection.x + vector * inputDirection.y);
		velocityXZ = Vector2.MoveTowards(velocityXZ, vector2.normalized * speedTarget, Time.deltaTime * Acceleration);
		velocity.x = velocityXZ.x * Time.deltaTime;
		velocity.z = velocityXZ.y * Time.deltaTime;
		velocity.y += -9.81f * Time.deltaTime * Time.deltaTime;
		cc.enabled = true;
		cc.Move(velocity);
		cc.enabled = false;
	}
}
