using System.Collections.Generic;
using System.Linq;
using Eflatun.SceneReference;
using UnityEngine;

namespace Duckov.MiniMaps;

public class MiniMapCenter : MonoBehaviour
{
	private static List<MiniMapCenter> activeMiniMapCenters = new List<MiniMapCenter>();

	[SerializeField]
	private float worldSize = -1f;

	public float WorldSize => worldSize;

	private void OnEnable()
	{
		activeMiniMapCenters.Add(this);
		if (activeMiniMapCenters.Count > 1)
		{
			if ((bool)activeMiniMapCenters.Find((MiniMapCenter e) => e != null && e != this && e.gameObject.scene.buildIndex == base.gameObject.scene.buildIndex))
			{
				Debug.LogError("场景 " + base.gameObject.scene.name + " 似乎存在两个MiniMapCenter！");
			}
		}
		else
		{
			CacheThisCenter();
		}
	}

	private void CacheThisCenter()
	{
		MiniMapSettings instance = MiniMapSettings.Instance;
		if (!(instance == null))
		{
			_ = base.transform.position;
			instance.Cache(this);
		}
	}

	private void OnDisable()
	{
		activeMiniMapCenters.Remove(this);
	}

	internal static Vector3 GetCenterOfObjectScene(MonoBehaviour target)
	{
		int sceneBuildIndex = target.gameObject.scene.buildIndex;
		if (target is IPointOfInterest { OverrideScene: >=0 } pointOfInterest)
		{
			sceneBuildIndex = pointOfInterest.OverrideScene;
		}
		return GetCenter(sceneBuildIndex);
	}

	internal static string GetSceneID(MonoBehaviour target)
	{
		int sceneBuildIndex = target.gameObject.scene.buildIndex;
		if (target is IPointOfInterest { OverrideScene: >=0 } pointOfInterest)
		{
			sceneBuildIndex = pointOfInterest.OverrideScene;
		}
		MiniMapSettings instance = MiniMapSettings.Instance;
		if (instance == null)
		{
			return null;
		}
		return instance.maps.Find((MiniMapSettings.MapEntry e) => e.SceneReference.UnsafeReason == SceneReferenceUnsafeReason.None && e.SceneReference.BuildIndex == sceneBuildIndex)?.sceneID;
	}

	internal static Vector3 GetCenter(int sceneBuildIndex)
	{
		MiniMapSettings instance = MiniMapSettings.Instance;
		if (instance == null)
		{
			return Vector3.zero;
		}
		return instance.maps.FirstOrDefault((MiniMapSettings.MapEntry e) => e.SceneReference.UnsafeReason == SceneReferenceUnsafeReason.None && e.SceneReference.BuildIndex == sceneBuildIndex)?.mapWorldCenter ?? instance.combinedCenter;
	}

	internal static Vector3 GetCenter(string sceneID)
	{
		return GetCenter(SceneInfoCollection.GetBuildIndex(sceneID));
	}

	internal static Vector3 GetCombinedCenter()
	{
		MiniMapSettings instance = MiniMapSettings.Instance;
		if (instance == null)
		{
			return Vector3.zero;
		}
		return instance.combinedCenter;
	}

	private void OnDrawGizmosSelected()
	{
		if (!(WorldSize < 0f))
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(WorldSize, 1f, WorldSize));
		}
	}
}
