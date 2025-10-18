using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Duckov.Scenes;

[ExecuteAlways]
public class SceneLocationsProvider : MonoBehaviour
{
	private static List<SceneLocationsProvider> activeProviders = new List<SceneLocationsProvider>();

	private static ReadOnlyCollection<SceneLocationsProvider> _activeProviders_ReadOnly;

	private StringBuilder sb = new StringBuilder();

	public static ReadOnlyCollection<SceneLocationsProvider> ActiveProviders
	{
		get
		{
			if (_activeProviders_ReadOnly == null)
			{
				_activeProviders_ReadOnly = new ReadOnlyCollection<SceneLocationsProvider>(activeProviders);
			}
			return _activeProviders_ReadOnly;
		}
	}

	public static SceneLocationsProvider GetProviderOfScene(SceneReference sceneReference)
	{
		if (sceneReference == null)
		{
			return null;
		}
		return ActiveProviders.FirstOrDefault((SceneLocationsProvider e) => e != null && e.gameObject.scene.buildIndex == sceneReference.BuildIndex);
	}

	public static SceneLocationsProvider GetProviderOfScene(Scene scene)
	{
		return ActiveProviders.FirstOrDefault((SceneLocationsProvider e) => e != null && e.gameObject.scene.buildIndex == scene.buildIndex);
	}

	internal static SceneLocationsProvider GetProviderOfScene(int sceneBuildIndex)
	{
		return ActiveProviders.FirstOrDefault((SceneLocationsProvider e) => e != null && e.gameObject.scene.buildIndex == sceneBuildIndex);
	}

	public static Transform GetLocation(SceneReference scene, string name)
	{
		if (scene.UnsafeReason != SceneReferenceUnsafeReason.None)
		{
			return null;
		}
		return GetLocation(scene.BuildIndex, name);
	}

	public static Transform GetLocation(int sceneBuildIndex, string name)
	{
		SceneLocationsProvider providerOfScene = GetProviderOfScene(sceneBuildIndex);
		if (providerOfScene == null)
		{
			return null;
		}
		return providerOfScene.GetLocation(name);
	}

	public static Transform GetLocation(string sceneID, string name)
	{
		SceneInfoEntry sceneInfo = SceneInfoCollection.GetSceneInfo(sceneID);
		if (sceneInfo == null)
		{
			return null;
		}
		return GetLocation(sceneInfo.BuildIndex, name);
	}

	private void Awake()
	{
		activeProviders.Add(this);
	}

	private void OnDestroy()
	{
		activeProviders.Remove(this);
	}

	public Transform GetLocation(string path)
	{
		string[] array = path.Split('/');
		Transform transform = base.transform;
		foreach (string text in array)
		{
			if (!string.IsNullOrEmpty(text))
			{
				transform = transform.Find(text);
				if (transform == null)
				{
					return null;
				}
			}
		}
		return transform;
	}

	public bool TryGetPath(Transform value, out string path)
	{
		path = "";
		Transform transform = value;
		List<Transform> list = new List<Transform>();
		while (transform != null && transform != base.transform)
		{
			list.Insert(0, transform);
			transform = transform.parent;
		}
		if (transform != base.transform)
		{
			return false;
		}
		sb.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				sb.Append('/');
			}
			sb.Append(list[i].name);
		}
		path = sb.ToString();
		return true;
	}

	public List<(string path, Vector3 worldPosition, GameObject gameObject)> GetAllPathsAndItsPosition()
	{
		List<(string, Vector3, GameObject)> list = new List<(string, Vector3, GameObject)>();
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(base.transform);
		while (stack.Count > 0)
		{
			Transform transform = stack.Pop();
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (TryGetPath(child, out var path))
				{
					list.Add((path, child.transform.position, child.gameObject));
					stack.Push(child);
				}
			}
		}
		return list;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.childCount == 0)
			{
				Gizmos.DrawSphere(transform.position, 1.5f);
			}
		}
	}
}
