using Saves;
using UnityEngine;

public class SetDoorOpenIfSaveData : MonoBehaviour
{
	public Door door;

	public string key;

	public bool openIfDataTure = true;

	private void Start()
	{
		if (LevelManager.LevelInited)
		{
			OnSet();
		}
		else
		{
			LevelManager.OnLevelInitialized += OnSet;
		}
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelInitialized -= OnSet;
	}

	private void OnSet()
	{
		bool flag = SavesSystem.Load<bool>(key);
		Debug.Log($"Load door data:{key}  {flag}");
		door.ForceSetClosed(flag != openIfDataTure, triggerEvent: false);
	}
}
