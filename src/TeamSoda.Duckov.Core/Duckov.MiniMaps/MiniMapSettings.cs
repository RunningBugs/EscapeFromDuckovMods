using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Duckov.Scenes;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Duckov.MiniMaps;

public class MiniMapSettings : MonoBehaviour, IMiniMapDataProvider
{
	[Serializable]
	public class MapEntry : IMiniMapEntry
	{
		public float imageWorldSize;

		[SceneID]
		public string sceneID;

		public Sprite sprite;

		public SpriteRenderer offsetReference;

		public Vector3 mapWorldCenter;

		public bool hide;

		public bool noSignal;

		public SceneReference SceneReference => SceneInfoCollection.GetSceneInfo(sceneID)?.SceneReference;

		public string SceneID => sceneID;

		public Sprite Sprite => sprite;

		public bool Hide => hide;

		public bool NoSignal => noSignal;

		public float PixelSize
		{
			get
			{
				int width = sprite.texture.width;
				if (width > 0 && imageWorldSize > 0f)
				{
					return imageWorldSize / (float)width;
				}
				return -1f;
			}
		}

		public Vector2 Offset
		{
			get
			{
				if (offsetReference == null)
				{
					return Vector2.zero;
				}
				return offsetReference.transform.localPosition;
			}
		}

		public MapEntry()
		{
		}

		public MapEntry(MapEntry copyFrom)
		{
			imageWorldSize = copyFrom.imageWorldSize;
			sceneID = copyFrom.sceneID;
			sprite = copyFrom.sprite;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Data
	{
	}

	public List<MapEntry> maps;

	public Vector3 combinedCenter;

	public float combinedSize;

	public Sprite combinedSprite;

	public Sprite CombinedSprite => combinedSprite;

	public Vector3 CombinedCenter => combinedCenter;

	public List<IMiniMapEntry> Maps => ((IEnumerable<IMiniMapEntry>)maps).ToList();

	public static MiniMapSettings Instance { get; private set; }

	public float PixelSize
	{
		get
		{
			int width = combinedSprite.texture.width;
			if (width > 0 && combinedSize > 0f)
			{
				return combinedSize / (float)width;
			}
			return -1f;
		}
	}

	private void Awake()
	{
		foreach (MapEntry map in maps)
		{
			SpriteRenderer offsetReference = map.offsetReference;
			if (offsetReference != null)
			{
				offsetReference.gameObject.SetActive(value: false);
			}
		}
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public static bool TryGetMinimapPosition(Vector3 worldPosition, string sceneID, out Vector3 result)
	{
		result = worldPosition;
		if (Instance == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(sceneID))
		{
			return false;
		}
		MapEntry mapEntry = Instance.maps.FirstOrDefault((MapEntry e) => e != null && e.sceneID == sceneID);
		if (mapEntry == null)
		{
			return false;
		}
		Vector3 vector = worldPosition - mapEntry.mapWorldCenter;
		Vector3 vector2 = mapEntry.mapWorldCenter - Instance.combinedCenter;
		_ = vector + vector2;
		return true;
	}

	public static bool TryGetWorldPosition(Vector3 minimapPosition, string sceneID, out Vector3 result)
	{
		result = minimapPosition;
		if (Instance == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(sceneID))
		{
			return false;
		}
		MapEntry mapEntry = Instance.maps.FirstOrDefault((MapEntry e) => e != null && e.sceneID == sceneID);
		if (mapEntry == null)
		{
			return false;
		}
		result = mapEntry.mapWorldCenter + minimapPosition;
		return true;
	}

	public static bool TryGetMinimapPosition(Vector3 worldPosition, out Vector3 result)
	{
		result = worldPosition;
		Scene activeScene = SceneManager.GetActiveScene();
		if (!activeScene.isLoaded)
		{
			return false;
		}
		string sceneID = SceneInfoCollection.GetSceneID(activeScene.buildIndex);
		return TryGetMinimapPosition(worldPosition, sceneID, out result);
	}

	internal void Cache(MiniMapCenter miniMapCenter)
	{
		int scene = miniMapCenter.gameObject.scene.buildIndex;
		MapEntry mapEntry = maps.FirstOrDefault((MapEntry e) => e.SceneReference != null && e.SceneReference.UnsafeReason == SceneReferenceUnsafeReason.None && e.SceneReference.BuildIndex == scene);
		if (mapEntry != null)
		{
			mapEntry.mapWorldCenter = miniMapCenter.transform.position;
		}
	}
}
