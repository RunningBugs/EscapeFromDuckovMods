using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.MiniMaps;

public class MapMarkerPOI : MonoBehaviour, IPointOfInterest
{
	[Serializable]
	public struct RuntimeData
	{
		public Vector3 worldPosition;

		public string iconName;

		public string overrideSceneKey;

		public Color color;
	}

	[SerializeField]
	private RuntimeData data;

	public RuntimeData Data => data;

	public Sprite Icon => MapMarkerManager.GetIcon(data.iconName);

	public int OverrideScene => SceneInfoCollection.GetBuildIndex(data.overrideSceneKey);

	public Color Color => data.color;

	public Color ShadowColor => Color.black;

	public float ScaleFactor => 0.8f;

	public void Setup(Vector3 worldPosition, string iconName = "", string overrideScene = "", Color? color = null)
	{
		data = new RuntimeData
		{
			worldPosition = worldPosition,
			iconName = iconName,
			overrideSceneKey = overrideScene,
			color = ((!color.HasValue) ? Color.white : color.Value)
		};
		base.transform.position = worldPosition;
		PointsOfInterests.Unregister(this);
		PointsOfInterests.Register(this);
	}

	public void Setup(RuntimeData data)
	{
		this.data = data;
		base.transform.position = data.worldPosition;
		PointsOfInterests.Unregister(this);
		PointsOfInterests.Register(this);
	}

	public void NotifyClicked(PointerEventData eventData)
	{
		MapMarkerManager.Release(this);
	}

	private void OnDestroy()
	{
		PointsOfInterests.Unregister(this);
	}
}
