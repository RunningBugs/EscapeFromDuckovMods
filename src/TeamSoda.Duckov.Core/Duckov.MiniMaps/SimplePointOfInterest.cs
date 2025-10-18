using System;
using Duckov.Scenes;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Duckov.MiniMaps;

public class SimplePointOfInterest : MonoBehaviour, IPointOfInterest
{
	[SerializeField]
	private Sprite icon;

	[SerializeField]
	private Color color = Color.white;

	[SerializeField]
	private Color shadowColor = Color.white;

	[SerializeField]
	private float shadowDistance;

	[LocalizationKey("Default")]
	[SerializeField]
	private string displayName = "";

	[SerializeField]
	private bool followActiveScene;

	[SceneID]
	[SerializeField]
	private string overrideSceneID;

	[SerializeField]
	private bool isArea;

	[SerializeField]
	private float areaRadius;

	[SerializeField]
	private float scaleFactor = 1f;

	[SerializeField]
	private bool hideIcon;

	public float ScaleFactor
	{
		get
		{
			return scaleFactor;
		}
		set
		{
			scaleFactor = value;
		}
	}

	public Color Color
	{
		get
		{
			return color;
		}
		set
		{
			color = value;
		}
	}

	public Color ShadowColor
	{
		get
		{
			return shadowColor;
		}
		set
		{
			shadowColor = value;
		}
	}

	public float ShadowDistance
	{
		get
		{
			return shadowDistance;
		}
		set
		{
			shadowDistance = value;
		}
	}

	public string DisplayName => displayName.ToPlainText();

	public Sprite Icon => icon;

	public int OverrideScene
	{
		get
		{
			if (followActiveScene && MultiSceneCore.ActiveSubScene.HasValue)
			{
				return MultiSceneCore.ActiveSubScene.Value.buildIndex;
			}
			if (!string.IsNullOrEmpty(overrideSceneID))
			{
				return SceneInfoCollection.GetBuildIndex(overrideSceneID);
			}
			return -1;
		}
	}

	public bool IsArea
	{
		get
		{
			return isArea;
		}
		set
		{
			isArea = value;
		}
	}

	public float AreaRadius
	{
		get
		{
			return areaRadius;
		}
		set
		{
			areaRadius = value;
		}
	}

	public bool HideIcon
	{
		get
		{
			return hideIcon;
		}
		set
		{
			hideIcon = value;
		}
	}

	public event Action<PointerEventData> OnClicked;

	private void OnEnable()
	{
		PointsOfInterests.Register(this);
	}

	private void OnDisable()
	{
		PointsOfInterests.Unregister(this);
	}

	public void Setup(Sprite icon = null, string displayName = null, bool followActiveScene = false, string overrideSceneID = null)
	{
		if (icon != null)
		{
			this.icon = icon;
		}
		this.displayName = displayName;
		this.followActiveScene = followActiveScene;
		this.overrideSceneID = overrideSceneID;
		PointsOfInterests.Unregister(this);
		PointsOfInterests.Register(this);
	}

	public void SetColor(Color color)
	{
		this.color = color;
	}

	public bool SetupMultiSceneLocation(MultiSceneLocation location, bool moveToMainScene = true)
	{
		if (!location.TryGetLocationPosition(out var result))
		{
			return false;
		}
		base.transform.position = result;
		overrideSceneID = location.SceneID;
		if (moveToMainScene && MultiSceneCore.MainScene.HasValue)
		{
			SceneManager.MoveGameObjectToScene(base.gameObject, MultiSceneCore.MainScene.Value);
		}
		return true;
	}

	public static SimplePointOfInterest Create(Vector3 position, string sceneID, string displayName, Sprite icon = null, bool hideIcon = false)
	{
		GameObject gameObject = new GameObject("POI_" + displayName);
		gameObject.transform.position = position;
		SimplePointOfInterest simplePointOfInterest = gameObject.AddComponent<SimplePointOfInterest>();
		simplePointOfInterest.overrideSceneID = sceneID;
		simplePointOfInterest.displayName = displayName;
		simplePointOfInterest.hideIcon = hideIcon;
		simplePointOfInterest.icon = icon;
		SceneManager.MoveGameObjectToScene(gameObject, MultiSceneCore.MainScene.Value);
		return simplePointOfInterest;
	}

	public void NotifyClicked(PointerEventData pointerEventData)
	{
		this.OnClicked?.Invoke(pointerEventData);
	}
}
