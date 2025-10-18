using System;
using System.IO;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CameraModeController : MonoBehaviour
{
	public CinemachineVirtualCamera vCam;

	private bool actived;

	public Transform dofTarget;

	private Vector3 dofTargetPoint;

	public InputActionAsset inputActionAsset;

	public LayerMask dofLayerMask;

	private Vector2 moveInput;

	private float upDownInput;

	private bool focusInput;

	private bool captureInput;

	private bool fastInput;

	private bool openFolderInput;

	public GameObject focusMesh;

	public float focusMeshSize = 0.3f;

	private float focusMeshCurrentSize = 0.3f;

	public float focusMeshAppearTime = 1f;

	private float focusMeshTimer = 0.3f;

	private float fovInput;

	private Vector2 aimInput;

	public float moveSpeed;

	public float fastMoveSpeed;

	public float aimSpeed;

	private float yaw;

	private float pitch;

	private bool shootting;

	public ColorPunch colorPunch;

	public Vector2 fovRange = new Vector2(5f, 60f);

	[Range(0.01f, 0.5f)]
	public float fovChangeSpeed = 10f;

	public CanvasGroup indicatorGroup;

	public UnityEvent OnCapturedEvent;

	private static string filePath
	{
		get
		{
			if (GameMetaData.Instance.Platform == Platform.WeGame)
			{
				return Application.streamingAssetsPath + "/ScreenShots";
			}
			return Application.persistentDataPath + "/ScreenShots";
		}
	}

	private void UpdateInput()
	{
		moveInput = inputActionAsset["CameraModeMove"].ReadValue<Vector2>();
		focusInput = inputActionAsset["CameraModeFocus"].IsPressed();
		upDownInput = inputActionAsset["CameraModeUpDown"].ReadValue<float>();
		fovInput = inputActionAsset["CameraModeFOV"].ReadValue<float>();
		aimInput = inputActionAsset["CameraModeAim"].ReadValue<Vector2>();
		captureInput = inputActionAsset["CameraModeCapture"].WasPressedThisFrame();
		fastInput = inputActionAsset["CameraModeFaster"].IsPressed();
		openFolderInput = inputActionAsset["CameraModeOpenFolder"].WasPressedThisFrame();
	}

	private void Awake()
	{
		CameraMode.OnCameraModeActivated = (Action)Delegate.Combine(CameraMode.OnCameraModeActivated, new Action(OnCameraModeActivated));
		CameraMode.OnCameraModeDeactivated = (Action)Delegate.Combine(CameraMode.OnCameraModeDeactivated, new Action(OnCameraModeDeactivated));
		inputActionAsset.Enable();
		vCam.gameObject.SetActive(value: true);
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (!actived)
		{
			return;
		}
		UpdateInput();
		if (!shootting)
		{
			UpdateMove();
			UpdateLook();
			UpdateFov();
			if (captureInput)
			{
				Shot().Forget();
			}
			if (openFolderInput)
			{
				OpenFolder();
				openFolderInput = false;
			}
		}
	}

	private void LateUpdate()
	{
		UpdateFocus();
	}

	private void UpdateMove()
	{
		Vector3 forward = vCam.transform.forward;
		forward.y = 0f;
		forward.Normalize();
		Vector3 right = vCam.transform.right;
		right.y = 0f;
		right.Normalize();
		Vector3 vector = right * moveInput.x + forward * moveInput.y;
		vector.Normalize();
		vector += upDownInput * Vector3.up;
		vCam.transform.position += Time.unscaledDeltaTime * vector * (fastInput ? fastMoveSpeed : moveSpeed);
	}

	private void UpdateLook()
	{
		pitch += (0f - aimInput.y) * aimSpeed * Time.unscaledDeltaTime;
		pitch = Mathf.Clamp(pitch, -89.9f, 89.9f);
		yaw += aimInput.x * aimSpeed * Time.unscaledDeltaTime;
		vCam.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
	}

	private void UpdateFocus()
	{
		if (focusInput)
		{
			if (Physics.Raycast(vCam.transform.position, vCam.transform.forward, out var hitInfo, 100f, dofLayerMask))
			{
				dofTargetPoint = hitInfo.point + vCam.transform.forward * -0.2f;
				dofTarget.position = dofTargetPoint;
			}
			focusMeshTimer = focusMeshAppearTime;
			if (!focusMesh.gameObject.activeSelf)
			{
				focusMesh.gameObject.SetActive(value: true);
			}
		}
		else if (focusMeshTimer > 0f)
		{
			focusMeshTimer -= Time.unscaledDeltaTime;
			if (focusMeshTimer <= 0f)
			{
				focusMeshTimer = 0f;
				focusMesh.gameObject.SetActive(value: false);
			}
		}
		if (focusMesh.gameObject.activeSelf)
		{
			focusMesh.transform.localScale = Vector3.one * focusMeshSize * focusMeshTimer / focusMeshAppearTime;
		}
	}

	private void UpdateFov()
	{
		float fieldOfView = vCam.m_Lens.FieldOfView;
		fieldOfView += (0f - fovChangeSpeed) * fovInput;
		fieldOfView = Mathf.Clamp(fieldOfView, fovRange.x, fovRange.y);
		vCam.m_Lens.FieldOfView = fieldOfView;
	}

	private void OnDestroy()
	{
		CameraMode.OnCameraModeActivated = (Action)Delegate.Remove(CameraMode.OnCameraModeActivated, new Action(OnCameraModeActivated));
		CameraMode.OnCameraModeDeactivated = (Action)Delegate.Remove(CameraMode.OnCameraModeDeactivated, new Action(OnCameraModeDeactivated));
	}

	private void OnCameraModeActivated()
	{
		GameCamera instance = GameCamera.Instance;
		if (instance != null)
		{
			CameraArm mianCameraArm = instance.mianCameraArm;
			yaw = mianCameraArm.yaw;
			pitch = mianCameraArm.pitch;
			vCam.transform.position = instance.renderCamera.transform.position;
			dofTargetPoint = instance.target.transform.position;
			actived = true;
			vCam.m_Lens.FieldOfView = instance.renderCamera.fieldOfView;
			base.gameObject.SetActive(value: true);
		}
	}

	public static void OpenFolder()
	{
		GUIUtility.systemCopyBuffer = filePath;
		NotificationText.Push(filePath ?? "");
	}

	private void OnCameraModeDeactivated()
	{
		actived = false;
		base.gameObject.SetActive(value: false);
	}

	private async UniTaskVoid Shot()
	{
		if (!shootting)
		{
			indicatorGroup.alpha = 0f;
			await UniTask.WaitForEndOfFrame(this);
			shootting = true;
			int num = 0;
			_ = Screen.currentResolution.height;
			_ = 1440;
			if (PlayerPrefs.HasKey("ScreenShotIndex"))
			{
				num = PlayerPrefs.GetInt("ScreenShotIndex");
			}
			if (!Directory.Exists(filePath))
			{
				Directory.CreateDirectory(filePath);
			}
			ScreenCapture.CaptureScreenshot($"{filePath}/ScreenShot_{num:0000}.png", 2);
			num++;
			PlayerPrefs.SetInt("ScreenShotIndex", num);
			await UniTask.WaitForEndOfFrame(this);
			await UniTask.WaitForEndOfFrame(this);
			await UniTask.WaitForEndOfFrame(this);
			indicatorGroup.alpha = 1f;
			colorPunch.Punch();
			OnCapturedEvent?.Invoke();
			await UniTask.WaitForSeconds(0.3f, ignoreTimeScale: true);
			shootting = false;
		}
	}
}
