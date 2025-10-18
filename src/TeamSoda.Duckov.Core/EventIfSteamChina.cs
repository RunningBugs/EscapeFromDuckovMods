using Steamworks;
using UnityEngine;
using UnityEngine.Events;

public class EventIfSteamChina : MonoBehaviour
{
	public UnityEvent onStart_IsSteamChina;

	public UnityEvent onStart_IsNotSteamChina;

	private void Start()
	{
		if (SteamManager.Initialized)
		{
			if (SteamUtils.IsSteamChinaLauncher())
			{
				onStart_IsSteamChina.Invoke();
			}
			else
			{
				onStart_IsNotSteamChina.Invoke();
			}
		}
	}
}
