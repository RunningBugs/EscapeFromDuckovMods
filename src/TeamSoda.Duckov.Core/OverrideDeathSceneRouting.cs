using Duckov.Scenes;
using UnityEngine;

public class OverrideDeathSceneRouting : MonoBehaviour
{
	[SceneID]
	[SerializeField]
	private string sceneID;

	public static OverrideDeathSceneRouting Instance { get; private set; }

	private void OnEnable()
	{
		if (Instance != null)
		{
			Debug.LogError("存在多个OverrideDeathSceneRouting实例");
		}
		Instance = this;
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public string GetSceneID()
	{
		return sceneID;
	}
}
