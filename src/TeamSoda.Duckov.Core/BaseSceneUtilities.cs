using Saves;
using UnityEngine;

public class BaseSceneUtilities : MonoBehaviour
{
	[SerializeField]
	private float saveInterval = 5f;

	private float lastTimeSaved = float.MinValue;

	private float TimeSinceLastSave => Time.realtimeSinceStartup - lastTimeSaved;

	private void Save()
	{
		LevelManager.Instance.SaveMainCharacter();
		SavesSystem.CollectSaveData();
		SavesSystem.SaveFile();
		lastTimeSaved = Time.realtimeSinceStartup;
	}

	private void Awake()
	{
		lastTimeSaved = Time.realtimeSinceStartup;
	}

	private void Update()
	{
		if (LevelManager.LevelInited && TimeSinceLastSave > saveInterval)
		{
			Save();
		}
	}
}
