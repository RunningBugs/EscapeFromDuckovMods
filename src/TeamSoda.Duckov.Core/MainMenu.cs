using System;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	public static Action OnMainMenuAwake;

	public static Action OnMainMenuDestroy;

	private void Awake()
	{
		OnMainMenuAwake?.Invoke();
	}

	private void OnDestroy()
	{
		OnMainMenuDestroy?.Invoke();
	}
}
