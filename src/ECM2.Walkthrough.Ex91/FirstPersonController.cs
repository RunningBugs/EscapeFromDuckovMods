using UnityEngine;

namespace ECM2.Walkthrough.Ex91;

public class FirstPersonController : MonoBehaviour
{
	[Header("Cinemachine")]
	[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow.")]
	public GameObject cameraTarget;

	[Tooltip("How far in degrees can you move the camera up.")]
	public float maxPitch = 80f;

	[Tooltip("How far in degrees can you move the camera down.")]
	public float minPitch = -80f;

	[Space(15f)]
	[Tooltip("Cinemachine Virtual Camera positioned at desired crouched height.")]
	public GameObject crouchedCamera;

	[Tooltip("Cinemachine Virtual Camera positioned at desired un-crouched height.")]
	public GameObject unCrouchedCamera;

	[Space(15f)]
	[Tooltip("Mouse look sensitivity")]
	public Vector2 lookSensitivity = new Vector2(1.5f, 1.25f);

	private Character _character;

	private float _cameraTargetPitch;

	public void AddControlYawInput(float value)
	{
		_character.AddYawInput(value);
	}

	public void AddControlPitchInput(float value, float minValue = -80f, float maxValue = 80f)
	{
		if (value != 0f)
		{
			_cameraTargetPitch = MathLib.ClampAngle(_cameraTargetPitch + value, minValue, maxValue);
			cameraTarget.transform.localRotation = Quaternion.Euler(0f - _cameraTargetPitch, 0f, 0f);
		}
	}

	private void OnCrouched()
	{
		crouchedCamera.SetActive(value: true);
		unCrouchedCamera.SetActive(value: false);
	}

	private void OnUnCrouched()
	{
		crouchedCamera.SetActive(value: false);
		unCrouchedCamera.SetActive(value: true);
	}

	private void Awake()
	{
		_character = GetComponent<Character>();
	}

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		_character.SetRotationMode(Character.RotationMode.None);
	}

	private void OnEnable()
	{
		_character.Crouched += OnCrouched;
		_character.UnCrouched += OnUnCrouched;
	}

	private void OnDisable()
	{
		_character.Crouched -= OnCrouched;
		_character.UnCrouched -= OnUnCrouched;
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
		Vector2 vector2 = new Vector2
		{
			x = Input.GetAxisRaw("Mouse X"),
			y = Input.GetAxisRaw("Mouse Y")
		};
		AddControlYawInput(vector2.x * lookSensitivity.x);
		AddControlPitchInput(vector2.y * lookSensitivity.y, minPitch, maxPitch);
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
