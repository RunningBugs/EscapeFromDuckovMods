using System.Collections.Generic;
using Duckov.Modding;
using Duckov.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterSortingMod;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
	private readonly List<InventorySortController> _controllers = new List<InventorySortController>();
	private bool _pendingScan;

	private void Awake()
	{
		Debug.Log("BetterSortingMod loaded");
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		View.OnActiveViewChanged += OnActiveViewChanged;
		QueueScan();
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		View.OnActiveViewChanged -= OnActiveViewChanged;
		CleanupMissingControllers();
	}

	private void Update()
	{
		if (!_pendingScan)
		{
			return;
		}
		_pendingScan = false;
		AttachControllers();
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		QueueScan();
	}

	private void OnActiveViewChanged()
	{
		QueueScan();
	}

	private void QueueScan()
	{
		_pendingScan = true;
	}

	private void AttachControllers()
	{
		CleanupMissingControllers();
		InventoryDisplay[] displays;
		try
		{
			displays = Object.FindObjectsByType<InventoryDisplay>(FindObjectsSortMode.InstanceID);
		}
		catch
		{
			displays = Object.FindObjectsOfType<InventoryDisplay>(includeInactive: true);
		}
		foreach (InventoryDisplay display in displays)
		{
			if (display == null)
			{
				continue;
			}
			if (display.GetComponent<InventorySortController>() != null)
			{
				continue;
			}
			InventorySortController controller = display.gameObject.AddComponent<InventorySortController>();
			controller.Initialize(display);
			_controllers.Add(controller);
		}
	}

	private void CleanupMissingControllers()
	{
		for (int i = _controllers.Count - 1; i >= 0; i--)
		{
			InventorySortController controller = _controllers[i];
			if (controller == null || controller.TargetDisplay == null)
			{
				_controllers.RemoveAt(i);
			}
		}
	}
}
