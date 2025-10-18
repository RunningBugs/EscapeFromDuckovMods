using System.Collections.Generic;
using Duckov.Utilities;
using Eflatun.SceneReference;
using UnityEngine;

[CreateAssetMenu]
public class SceneInfoCollection : ScriptableObject
{
	public const string BaseSceneID = "Base";

	[SerializeField]
	private List<SceneInfoEntry> entries;

	internal static SceneInfoCollection Instance => GameplayDataSettings.SceneManagement?.SceneInfoCollection;

	public static List<SceneInfoEntry> Entries
	{
		get
		{
			if (Instance == null)
			{
				return null;
			}
			return Instance.entries;
		}
	}

	public SceneInfoEntry InstanceGetSceneInfo(string id)
	{
		return entries.Find((SceneInfoEntry e) => e.ID == id);
	}

	public string InstanceGetSceneID(int buildIndex)
	{
		return entries.Find(delegate(SceneInfoEntry e)
		{
			if (e == null)
			{
				return false;
			}
			return e.SceneReference.UnsafeReason == SceneReferenceUnsafeReason.None && e.SceneReference.BuildIndex == buildIndex;
		})?.ID;
	}

	internal string InstanceGetSceneID(SceneReference sceneRef)
	{
		if (sceneRef.UnsafeReason != SceneReferenceUnsafeReason.None)
		{
			return null;
		}
		return InstanceGetSceneID(sceneRef.BuildIndex);
	}

	internal SceneReference InstanceGetSceneReferencce(string requireSceneID)
	{
		return InstanceGetSceneInfo(requireSceneID)?.SceneReference;
	}

	public static SceneInfoEntry GetSceneInfo(string sceneID)
	{
		if (Instance == null)
		{
			return null;
		}
		return Instance.InstanceGetSceneInfo(sceneID);
	}

	public static string GetSceneID(SceneReference sceneRef)
	{
		if (Instance == null)
		{
			return null;
		}
		return Instance.InstanceGetSceneID(sceneRef);
	}

	public static string GetSceneID(int buildIndex)
	{
		if (Instance == null)
		{
			return null;
		}
		return Instance.InstanceGetSceneID(buildIndex);
	}

	internal static int GetBuildIndex(string overrideSceneID)
	{
		if (Instance == null)
		{
			return -1;
		}
		return Instance.InstanceGetSceneInfo(overrideSceneID)?.BuildIndex ?? (-1);
	}

	internal static SceneInfoEntry GetSceneInfo(int sceneBuildIndex)
	{
		if (Instance == null)
		{
			return null;
		}
		return Instance.entries.Find((SceneInfoEntry e) => e.BuildIndex == sceneBuildIndex);
	}
}
