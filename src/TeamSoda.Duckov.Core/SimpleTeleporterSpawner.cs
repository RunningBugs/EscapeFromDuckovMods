using System.Collections.Generic;
using Duckov.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Points))]
public class SimpleTeleporterSpawner : MonoBehaviour
{
	private int scene = -1;

	[SerializeField]
	private int pairCount = 3;

	[SerializeField]
	private SimpleTeleporter simpleTeleporterPfb;

	[SerializeField]
	private Points points;

	private void Start()
	{
		if (points == null)
		{
			points = GetComponent<Points>();
			if (points == null)
			{
				return;
			}
		}
		scene = SceneManager.GetActiveScene().buildIndex;
		if (LevelManager.LevelInited)
		{
			StartCreate();
		}
		else
		{
			LevelManager.OnLevelInitialized += StartCreate;
		}
	}

	private void OnValidate()
	{
		if (points == null)
		{
			points = GetComponent<Points>();
		}
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelInitialized -= StartCreate;
	}

	public void StartCreate()
	{
		scene = SceneManager.GetActiveScene().buildIndex;
		int key = GetKey();
		if (!MultiSceneCore.Instance.inLevelData.TryGetValue(key, out var _))
		{
			MultiSceneCore.Instance.inLevelData.Add(key, true);
			Create();
		}
	}

	private void Create()
	{
		List<Vector3> randomPoints = points.GetRandomPoints(pairCount * 2);
		for (int i = 0; i < pairCount; i++)
		{
			CreateAPair(randomPoints[i * 2], randomPoints[i * 2 + 1]);
		}
	}

	private void CreateAPair(Vector3 point1, Vector3 point2)
	{
		SimpleTeleporter simpleTeleporter = CreateATeleporter(point1);
		SimpleTeleporter simpleTeleporter2 = CreateATeleporter(point2);
		simpleTeleporter.target = simpleTeleporter2.TeleportPoint;
		simpleTeleporter2.target = simpleTeleporter.TeleportPoint;
	}

	private SimpleTeleporter CreateATeleporter(Vector3 point)
	{
		SimpleTeleporter simpleTeleporter = Object.Instantiate(simpleTeleporterPfb);
		MultiSceneCore.MoveToActiveWithScene(simpleTeleporter.gameObject, scene);
		simpleTeleporter.transform.position = point;
		return simpleTeleporter;
	}

	private int GetKey()
	{
		Vector3 vector = base.transform.position * 10f;
		int x = Mathf.RoundToInt(vector.x);
		int y = Mathf.RoundToInt(vector.y);
		int z = Mathf.RoundToInt(vector.z);
		Vector3Int vector3Int = new Vector3Int(x, y, z);
		return $"SimpTeles_{vector3Int}".GetHashCode();
	}
}
