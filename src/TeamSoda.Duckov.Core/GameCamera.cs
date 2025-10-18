using System;
using Cinemachine;
using Cinemachine.PostFX;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameCamera : MonoBehaviour
{
	public enum CameraAimingTypes
	{
		normal,
		bounds
	}

	public Camera renderCamera;

	public CinemachineVirtualCamera mainVCam;

	public CameraArm mianCameraArm;

	public CharacterMainControl target;

	public CinemachineBrain brain;

	public float defaultFOV = 20f;

	public float adsFOV = 15f;

	public Transform mainCamDepthPoint;

	private float currentFov;

	public static Action<GameCamera, CharacterMainControl> OnCameraPosUpdate;

	private Vector3 virtualTarget;

	private float offsetFromTargetX;

	private float offsetFromTargetZ;

	public Transform volumeProxy;

	private DepthOfField dofComponent;

	private float adsValue;

	private bool adsChanging;

	private float defaultAimOffset = 5f;

	private float maxAimOffset = 999f;

	private ItemAgent_Gun gun;

	private InputManager inputManager;

	private Vector3 cameraForwardVector;

	private Vector3 cameraRightVector;

	private float defaultAimOffsetDistanceFactor = 0.5f;

	private float aimOffsetDistanceFactor;

	private CameraAimingTypes cameraAimingType;

	private float lerpSpeed = 12f;

	public static GameCamera Instance => LevelManager.Instance?.GameCamera;

	private bool sickProtectModeOn => DisableCameraOffset.disableCameraOffset;

	private void Start()
	{
		currentFov = mainVCam.m_Lens.FieldOfView;
		if (!dofComponent && (bool)mainVCam)
		{
			VolumeProfile profile = mainVCam.GetComponent<CinemachineVolumeSettings>().m_Profile;
			if ((bool)profile)
			{
				profile.TryGet<DepthOfField>(out dofComponent);
			}
		}
	}

	private void Update()
	{
		if ((bool)target)
		{
			volumeProxy.position = target.transform.position;
			mainCamDepthPoint.position = target.GetCurrentAimPoint();
			gun = target.GetGun();
			if (!inputManager)
			{
				inputManager = LevelManager.Instance.InputManager;
			}
		}
	}

	private void LateUpdate()
	{
		if ((bool)target)
		{
			float num = adsValue;
			if ((bool)gun)
			{
				adsValue = target.AdsValue;
				maxAimOffset = Mathf.Lerp(sickProtectModeOn ? 0f : defaultAimOffset, defaultAimOffset * gun.ADSAimDistanceFactor, adsValue);
				aimOffsetDistanceFactor = Mathf.Lerp(defaultAimOffsetDistanceFactor, defaultAimOffsetDistanceFactor * gun.ADSAimDistanceFactor, adsValue);
			}
			else
			{
				adsValue = Mathf.MoveTowards(adsValue, target.IsInAdsInput ? 1f : 0f, Time.deltaTime * 10f);
				maxAimOffset = Mathf.Lerp(sickProtectModeOn ? 0f : defaultAimOffset, defaultAimOffset * 1.25f, adsValue);
				aimOffsetDistanceFactor = Mathf.Lerp(defaultAimOffsetDistanceFactor, defaultAimOffsetDistanceFactor, adsValue);
			}
			adsChanging = num != adsValue;
			UpdateFov(Time.deltaTime);
			UpdatePosition(Time.deltaTime);
		}
	}

	public void UpdateFov(float deltaTime)
	{
		float num = Mathf.Lerp(defaultFOV, adsFOV, adsValue);
		if (currentFov != num)
		{
			currentFov = num;
			mainVCam.m_Lens.FieldOfView = currentFov;
		}
	}

	public void ForceSyncPos()
	{
		UpdatePosition(1f);
	}

	public void SetTarget(CharacterMainControl _target)
	{
		target = _target;
	}

	private void UpdateCameraVectors()
	{
		cameraForwardVector = renderCamera.transform.forward;
		cameraForwardVector.y = 0f;
		cameraForwardVector.Normalize();
		cameraRightVector = renderCamera.transform.right;
		cameraRightVector.y = 0f;
		cameraRightVector.Normalize();
	}

	public void UpdatePosition(float deltaTime)
	{
		UpdateCameraVectors();
		if (!target)
		{
			return;
		}
		if ((bool)inputManager)
		{
			switch (cameraAimingType)
			{
			case CameraAimingTypes.normal:
				UpdateAimOffsetNormal(deltaTime);
				break;
			case CameraAimingTypes.bounds:
				UpdateAimOffsetUsingBound(deltaTime);
				break;
			}
		}
		Vector3 vector = cameraForwardVector * offsetFromTargetZ + cameraRightVector * offsetFromTargetX;
		virtualTarget = target.transform.position + vector + Vector3.up * 0.5f;
		Vector3.Distance(base.transform.position, virtualTarget);
		_ = 20f;
		base.transform.position = virtualTarget;
		OnCameraPosUpdate?.Invoke(this, target);
		if ((bool)dofComponent)
		{
			dofComponent.focusDistance.value = Vector3.Distance(renderCamera.transform.position, target.transform.position) - 1.5f;
		}
	}

	private void UpdateAimOffsetNormal(float deltaTime)
	{
		float num = Mathf.InverseLerp(20f, 50f, mianCameraArm.pitch);
		lerpSpeed = Mathf.MoveTowards(lerpSpeed, inputManager.TriggerInput ? 1.5f : 12f, Time.deltaTime * 1f);
		lerpSpeed = Mathf.Lerp(1.5f, lerpSpeed, num);
		if (!InputManager.InputActived)
		{
			offsetFromTargetX = Mathf.Lerp(offsetFromTargetX, 0f, Time.unscaledDeltaTime * lerpSpeed);
			offsetFromTargetZ = Mathf.Lerp(offsetFromTargetZ, 0f, Time.unscaledDeltaTime * lerpSpeed);
			return;
		}
		Vector2 mousePos = inputManager.MousePos;
		Vector2 vector = new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
		Vector3 vector2 = ScreenPointToCharacterPlane(vector);
		Vector3 vector3 = ScreenPointToCharacterPlane(new Vector2(vector.x, Screen.height));
		Vector3 vector4 = ScreenPointToCharacterPlane(new Vector2(vector.x, 0f));
		Vector3.Distance(vector3, vector4);
		Vector3 vector5 = ScreenPointToCharacterPlane(mousePos);
		Vector3 vector6 = (vector3 + vector4) * 0.5f;
		vector6 = Vector3.MoveTowards(vector2, vector6, 5f * num);
		float num2 = Vector3.Distance(vector6, vector2);
		Vector3 lhs = Vector3.ClampMagnitude(vector5 - vector6, maxAimOffset) * num;
		float num3 = Vector3.Dot(lhs, cameraRightVector);
		float num4 = Vector3.Dot(lhs, cameraForwardVector);
		offsetFromTargetX = Mathf.Lerp(offsetFromTargetX, Mathf.Clamp(num3 * aimOffsetDistanceFactor, 0f - maxAimOffset, maxAimOffset), deltaTime * lerpSpeed);
		offsetFromTargetZ = Mathf.Lerp(offsetFromTargetZ, Mathf.Clamp(num4 * aimOffsetDistanceFactor, 0f - maxAimOffset, maxAimOffset) - num2, deltaTime * lerpSpeed);
	}

	private void UpdateAimOffsetUsingBound(float deltaTime)
	{
		if (!inputManager.TriggerInput)
		{
			float num = 20f;
			Vector2 mousePos = inputManager.MousePos;
			Vector2 zero = Vector2.zero;
			int num2 = (int)((float)Screen.height * 0.05f);
			if (mousePos.x < (float)num2)
			{
				zero += Vector2.left;
			}
			else if (mousePos.x > (float)(Screen.width - num2))
			{
				zero += Vector2.right;
			}
			if (mousePos.y < (float)num2)
			{
				zero += Vector2.down;
			}
			else if (mousePos.y > (float)(Screen.height - num2))
			{
				zero += Vector2.up;
			}
			if (!InputManager.InputActived)
			{
				zero = Vector2.zero;
			}
			if (!Application.isFocused)
			{
				zero = Vector2.zero;
			}
			if (zero.x != 0f)
			{
				offsetFromTargetX += zero.x * deltaTime * num;
				offsetFromTargetX = Mathf.Clamp(offsetFromTargetX, 0f - maxAimOffset, maxAimOffset);
			}
			if (zero.y != 0f)
			{
				offsetFromTargetZ += zero.y * deltaTime * num;
				offsetFromTargetZ = Mathf.Clamp(offsetFromTargetZ, 0f - maxAimOffset, maxAimOffset);
			}
		}
	}

	private Vector3 ScreenPointToCharacterPlane(Vector3 screenPoint)
	{
		Plane plane = new Plane(Vector3.up, target.transform.position + Vector3.up * 0.5f);
		Ray ray = renderCamera.ScreenPointToRay(screenPoint);
		if (plane.Raycast(ray, out var enter))
		{
			return ray.GetPoint(enter);
		}
		return Vector3.zero;
	}

	public bool IsOffScreen(Vector3 woorldPos)
	{
		Vector3 vector = Camera.main.WorldToScreenPoint(woorldPos);
		if (!(vector.x <= 0f) && !(vector.x >= (float)Screen.width) && !(vector.y <= 0f))
		{
			return vector.y >= (float)Screen.height;
		}
		return true;
	}
}
