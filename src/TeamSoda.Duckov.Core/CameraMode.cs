using System;
using Duckov.UI;
using UnityEngine;

public class CameraMode : MonoBehaviour
{
	public static Action OnCameraModeActivated;

	public static Action OnCameraModeDeactivated;

	public static Action<bool> OnCameraModeChanged;

	private bool active;

	public static CameraMode Instance { get; private set; }

	public static bool Active
	{
		get
		{
			if (Instance == null)
			{
				return false;
			}
			return Instance.active;
		}
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("检测到多个Camera Mode", base.gameObject);
			return;
		}
		Shader.SetGlobalFloat("CameraModeOn", 0f);
		Instance = this;
		UIInputManager.OnToggleCameraMode += OnToggleCameraMode;
		UIInputManager.OnCancel += OnUICancel;
		ManagedUIElement.onOpen += OnViewOpen;
	}

	private void OnDestroy()
	{
		Shader.SetGlobalFloat("CameraModeOn", 0f);
		UIInputManager.OnToggleCameraMode -= OnToggleCameraMode;
		UIInputManager.OnCancel -= OnUICancel;
		ManagedUIElement.onOpen -= OnViewOpen;
		Shader.SetGlobalFloat("CameraModeOn", 0f);
	}

	private void OnViewOpen(ManagedUIElement element)
	{
		if (Active)
		{
			Deactivate();
		}
	}

	private void OnUICancel(UIInputEventData data)
	{
		if (!data.Used && Active)
		{
			Deactivate();
			data.Use();
		}
	}

	private void OnToggleCameraMode(UIInputEventData data)
	{
		if (Active)
		{
			Deactivate();
		}
		else
		{
			Activate();
		}
		data.Use();
	}

	private void MActivate()
	{
		if (!(View.ActiveView != null))
		{
			active = true;
			Shader.SetGlobalFloat("CameraModeOn", 1f);
			OnCameraModeActivated?.Invoke();
			OnCameraModeChanged?.Invoke(active);
		}
	}

	private void MDeactivate()
	{
		active = false;
		Shader.SetGlobalFloat("CameraModeOn", 0f);
		OnCameraModeDeactivated?.Invoke();
		OnCameraModeChanged?.Invoke(active);
	}

	public static void Activate()
	{
		if (!(Instance == null))
		{
			Shader.SetGlobalFloat("CameraModeOn", 1f);
			Instance.MActivate();
		}
	}

	public static void Deactivate()
	{
		Shader.SetGlobalFloat("CameraModeOn", 0f);
		if (!(Instance == null))
		{
			Instance.MDeactivate();
		}
	}
}
