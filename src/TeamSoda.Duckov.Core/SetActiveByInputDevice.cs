using UnityEngine;

public class SetActiveByInputDevice : MonoBehaviour
{
	public InputManager.InputDevices activeIfDeviceIs;

	private void Awake()
	{
		OnInputDeviceChanged();
		InputManager.OnInputDeviceChanged += OnInputDeviceChanged;
	}

	private void OnDestroy()
	{
		InputManager.OnInputDeviceChanged -= OnInputDeviceChanged;
	}

	private void OnInputDeviceChanged()
	{
		if (InputManager.InputDevice == activeIfDeviceIs)
		{
			base.gameObject.SetActive(value: true);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
