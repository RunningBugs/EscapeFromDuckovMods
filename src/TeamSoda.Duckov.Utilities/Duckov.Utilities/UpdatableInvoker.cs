using System.Collections.Generic;
using Duckov.Utilities.Updatables;
using UnityEngine;

namespace Duckov.Utilities;

public class UpdatableInvoker : MonoBehaviour
{
	private static UpdatableInvoker instance;

	private static List<object> incomingObjects = new List<object>();

	private static List<object> activeObjects = new List<object>();

	public static UpdatableInvoker Instance
	{
		get
		{
			if (instance == null)
			{
				CreateInstance();
			}
			return instance;
		}
	}

	private static void CreateInstance()
	{
		if (Application.isPlaying)
		{
			GameObject obj = new GameObject("UpdateInvoker");
			instance = obj.AddComponent<UpdatableInvoker>();
			Object.DontDestroyOnLoad(obj);
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
	}

	private void Update()
	{
		if (!(instance != this))
		{
			DoUpdate();
		}
	}

	private static void DoUpdate()
	{
		if (incomingObjects.Count > 0)
		{
			ActivateIncomingObjects();
		}
		bool flag = false;
		for (int i = 0; i < activeObjects.Count; i++)
		{
			object obj = activeObjects[i];
			if (obj == null)
			{
				flag = true;
			}
			else
			{
				(obj as IUpdatable).OnUpdate();
			}
		}
		if (flag)
		{
			activeObjects.RemoveAll((object e) => e == null);
		}
	}

	private static void ActivateIncomingObjects()
	{
		foreach (object incomingObject in incomingObjects)
		{
			activeObjects.Add(incomingObject);
		}
		incomingObjects.Clear();
	}

	public static void Register(IUpdatable updatable)
	{
		incomingObjects.Add(updatable);
		if (Instance == null)
		{
			CreateInstance();
		}
	}

	public static bool Unregister(IUpdatable updatable)
	{
		bool num = incomingObjects.Remove(updatable);
		bool flag = activeObjects.Remove(updatable);
		return num || flag;
	}
}
