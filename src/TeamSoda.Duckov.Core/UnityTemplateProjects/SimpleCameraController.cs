using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityTemplateProjects;

public class SimpleCameraController : MonoBehaviour
{
	private class CameraState
	{
		public float yaw;

		public float pitch;

		public float roll;

		public float x;

		public float y;

		public float z;

		public void SetFromTransform(Transform t)
		{
			pitch = t.eulerAngles.x;
			yaw = t.eulerAngles.y;
			roll = t.eulerAngles.z;
			x = t.position.x;
			y = t.position.y;
			z = t.position.z;
		}

		public void Translate(Vector3 translation)
		{
			Vector3 vector = Quaternion.Euler(pitch, yaw, roll) * translation;
			x += vector.x;
			y += vector.y;
			z += vector.z;
		}

		public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
		{
			yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
			pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
			roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);
			x = Mathf.Lerp(x, target.x, positionLerpPct);
			y = Mathf.Lerp(y, target.y, positionLerpPct);
			z = Mathf.Lerp(z, target.z, positionLerpPct);
		}

		public void UpdateTransform(Transform t)
		{
			t.eulerAngles = new Vector3(pitch, yaw, roll);
			t.position = new Vector3(x, y, z);
		}
	}

	private CameraState m_TargetCameraState = new CameraState();

	private CameraState m_InterpolatingCameraState = new CameraState();

	[Header("Movement Settings")]
	[Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
	public float boost = 3.5f;

	[Tooltip("Time it takes to interpolate camera position 99% of the way to the target.")]
	[Range(0.001f, 1f)]
	public float positionLerpTime = 0.2f;

	[Header("Rotation Settings")]
	[Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
	public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

	[Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target.")]
	[Range(0.001f, 1f)]
	public float rotationLerpTime = 0.01f;

	[Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
	public bool invertY;

	private InputAction movementAction;

	private InputAction verticalMovementAction;

	private InputAction lookAction;

	private InputAction boostFactorAction;

	private bool mouseRightButtonPressed;

	private void Start()
	{
		InputActionMap map = new InputActionMap("Simple Camera Controller");
		lookAction = map.AddAction("look", InputActionType.Value, "<Mouse>/delta");
		movementAction = map.AddAction("move", InputActionType.Value, "<Gamepad>/leftStick");
		verticalMovementAction = map.AddAction("Vertical Movement");
		boostFactorAction = map.AddAction("Boost Factor", InputActionType.Value, "<Mouse>/scroll");
		lookAction.AddBinding("<Gamepad>/rightStick").WithProcessor("scaleVector2(x=15, y=15)");
		movementAction.AddCompositeBinding("Dpad").With("Up", "<Keyboard>/w").With("Up", "<Keyboard>/upArrow")
			.With("Down", "<Keyboard>/s")
			.With("Down", "<Keyboard>/downArrow")
			.With("Left", "<Keyboard>/a")
			.With("Left", "<Keyboard>/leftArrow")
			.With("Right", "<Keyboard>/d")
			.With("Right", "<Keyboard>/rightArrow");
		verticalMovementAction.AddCompositeBinding("Dpad").With("Up", "<Keyboard>/pageUp").With("Down", "<Keyboard>/pageDown")
			.With("Up", "<Keyboard>/e")
			.With("Down", "<Keyboard>/q")
			.With("Up", "<Gamepad>/rightshoulder")
			.With("Down", "<Gamepad>/leftshoulder");
		boostFactorAction.AddBinding("<Gamepad>/Dpad").WithProcessor("scaleVector2(x=1, y=4)");
		movementAction.Enable();
		lookAction.Enable();
		verticalMovementAction.Enable();
		boostFactorAction.Enable();
	}

	private void OnEnable()
	{
		m_TargetCameraState.SetFromTransform(base.transform);
		m_InterpolatingCameraState.SetFromTransform(base.transform);
	}

	private Vector3 GetInputTranslationDirection()
	{
		Vector3 zero = Vector3.zero;
		Vector2 vector = movementAction.ReadValue<Vector2>();
		zero.x = vector.x;
		zero.z = vector.y;
		zero.y = verticalMovementAction.ReadValue<Vector2>().y;
		return zero;
	}

	private void Update()
	{
		if (IsEscapePressed())
		{
			Application.Quit();
		}
		if (IsRightMouseButtonDown())
		{
			Cursor.lockState = CursorLockMode.Locked;
		}
		if (IsRightMouseButtonUp())
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}
		if (IsCameraRotationAllowed())
		{
			Vector2 vector = GetInputLookRotation() * Time.deltaTime * 5f;
			if (invertY)
			{
				vector.y = 0f - vector.y;
			}
			float num = mouseSensitivityCurve.Evaluate(vector.magnitude);
			m_TargetCameraState.yaw += vector.x * num;
			m_TargetCameraState.pitch += vector.y * num;
		}
		Vector3 translation = GetInputTranslationDirection() * Time.deltaTime;
		if (IsBoostPressed())
		{
			translation *= 10f;
		}
		boost += GetBoostFactor();
		translation *= Mathf.Pow(2f, boost);
		m_TargetCameraState.Translate(translation);
		float positionLerpPct = 1f - Mathf.Exp(Mathf.Log(0.00999999f) / positionLerpTime * Time.deltaTime);
		float rotationLerpPct = 1f - Mathf.Exp(Mathf.Log(0.00999999f) / rotationLerpTime * Time.deltaTime);
		m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);
		m_InterpolatingCameraState.UpdateTransform(base.transform);
	}

	private float GetBoostFactor()
	{
		return boostFactorAction.ReadValue<Vector2>().y * 0.01f;
	}

	private Vector2 GetInputLookRotation()
	{
		return lookAction.ReadValue<Vector2>();
	}

	private bool IsBoostPressed()
	{
		return (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) | (Gamepad.current != null && Gamepad.current.xButton.isPressed);
	}

	private bool IsEscapePressed()
	{
		if (Keyboard.current == null)
		{
			return false;
		}
		return Keyboard.current.escapeKey.isPressed;
	}

	private bool IsCameraRotationAllowed()
	{
		return (Mouse.current != null && Mouse.current.rightButton.isPressed) | (Gamepad.current != null && Gamepad.current.rightStick.ReadValue().magnitude > 0f);
	}

	private bool IsRightMouseButtonDown()
	{
		if (Mouse.current == null)
		{
			return false;
		}
		return Mouse.current.rightButton.isPressed;
	}

	private bool IsRightMouseButtonUp()
	{
		if (Mouse.current == null)
		{
			return false;
		}
		return !Mouse.current.rightButton.isPressed;
	}
}
