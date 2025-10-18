using Duckov.Scenes;
using UnityEngine;

public class LevelManagerProxy : MonoBehaviour
{
	public void NotifyEvacuated()
	{
		LevelManager.Instance?.NotifyEvacuated(new EvacuationInfo(MultiSceneCore.ActiveSubSceneID, base.transform.position));
	}
}
