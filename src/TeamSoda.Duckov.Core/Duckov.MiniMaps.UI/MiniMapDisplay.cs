using System;
using System.Collections.ObjectModel;
using System.Linq;
using Cinemachine.Utility;
using Duckov.Scenes;
using Duckov.Utilities;
using UI_Spline_Renderer;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.MiniMaps.UI;

public class MiniMapDisplay : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	[SerializeField]
	private MiniMapView master;

	[SerializeField]
	private MiniMapDisplayEntry mapDisplayEntryPrefab;

	[SerializeField]
	private PointOfInterestEntry pointOfInterestEntryPrefab;

	[SerializeField]
	private UISplineRenderer teleporterSplines;

	[SerializeField]
	private bool autoSetupOnEnable;

	[SerializeField]
	private float padding = 25f;

	private PrefabPool<MiniMapDisplayEntry> _mapEntryPool;

	private PrefabPool<PointOfInterestEntry> _pointOfInterestEntryPool;

	private PrefabPool<MiniMapDisplayEntry> MapEntryPool
	{
		get
		{
			if (_mapEntryPool == null)
			{
				_mapEntryPool = new PrefabPool<MiniMapDisplayEntry>(mapDisplayEntryPrefab, base.transform, OnGetMapEntry);
			}
			return _mapEntryPool;
		}
	}

	private PrefabPool<PointOfInterestEntry> PointOfInterestEntryPool
	{
		get
		{
			if (_pointOfInterestEntryPool == null)
			{
				_pointOfInterestEntryPool = new PrefabPool<PointOfInterestEntry>(pointOfInterestEntryPrefab, base.transform, OnGetPointOfInterestEntry);
			}
			return _pointOfInterestEntryPool;
		}
	}

	public bool NoSignal()
	{
		foreach (MiniMapDisplayEntry activeEntry in MapEntryPool.ActiveEntries)
		{
			if (!(activeEntry == null) && !(activeEntry.SceneID != MultiSceneCore.ActiveSubSceneID) && activeEntry.NoSignal())
			{
				return true;
			}
		}
		return false;
	}

	private void OnGetPointOfInterestEntry(PointOfInterestEntry entry)
	{
		entry.gameObject.hideFlags |= HideFlags.DontSave;
	}

	private void OnGetMapEntry(MiniMapDisplayEntry entry)
	{
		entry.gameObject.hideFlags |= HideFlags.DontSave;
	}

	private void Awake()
	{
		if (master == null)
		{
			master = GetComponentInParent<MiniMapView>();
		}
		mapDisplayEntryPrefab.gameObject.SetActive(value: false);
		pointOfInterestEntryPrefab.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		if (autoSetupOnEnable)
		{
			AutoSetup();
		}
		RegisterEvents();
	}

	private void OnDisable()
	{
		UnregisterEvents();
	}

	private void RegisterEvents()
	{
		PointsOfInterests.OnPointRegistered += HandlePointOfInterest;
		PointsOfInterests.OnPointUnregistered += ReleasePointOfInterest;
	}

	private void UnregisterEvents()
	{
		PointsOfInterests.OnPointRegistered -= HandlePointOfInterest;
		PointsOfInterests.OnPointUnregistered -= ReleasePointOfInterest;
	}

	internal void AutoSetup()
	{
		MiniMapSettings miniMapSettings = UnityEngine.Object.FindAnyObjectByType<MiniMapSettings>();
		if ((bool)miniMapSettings)
		{
			Setup(miniMapSettings);
		}
	}

	public void Setup(IMiniMapDataProvider dataProvider)
	{
		if (dataProvider == null)
		{
			return;
		}
		MapEntryPool.ReleaseAll();
		bool flag = dataProvider.CombinedSprite != null;
		foreach (IMiniMapEntry map in dataProvider.Maps)
		{
			MiniMapDisplayEntry miniMapDisplayEntry = MapEntryPool.Get();
			miniMapDisplayEntry.Setup(this, map, !flag);
			miniMapDisplayEntry.gameObject.SetActive(value: true);
		}
		if (flag)
		{
			MiniMapDisplayEntry miniMapDisplayEntry2 = MapEntryPool.Get();
			miniMapDisplayEntry2.SetupCombined(this, dataProvider);
			miniMapDisplayEntry2.gameObject.SetActive(value: true);
			miniMapDisplayEntry2.transform.SetAsFirstSibling();
		}
		SetupRotation();
		FitContent();
		HandlePointsOfInterests();
		HandleTeleporters();
	}

	private void SetupRotation()
	{
		Vector3 to = LevelManager.Instance.GameCamera.mainVCam.transform.up.ProjectOntoPlane(Vector3.up);
		float z = Vector3.SignedAngle(Vector3.forward, to, Vector3.up);
		base.transform.localRotation = Quaternion.Euler(0f, 0f, z);
	}

	private void HandlePointsOfInterests()
	{
		PointOfInterestEntryPool.ReleaseAll();
		foreach (MonoBehaviour point in PointsOfInterests.Points)
		{
			if (!(point == null))
			{
				HandlePointOfInterest(point);
			}
		}
	}

	private void HandlePointOfInterest(MonoBehaviour poi)
	{
		int targetSceneIndex = poi.gameObject.scene.buildIndex;
		if (poi is IPointOfInterest { OverrideScene: >=0 } pointOfInterest)
		{
			targetSceneIndex = pointOfInterest.OverrideScene;
		}
		if (MultiSceneCore.ActiveSubScene.HasValue && targetSceneIndex == MultiSceneCore.ActiveSubScene.Value.buildIndex)
		{
			MiniMapDisplayEntry miniMapDisplayEntry = MapEntryPool.ActiveEntries.FirstOrDefault((MiniMapDisplayEntry e) => e.SceneReference != null && e.SceneReference.BuildIndex == targetSceneIndex);
			if (!(miniMapDisplayEntry == null) && !miniMapDisplayEntry.Hide)
			{
				PointOfInterestEntryPool.Get().Setup(this, poi, miniMapDisplayEntry);
			}
		}
	}

	private void ReleasePointOfInterest(MonoBehaviour poi)
	{
		PointOfInterestEntry pointOfInterestEntry = PointOfInterestEntryPool.ActiveEntries.FirstOrDefault((PointOfInterestEntry e) => e != null && e.Target == poi);
		if ((bool)pointOfInterestEntry)
		{
			PointOfInterestEntryPool.Release(pointOfInterestEntry);
		}
	}

	private void HandleTeleporters()
	{
		teleporterSplines.gameObject.SetActive(value: false);
	}

	private void FitContent()
	{
		ReadOnlyCollection<MiniMapDisplayEntry> activeEntries = MapEntryPool.ActiveEntries;
		Vector2 vector = new Vector2(float.MinValue, float.MinValue);
		Vector2 vector2 = new Vector2(float.MaxValue, float.MaxValue);
		foreach (MiniMapDisplayEntry item in activeEntries)
		{
			RectTransform rectTransform = item.transform as RectTransform;
			Vector2 vector3 = rectTransform.anchoredPosition + rectTransform.rect.min;
			Vector2 vector4 = rectTransform.anchoredPosition + rectTransform.rect.max;
			vector.x = MathF.Max(vector4.x, vector.x);
			vector.y = MathF.Max(vector4.y, vector.y);
			vector2.x = MathF.Min(vector3.x, vector2.x);
			vector2.y = MathF.Min(vector3.y, vector2.y);
		}
		Vector2 vector5 = (vector + vector2) / 2f;
		foreach (MiniMapDisplayEntry item2 in activeEntries)
		{
			item2.transform.localPosition -= (Vector3)vector5;
		}
		(base.transform as RectTransform).sizeDelta = new Vector2(vector.x - vector2.x + padding * 2f, vector.y - vector2.y + padding * 2f);
	}

	public bool TryConvertWorldToMinimap(Vector3 worldPosition, string sceneID, out Vector3 result)
	{
		result = worldPosition;
		MiniMapDisplayEntry miniMapDisplayEntry = MapEntryPool.ActiveEntries.FirstOrDefault((MiniMapDisplayEntry e) => e != null && e.SceneID == sceneID);
		if (miniMapDisplayEntry == null)
		{
			return false;
		}
		Vector3 center = MiniMapCenter.GetCenter(sceneID);
		Vector3 vector = worldPosition - center;
		Vector3 point = new Vector3(vector.x, vector.z);
		Vector3 point2 = miniMapDisplayEntry.transform.localToWorldMatrix.MultiplyPoint(point);
		result = base.transform.worldToLocalMatrix.MultiplyPoint(point2);
		return true;
	}

	public bool TryConvertToWorldPosition(Vector3 displayPosition, out Vector3 result)
	{
		result = default(Vector3);
		string activeSubsceneID = MultiSceneCore.ActiveSubSceneID;
		MiniMapDisplayEntry miniMapDisplayEntry = MapEntryPool.ActiveEntries.FirstOrDefault((MiniMapDisplayEntry e) => e != null && e.SceneID == activeSubsceneID);
		if (miniMapDisplayEntry == null)
		{
			return false;
		}
		Vector3 vector = miniMapDisplayEntry.transform.worldToLocalMatrix.MultiplyPoint(displayPosition);
		Vector3 vector2 = new Vector3(vector.x, 0f, vector.y);
		Vector3 center = MiniMapCenter.GetCenter(activeSubsceneID);
		result = center + vector2;
		return true;
	}

	internal void Center(Vector3 minimapPos)
	{
		RectTransform rectTransform = base.transform as RectTransform;
		if (!(rectTransform == null))
		{
			Vector3 vector = rectTransform.localToWorldMatrix.MultiplyPoint(minimapPos);
			Vector3 vector2 = (rectTransform.parent as RectTransform).position - vector;
			rectTransform.position += vector2;
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		master.OnScroll(eventData);
	}
}
