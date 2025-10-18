using System.Collections.Generic;
using Duckov.MiniMaps;
using Duckov.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitCreator : MonoBehaviour
{
	public GameObject exitPrefab;

	[LocalizationKey("Default")]
	public string exitNameKey;

	[SerializeField]
	private Sprite icon;

	[SerializeField]
	private Color iconColor = Color.white;

	[SerializeField]
	private Color shadowColor = Color.white;

	[SerializeField]
	private float shadowDistance;

	private int minExitCount => LevelConfig.MinExitCount;

	private int maxExitCount => LevelConfig.MaxExitCount;

	public void Spawn()
	{
		int num = Random.Range(minExitCount, maxExitCount + 1);
		if (MultiSceneCore.Instance == null)
		{
			return;
		}
		List<(string, SubSceneEntry.Location)> list = new List<(string, SubSceneEntry.Location)>();
		foreach (SubSceneEntry subScene in MultiSceneCore.Instance.SubScenes)
		{
			foreach (SubSceneEntry.Location cachedLocation in subScene.cachedLocations)
			{
				if (IsPathCompitable(cachedLocation))
				{
					list.Add((subScene.sceneID, cachedLocation));
				}
			}
		}
		list.Sort(compareExit);
		if (num > list.Count)
		{
			num = list.Count;
		}
		MiniMapSettings.TryGetMinimapPosition(LevelManager.Instance.MainCharacter.transform.position, out var _);
		int num2 = Mathf.RoundToInt((float)list.Count * 0.8f);
		if (num > num2)
		{
			num2 = num;
		}
		for (int i = 0; i < num; i++)
		{
			int index = Random.Range(0, num2);
			num2--;
			(string, SubSceneEntry.Location) tuple = list[index];
			list.RemoveAt(index);
			SceneInfoEntry sceneInfo = SceneInfoCollection.GetSceneInfo(tuple.Item1);
			CreateExit(tuple.Item2.position, sceneInfo.BuildIndex, i);
		}
	}

	private int compareExit((string sceneID, SubSceneEntry.Location locationData) a, (string sceneID, SubSceneEntry.Location locationData) b)
	{
		if (!MiniMapSettings.TryGetMinimapPosition(LevelManager.Instance.MainCharacter.transform.position, out var result))
		{
			return -1;
		}
		if (!MiniMapSettings.TryGetMinimapPosition(a.locationData.position, a.sceneID, out var result2))
		{
			return -1;
		}
		if (!MiniMapSettings.TryGetMinimapPosition(b.locationData.position, b.sceneID, out var result3))
		{
			return -1;
		}
		float num = Vector3.Distance(result, result2);
		float num2 = Vector3.Distance(result, result3);
		if (num > num2)
		{
			return -1;
		}
		return 1;
	}

	private bool IsPathCompitable(SubSceneEntry.Location location)
	{
		string path = location.path;
		int num = path.IndexOf('/');
		if (num != -1 && path.Substring(0, num) == "Exits")
		{
			return true;
		}
		return false;
	}

	private void CreateExit(Vector3 position, int sceneBuildIndex, int debugIndex)
	{
		GameObject go = Object.Instantiate(exitPrefab, position, Quaternion.identity);
		if ((bool)MultiSceneCore.Instance)
		{
			MultiSceneCore.MoveToActiveWithScene(go, sceneBuildIndex);
		}
		SpawnMapElement(position, sceneBuildIndex, debugIndex);
	}

	private void SpawnMapElement(Vector3 position, int sceneBuildIndex, int debugIndex)
	{
		SimplePointOfInterest simplePointOfInterest = new GameObject("MapElement").AddComponent<SimplePointOfInterest>();
		simplePointOfInterest.transform.position = position;
		if (MultiSceneCore.Instance != null)
		{
			simplePointOfInterest.Color = iconColor;
			simplePointOfInterest.ShadowColor = shadowColor;
			simplePointOfInterest.ShadowDistance = shadowDistance;
			simplePointOfInterest.IsArea = false;
			simplePointOfInterest.ScaleFactor = 1f;
			string sceneID = SceneInfoCollection.GetSceneID(sceneBuildIndex);
			simplePointOfInterest.Setup(icon, exitNameKey, followActiveScene: false, sceneID);
			SceneManager.MoveGameObjectToScene(simplePointOfInterest.gameObject, MultiSceneCore.MainScene.Value);
		}
	}
}
