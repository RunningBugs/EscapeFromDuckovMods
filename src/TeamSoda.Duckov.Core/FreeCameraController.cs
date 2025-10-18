using CameraSystems;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreeCameraController : MonoBehaviour
{
	[SerializeField]
	private CameraPropertiesControl propertiesControl;

	[SerializeField]
	private float moveSpeed = 10f;

	[SerializeField]
	private float rotateSpeed = 180f;

	[SerializeField]
	private float smoothTime = 2f;

	[SerializeField]
	private Vector2 minMaxXRotation = new Vector2(-89f, 89f);

	[SerializeField]
	private bool projectMovementOnXZPlane;

	[Range(-180f, 180f)]
	private float yaw;

	[Range(-89f, 89f)]
	private float pitch;

	[SerializeField]
	private CinemachineVirtualCamera vCamera;

	private bool followCharacter;

	private Vector3 offsetFromCharacter;

	private Vector3 worldPosTarget;

	private Vector3 velocityWorldSpace;

	private Vector3 velocityLocalSpace;

	private float yawVelocity;

	private float pitchVelocity;

	private float yawTarget;

	private float pitchTarget;

	private Gamepad Gamepad => Gamepad.current;

	private void Awake()
	{
		if (!propertiesControl)
		{
			propertiesControl = GetComponent<CameraPropertiesControl>();
		}
	}

	private void OnEnable()
	{
		SetRotation(base.transform.rotation);
		SnapToMainCamera();
	}

	public void SetRotation(Quaternion rotation)
	{
		Vector3 eulerAngles = rotation.eulerAngles;
		yaw = eulerAngles.y;
		pitch = eulerAngles.x;
		yawTarget = yaw;
		pitchTarget = pitch;
		if (pitch > 180f)
		{
			pitch -= 360f;
		}
		if (pitch < -180f)
		{
			pitch += 360f;
		}
		pitch = Mathf.Clamp(pitch, -89f, 89f);
		base.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
	}

	private void Update()
	{
		if (Gamepad == null)
		{
			return;
		}
		bool isPressed = Gamepad.rightShoulder.isPressed;
		float num = moveSpeed * (float)((!isPressed) ? 1 : 2);
		CharacterMainControl main = CharacterMainControl.Main;
		Vector2 value = Gamepad.leftStick.value;
		float num2 = Gamepad.rightTrigger.value - Gamepad.leftTrigger.value;
		Vector3 vector = new Vector3(value.x * num, 0f, value.y * num) * Time.unscaledDeltaTime;
		Vector3 obj = (projectMovementOnXZPlane ? Vector3.ProjectOnPlane(base.transform.forward, Vector3.up).normalized : base.transform.forward);
		Vector3 vector2 = (projectMovementOnXZPlane ? Vector3.ProjectOnPlane(base.transform.right, Vector3.up).normalized : base.transform.right);
		Vector3 vector3 = num2 * Vector3.up * num * 0.5f * Time.unscaledDeltaTime;
		Vector3 vector4 = obj * vector.z + vector2 * vector.x + vector3;
		if (!followCharacter || main == null)
		{
			worldPosTarget += vector4;
			base.transform.position = Vector3.SmoothDamp(base.transform.position, worldPosTarget, ref velocityWorldSpace, smoothTime, 20f, 10f * Time.unscaledDeltaTime);
			if (main == null)
			{
				followCharacter = false;
			}
		}
		else
		{
			offsetFromCharacter += vector4;
			base.transform.position = Vector3.SmoothDamp(base.transform.position, main.transform.position + offsetFromCharacter, ref velocityLocalSpace, smoothTime, 20f, 10f * Time.unscaledDeltaTime);
		}
		Vector3 vector5 = Gamepad.rightStick.value * rotateSpeed * vCamera.m_Lens.FieldOfView / 60f;
		yawTarget += vector5.x * Time.unscaledDeltaTime;
		yaw = Mathf.SmoothDamp(yaw, yawTarget, ref yawVelocity, smoothTime, 20f, 10f * Time.unscaledDeltaTime);
		pitchTarget += (0f - vector5.y) * Time.unscaledDeltaTime;
		pitch = Mathf.SmoothDamp(pitch, pitchTarget, ref pitchVelocity, smoothTime, 20f, 10f * Time.unscaledDeltaTime);
		pitch = Mathf.Clamp(pitch, -89f, 89f);
		base.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
		if (Gamepad.buttonNorth.wasPressedThisFrame)
		{
			SnapToMainCamera();
		}
		if (Gamepad.buttonEast.wasPressedThisFrame)
		{
			ToggleFollowTarget();
		}
	}

	private void OnDestroy()
	{
	}

	private void ToggleFollowTarget()
	{
		CharacterMainControl main = CharacterMainControl.Main;
		if (!(main == null))
		{
			followCharacter = !followCharacter;
			if (followCharacter)
			{
				offsetFromCharacter = base.transform.position - main.transform.position;
			}
			worldPosTarget = base.transform.position;
		}
	}

	private void SnapToMainCamera()
	{
		if (GameCamera.Instance == null)
		{
			return;
		}
		Camera renderCamera = GameCamera.Instance.renderCamera;
		if (!(renderCamera == null))
		{
			base.transform.position = renderCamera.transform.position;
			worldPosTarget = renderCamera.transform.position;
			vCamera.m_Lens.FieldOfView = renderCamera.fieldOfView;
			SetRotation(renderCamera.transform.rotation);
			CharacterMainControl main = CharacterMainControl.Main;
			if (main != null && followCharacter)
			{
				offsetFromCharacter = base.transform.position - main.transform.position;
			}
		}
	}
}
