using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Duckov.Utilities;

public class SetActiveByPlayerDistance : MonoBehaviour
{
	private static Dictionary<int, List<GameObject>> listsOfScenes = new Dictionary<int, List<GameObject>>();

	[SerializeField]
	private float distance = 100f;

	private Scene cachedActiveScene;

	private List<GameObject> cachedListRef;

	private Transform cachedPlayerTransform;

	public static SetActiveByPlayerDistance Instance { get; private set; }

	public float Distance => distance;

	private Transform PlayerTransform
	{
		get
		{
			if (!cachedPlayerTransform)
			{
				cachedPlayerTransform = CharacterMainControl.Main?.transform;
			}
			return cachedPlayerTransform;
		}
	}

	private static List<GameObject> GetListByScene(int sceneBuildIndex, bool createIfNotExist = true)
	{
		if (listsOfScenes.TryGetValue(sceneBuildIndex, out var value))
		{
			return value;
		}
		if (createIfNotExist)
		{
			List<GameObject> list = new List<GameObject>();
			listsOfScenes[sceneBuildIndex] = list;
			return list;
		}
		return null;
	}

	private static List<GameObject> GetListByScene(Scene scene, bool createIfNotExist = true)
	{
		return GetListByScene(scene.buildIndex, createIfNotExist);
	}

	public static void Register(GameObject gameObject, int sceneBuildIndex)
	{
		GetListByScene(sceneBuildIndex).Add(gameObject);
	}

	public static bool Unregister(GameObject gameObject, int sceneBuildIndex)
	{
		return GetListByScene(sceneBuildIndex, createIfNotExist: false)?.Remove(gameObject) ?? false;
	}

	public static void Register(GameObject gameObject, Scene scene)
	{
		Register(gameObject, scene.buildIndex);
	}

	public static void Unregister(GameObject gameObject, Scene scene)
	{
		Unregister(gameObject, scene.buildIndex);
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		CleanUp();
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
		cachedActiveScene = SceneManager.GetActiveScene();
		RefreshCache();
	}

	private void CleanUp()
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, List<GameObject>> listsOfScene in listsOfScenes)
		{
			List<GameObject> value = listsOfScene.Value;
			value.RemoveAll((GameObject e) => e == null);
			if (value == null || value.Count < 1)
			{
				list.Add(listsOfScene.Key);
			}
		}
		foreach (int item in list)
		{
			listsOfScenes.Remove(item);
		}
	}

	private void OnActiveSceneChanged(Scene prev, Scene cur)
	{
		RefreshCache();
	}

	private void RefreshCache()
	{
		cachedActiveScene = SceneManager.GetActiveScene();
		cachedListRef = GetListByScene(cachedActiveScene);
	}

	private void FixedUpdate()
	{
		if (PlayerTransform == null || cachedListRef == null)
		{
			return;
		}
		foreach (GameObject item in cachedListRef)
		{
			if (!(item == null))
			{
				bool active = (PlayerTransform.position - item.transform.position).sqrMagnitude < distance * distance;
				item.gameObject.SetActive(active);
			}
		}
	}

	private void DebugRegister(GameObject go)
	{
		Register(go, go.gameObject.scene);
	}
}
