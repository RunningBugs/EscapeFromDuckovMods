using System.Collections.Generic;
using Duckov.MiniMaps;
using Duckov.Scenes;
using UnityEngine;

namespace Duckov.Quests;

public class MapElementForTask : MonoBehaviour
{
	private bool visable;

	public new string name;

	public List<MultiSceneLocation> locations;

	public float range;

	private List<SimplePointOfInterest> pointsInstance;

	public Sprite icon;

	public Color iconColor = Color.white;

	public Color shadowColor = Color.white;

	public float shadowDistance;

	public void SetVisibility(bool _visable)
	{
		if (visable != _visable)
		{
			visable = _visable;
			if (MultiSceneCore.Instance == null)
			{
				LevelManager.OnLevelInitialized += OnLevelInitialized;
			}
			else
			{
				SyncVisibility();
			}
		}
	}

	private void OnLevelInitialized()
	{
		SyncVisibility();
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelInitialized -= OnLevelInitialized;
	}

	private void OnDisable()
	{
		LevelManager.OnLevelInitialized -= OnLevelInitialized;
	}

	private void SyncVisibility()
	{
		if (visable)
		{
			if (pointsInstance != null && pointsInstance.Count > 0)
			{
				DespawnAll();
			}
			Spawn();
		}
		else
		{
			DespawnAll();
		}
	}

	private void Spawn()
	{
		foreach (MultiSceneLocation location in locations)
		{
			SpawnOnePoint(location, name);
		}
	}

	private void SpawnOnePoint(MultiSceneLocation _location, string name)
	{
		if (pointsInstance == null)
		{
			pointsInstance = new List<SimplePointOfInterest>();
		}
		if (!(MultiSceneCore.Instance == null) && _location.TryGetLocationPosition(out var _))
		{
			SimplePointOfInterest simplePointOfInterest = new GameObject("MapElement:" + name).AddComponent<SimplePointOfInterest>();
			Debug.Log("Spawning " + simplePointOfInterest.name + " for task", this);
			simplePointOfInterest.Color = iconColor;
			simplePointOfInterest.ShadowColor = shadowColor;
			simplePointOfInterest.ShadowDistance = shadowDistance;
			if (range > 0f)
			{
				simplePointOfInterest.IsArea = true;
				simplePointOfInterest.AreaRadius = range;
			}
			simplePointOfInterest.Setup(icon, name);
			simplePointOfInterest.SetupMultiSceneLocation(_location);
			pointsInstance.Add(simplePointOfInterest);
		}
	}

	public void DespawnAll()
	{
		if (pointsInstance == null || pointsInstance.Count == 0)
		{
			return;
		}
		foreach (SimplePointOfInterest item in pointsInstance)
		{
			Object.Destroy(item.gameObject);
		}
		pointsInstance.Clear();
	}

	public void DespawnPoint(SimplePointOfInterest point)
	{
		if (pointsInstance != null && pointsInstance.Contains(point))
		{
			pointsInstance.Remove(point);
		}
		Object.Destroy(point.gameObject);
	}
}
