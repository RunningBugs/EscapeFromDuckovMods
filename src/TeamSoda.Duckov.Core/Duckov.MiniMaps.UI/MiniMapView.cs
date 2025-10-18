using Duckov.Scenes;
using Duckov.UI;
using Duckov.UI.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Duckov.MiniMaps.UI;

public class MiniMapView : View
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private MiniMapDisplay display;

	[SerializeField]
	private TextMeshProUGUI mapNameText;

	[SerializeField]
	private TextMeshProUGUI mapInfoText;

	[SerializeField]
	private Slider zoomSlider;

	[SerializeField]
	private float zoomMin = 5f;

	[SerializeField]
	private float zoomMax = 20f;

	[SerializeField]
	[HideInInspector]
	private float _zoom = 5f;

	[SerializeField]
	[Range(0f, 0.01f)]
	private float scrollSensitivity = 0.01f;

	[SerializeField]
	private SimplePointOfInterest markPoiTemplate;

	[SerializeField]
	private AnimationCurve zoomCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private GameObject noSignalIndicator;

	public static MiniMapView Instance => View.GetViewInstance<MiniMapView>();

	private float Zoom
	{
		get
		{
			return _zoom;
		}
		set
		{
			value = Mathf.Clamp01(value);
			_zoom = value;
			OnSetZoom(value);
		}
	}

	private void OnSetZoom(float scale)
	{
		RefreshZoom();
	}

	private void RefreshZoom()
	{
		if (!(display == null))
		{
			RectTransform rectTransform = base.transform as RectTransform;
			Transform obj = display.transform;
			Vector3 vector = rectTransform.localToWorldMatrix.MultiplyPoint(rectTransform.rect.center);
			Vector3 point = obj.worldToLocalMatrix.MultiplyPoint(vector);
			display.transform.localScale = Vector3.one * Mathf.Lerp(zoomMin, zoomMax, zoomCurve.Evaluate(Zoom));
			Vector3 vector2 = obj.localToWorldMatrix.MultiplyPoint(point) - vector;
			display.transform.position -= vector2;
			zoomSlider.SetValueWithoutNotify(Zoom);
		}
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		fadeGroup.Show();
		display.AutoSetup();
		SceneInfoEntry sceneInfoEntry = MultiSceneCore.Instance?.SceneInfo;
		if (sceneInfoEntry != null)
		{
			mapNameText.text = sceneInfoEntry.DisplayName;
			mapInfoText.text = sceneInfoEntry.Description;
		}
		else
		{
			mapNameText.text = "";
			mapInfoText.text = "";
		}
		zoomSlider.SetValueWithoutNotify(Zoom);
		RefreshZoom();
		CeneterPlayer();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	protected override void Awake()
	{
		base.Awake();
		zoomSlider.onValueChanged.AddListener(OnZoomSliderValueChanged);
	}

	private void FixedUpdate()
	{
		RefreshNoSignalIndicator();
	}

	private void RefreshNoSignalIndicator()
	{
		noSignalIndicator.SetActive(display.NoSignal());
	}

	private void OnZoomSliderValueChanged(float value)
	{
		Zoom = value;
	}

	public static void Show()
	{
		if (!(Instance == null) && !(MiniMapSettings.Instance == null))
		{
			Instance.Open();
		}
	}

	public void CeneterPlayer()
	{
		CharacterMainControl main = CharacterMainControl.Main;
		if (!(main == null) && display.TryConvertWorldToMinimap(main.transform.position, SceneInfoCollection.GetSceneID(SceneManager.GetActiveScene().buildIndex), out var result))
		{
			display.Center(result);
		}
	}

	public static bool TryConvertWorldToMinimapPosition(Vector3 worldPosition, string sceneID, out Vector3 result)
	{
		result = default(Vector3);
		if (Instance == null)
		{
			return false;
		}
		return Instance.display.TryConvertWorldToMinimap(worldPosition, sceneID, out result);
	}

	public static bool TryConvertWorldToMinimapPosition(Vector3 worldPosition, out Vector3 result)
	{
		result = default(Vector3);
		if (Instance == null)
		{
			return false;
		}
		string sceneID = SceneInfoCollection.GetSceneID(SceneManager.GetActiveScene().buildIndex);
		return TryConvertWorldToMinimapPosition(worldPosition, sceneID, out result);
	}

	internal void OnScroll(PointerEventData eventData)
	{
		Zoom += eventData.scrollDelta.y * scrollSensitivity;
		eventData.Use();
	}

	internal static void RequestMarkPOI(Vector3 worldPos)
	{
		MapMarkerManager.Request(worldPos);
	}

	public void LoadData(PackedMapData mapData)
	{
		if (!(mapData == null))
		{
			display.Setup(mapData);
		}
	}

	public void LoadCurrent()
	{
		display.AutoSetup();
	}
}
