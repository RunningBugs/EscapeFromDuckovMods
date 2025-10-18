using Cinemachine;
using UnityEngine;

namespace ECM2.Walkthrough.Ex92;

public class ThirdPersonController : MonoBehaviour
{
	[Header("Cinemachine")]
	[Tooltip("The CM virtual Camera following the target.")]
	public CinemachineVirtualCamera followCamera;

	[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow.")]
	public GameObject followTarget;

	[Tooltip("The default distance behind the Follow target.")]
	[SerializeField]
	public float followDistance = 5f;

	[Tooltip("The minimum distance to Follow target.")]
	[SerializeField]
	public float followMinDistance = 2f;

	[Tooltip("The maximum distance to Follow target.")]
	[SerializeField]
	public float followMaxDistance = 10f;

	[Tooltip("How far in degrees can you move the camera up.")]
	public float maxPitch = 80f;

	[Tooltip("How far in degrees can you move the camera down.")]
	public float minPitch = -80f;

	[Space(15f)]
	public bool invertLook = true;

	[Tooltip("Mouse look sensitivity")]
	public Vector2 lookSensitivity = new Vector2(1.5f, 1.25f);

	private Character _character;

	private float _cameraTargetYaw;

	private float _cameraTargetPitch;

	private Cinemachine3rdPersonFollow _cmThirdPersonFollow;

	protected float _followDistanceSmoothVelocity;

	public void AddControlYawInput(float value, float minValue = -180f, float maxValue = 180f)
	{
		if (value != 0f)
		{
			_cameraTargetYaw = MathLib.ClampAngle(_cameraTargetYaw + value, minValue, maxValue);
		}
	}

	public void AddControlPitchInput(float value, float minValue = -80f, float maxValue = 80f)
	{
		if (value != 0f)
		{
			if (invertLook)
			{
				value = 0f - value;
			}
			_cameraTargetPitch = MathLib.ClampAngle(_cameraTargetPitch + value, minValue, maxValue);
		}
	}

	public virtual void AddControlZoomInput(float value)
	{
		followDistance = Mathf.Clamp(followDistance - value, followMinDistance, followMaxDistance);
	}

	private void UpdateCamera()
	{
		followTarget.transform.rotation = Quaternion.Euler(_cameraTargetPitch, _cameraTargetYaw, 0f);
		_cmThirdPersonFollow.CameraDistance = Mathf.SmoothDamp(_cmThirdPersonFollow.CameraDistance, followDistance, ref _followDistanceSmoothVelocity, 0.1f);
	}

	private void Awake()
	{
		_character = GetComponent<Character>();
		_cmThirdPersonFollow = followCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
		if ((bool)_cmThirdPersonFollow)
		{
			_cmThirdPersonFollow.CameraDistance = followDistance;
		}
	}

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
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
		Vector2 vector3 = new Vector2
		{
			x = Input.GetAxisRaw("Mouse X"),
			y = Input.GetAxisRaw("Mouse Y")
		};
		AddControlYawInput(vector3.x * lookSensitivity.x);
		AddControlPitchInput(vector3.y * lookSensitivity.y, minPitch, maxPitch);
		float axisRaw = Input.GetAxisRaw("Mouse ScrollWheel");
		AddControlZoomInput(axisRaw);
	}

	private void LateUpdate()
	{
		UpdateCamera();
	}
}
