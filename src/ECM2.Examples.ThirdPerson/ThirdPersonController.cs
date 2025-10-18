using UnityEngine;

namespace ECM2.Examples.ThirdPerson;

public class ThirdPersonController : MonoBehaviour
{
	[Space(15f)]
	public GameObject followTarget;

	[Tooltip("The default distance behind the Follow target.")]
	[SerializeField]
	public float followDistance = 5f;

	[Tooltip("The minimum distance to Follow target.")]
	[SerializeField]
	public float followMinDistance;

	[Tooltip("The maximum distance to Follow target.")]
	[SerializeField]
	public float followMaxDistance = 10f;

	[Space(15f)]
	public bool invertLook = true;

	[Tooltip("Mouse look sensitivity")]
	public Vector2 mouseSensitivity = new Vector2(1f, 1f);

	[Space(15f)]
	[Tooltip("How far in degrees can you move the camera down.")]
	public float minPitch = -80f;

	[Tooltip("How far in degrees can you move the camera up.")]
	public float maxPitch = 80f;

	protected float _cameraYaw;

	protected float _cameraPitch;

	protected float _currentFollowDistance;

	protected float _followDistanceSmoothVelocity;

	protected Character _character;

	public virtual void AddControlYawInput(float value)
	{
		_cameraYaw = MathLib.ClampAngle(_cameraYaw + value, -180f, 180f);
	}

	public virtual void AddControlPitchInput(float value, float minValue = -80f, float maxValue = 80f)
	{
		_cameraPitch = MathLib.ClampAngle(_cameraPitch + value, minValue, maxValue);
	}

	public virtual void AddControlZoomInput(float value)
	{
		followDistance = Mathf.Clamp(followDistance - value, followMinDistance, followMaxDistance);
	}

	protected virtual void UpdateCameraRotation()
	{
		_character.cameraTransform.rotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0f);
	}

	protected virtual void UpdateCameraPosition()
	{
		Transform cameraTransform = _character.cameraTransform;
		_currentFollowDistance = Mathf.SmoothDamp(_currentFollowDistance, followDistance, ref _followDistanceSmoothVelocity, 0.1f);
		cameraTransform.position = followTarget.transform.position - cameraTransform.forward * _currentFollowDistance;
	}

	protected virtual void UpdateCamera()
	{
		UpdateCameraRotation();
		UpdateCameraPosition();
	}

	protected virtual void Awake()
	{
		_character = GetComponent<Character>();
	}

	protected virtual void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Vector3 eulerAngles = _character.cameraTransform.eulerAngles;
		_cameraPitch = eulerAngles.x;
		_cameraYaw = eulerAngles.y;
		_currentFollowDistance = followDistance;
	}

	protected virtual void Update()
	{
		Vector2 vector = new Vector2
		{
			x = Input.GetAxisRaw("Horizontal"),
			y = Input.GetAxisRaw("Vertical")
		};
		Vector3 vector2 = Vector3.zero;
		vector2 += Vector3.right * vector.x;
		vector2 += Vector3.forward * vector.y;
		if ((bool)_character.cameraTransform)
		{
			vector2 = vector2.relativeTo(_character.cameraTransform, _character.GetUpVector());
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
		Vector2 vector3 = new Vector2
		{
			x = Input.GetAxisRaw("Mouse X"),
			y = Input.GetAxisRaw("Mouse Y")
		};
		vector3 *= mouseSensitivity;
		AddControlYawInput(vector3.x);
		AddControlPitchInput(invertLook ? (0f - vector3.y) : vector3.y, minPitch, maxPitch);
		float axisRaw = Input.GetAxisRaw("Mouse ScrollWheel");
		AddControlZoomInput(axisRaw);
	}

	protected virtual void LateUpdate()
	{
		UpdateCamera();
	}
}
