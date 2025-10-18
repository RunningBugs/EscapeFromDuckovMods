using System;
using Duckov.Scenes;
using UnityEngine;

namespace Duckov;

public class RichPresenceManager : MonoBehaviour
{
	public bool isMainMenu = true;

	public bool isInLevel;

	public string levelDisplayNameRaw;

	public static Action<RichPresenceManager> OnInstanceChanged;

	public bool isPlaying => !isMainMenu;

	private void InvokeChangeEvent()
	{
		OnInstanceChanged?.Invoke(this);
	}

	private void Awake()
	{
		MainMenu.OnMainMenuAwake = (Action)Delegate.Combine(MainMenu.OnMainMenuAwake, new Action(OnMainMenuAwake));
		MainMenu.OnMainMenuDestroy = (Action)Delegate.Combine(MainMenu.OnMainMenuDestroy, new Action(OnMainMenuDestroy));
		MultiSceneCore.OnInstanceAwake += OnMultiSceneCoreInstanceAwake;
		MultiSceneCore.OnInstanceDestroy += OnMultiSceneCoreInstanceDestroy;
	}

	private void OnDestroy()
	{
		MainMenu.OnMainMenuAwake = (Action)Delegate.Remove(MainMenu.OnMainMenuAwake, new Action(OnMainMenuAwake));
		MainMenu.OnMainMenuDestroy = (Action)Delegate.Remove(MainMenu.OnMainMenuDestroy, new Action(OnMainMenuDestroy));
		MultiSceneCore.OnInstanceAwake -= OnMultiSceneCoreInstanceAwake;
		MultiSceneCore.OnInstanceDestroy -= OnMultiSceneCoreInstanceDestroy;
	}

	private void OnMainMenuAwake()
	{
		isMainMenu = true;
		InvokeChangeEvent();
	}

	private void OnMainMenuDestroy()
	{
		isMainMenu = false;
		InvokeChangeEvent();
	}

	private void OnMultiSceneCoreInstanceAwake(MultiSceneCore core)
	{
		levelDisplayNameRaw = core.DisplaynameRaw;
		isInLevel = true;
		InvokeChangeEvent();
	}

	private void OnMultiSceneCoreInstanceDestroy(MultiSceneCore core)
	{
		isInLevel = false;
		InvokeChangeEvent();
	}

	internal string GetSteamDisplay()
	{
		if (Application.isEditor)
		{
			return "#Status_UnityEditor";
		}
		if (!isMainMenu)
		{
			return "#Status_Playing";
		}
		return "#Status_MainMenu";
	}
}
