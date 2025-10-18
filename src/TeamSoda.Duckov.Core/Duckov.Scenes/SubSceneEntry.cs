using System;
using System.Collections.Generic;
using Eflatun.SceneReference;
using UnityEngine;

namespace Duckov.Scenes;

[Serializable]
public class SubSceneEntry
{
	[Serializable]
	public class Location
	{
		public string path;

		public Vector3 position;

		public bool showInMap;

		[SerializeField]
		private string displayName;

		public string DisplayName => displayName;

		public string DisplayNameRaw
		{
			get
			{
				return displayName;
			}
			set
			{
				displayName = value;
			}
		}
	}

	[Serializable]
	public class TeleporterInfo
	{
		public Vector3 position;

		public MultiSceneLocation target;

		public Vector3 nearestTeleporterPositionToTarget;
	}

	[SceneID]
	public string sceneID;

	[SerializeField]
	private string overrideAmbientSound = "Default";

	[SerializeField]
	private bool isInDoor;

	public List<Location> cachedLocations = new List<Location>();

	public List<TeleporterInfo> cachedTeleporters = new List<TeleporterInfo>();

	public string AmbientSound => overrideAmbientSound;

	public bool IsInDoor => isInDoor;

	public SceneInfoEntry Info => SceneInfoCollection.GetSceneInfo(sceneID);

	public SceneReference SceneReference
	{
		get
		{
			SceneInfoEntry info = Info;
			if (info == null)
			{
				Debug.LogWarning("未找到场景" + sceneID + "的相关信息，获取SceneReference失败。");
				return null;
			}
			return info.SceneReference;
		}
	}

	public string DisplayName
	{
		get
		{
			SceneInfoEntry info = Info;
			if (info == null)
			{
				return sceneID;
			}
			return info.DisplayName;
		}
	}

	internal bool TryGetCachedPosition(string locationPath, out Vector3 result)
	{
		result = default(Vector3);
		if (cachedLocations == null)
		{
			return false;
		}
		Location location = cachedLocations.Find((Location e) => e.path == locationPath);
		if (location == null)
		{
			return false;
		}
		result = location.position;
		return true;
	}
}
