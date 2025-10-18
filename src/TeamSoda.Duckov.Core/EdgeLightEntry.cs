using System;
using UnityEngine;

public class EdgeLightEntry : MonoBehaviour
{
	private static bool settingEnabled = true;

	private static event Action OnSettingChangedEvent;

	public static void SetEnabled(bool enabled)
	{
		settingEnabled = enabled;
		EdgeLightEntry.OnSettingChangedEvent?.Invoke();
	}

	private void Awake()
	{
		OnSettingChangedEvent += OnSettingChanged;
		base.gameObject.SetActive(settingEnabled);
	}

	private void OnDestroy()
	{
		OnSettingChangedEvent -= OnSettingChanged;
	}

	private void OnSettingChanged()
	{
		base.gameObject.SetActive(settingEnabled);
	}
}
