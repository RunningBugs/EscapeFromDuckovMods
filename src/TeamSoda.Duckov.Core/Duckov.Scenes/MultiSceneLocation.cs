using System;
using Eflatun.SceneReference;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.Scenes;

[Serializable]
public struct MultiSceneLocation
{
	[SerializeField]
	private string sceneID;

	[SerializeField]
	private string locationName;

	[SerializeField]
	private string displayName;

	public Transform LocationTransform
	{
		get
		{
			return GetLocationTransform();
		}
		private set
		{
		}
	}

	public string SceneID
	{
		get
		{
			return sceneID;
		}
		set
		{
			sceneID = value;
		}
	}

	public SceneReference Scene => SceneInfoCollection.GetSceneInfo(sceneID)?.SceneReference;

	public string LocationName
	{
		get
		{
			return locationName;
		}
		set
		{
			locationName = value;
		}
	}

	public string DisplayName => displayName.ToPlainText();

	public Transform GetLocationTransform()
	{
		if (Scene == null)
		{
			return null;
		}
		if (Scene.UnsafeReason != SceneReferenceUnsafeReason.None)
		{
			return null;
		}
		return SceneLocationsProvider.GetLocation(Scene, locationName);
	}

	public bool TryGetLocationPosition(out Vector3 result)
	{
		result = default(Vector3);
		if (MultiSceneCore.Instance == null)
		{
			return false;
		}
		if (MultiSceneCore.Instance.TryGetCachedPosition(sceneID, locationName, out result))
		{
			return true;
		}
		Transform location = SceneLocationsProvider.GetLocation(sceneID, locationName);
		if (location != null)
		{
			result = location.transform.position;
			return true;
		}
		return false;
	}

	internal string GetDisplayName()
	{
		return DisplayName;
	}
}
