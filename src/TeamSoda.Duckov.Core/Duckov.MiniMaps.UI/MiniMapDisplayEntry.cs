using Duckov.Scenes;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Duckov.MiniMaps.UI;

public class MiniMapDisplayEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private Image image;

	private string sceneID;

	private RectTransform _rectTransform;

	private bool showGraphics;

	private bool isCombined;

	private IMiniMapEntry target;

	public SceneReference SceneReference => SceneInfoCollection.GetSceneInfo(SceneID)?.SceneReference;

	public string SceneID => sceneID;

	private RectTransform rectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = base.transform as RectTransform;
			}
			return _rectTransform;
		}
	}

	public MiniMapDisplay Master { get; private set; }

	public bool Hide
	{
		get
		{
			if (target == null)
			{
				return false;
			}
			return target.Hide;
		}
	}

	private void Awake()
	{
		MultiSceneCore.OnSubSceneLoaded += OnSubSceneLoaded;
	}

	private void OnDestroy()
	{
		MultiSceneCore.OnSubSceneLoaded -= OnSubSceneLoaded;
	}

	private void OnSubSceneLoaded(MultiSceneCore core, Scene scene)
	{
		LevelManager.LevelInitializingComment = "Mapping entries";
		Debug.Log("Mapping entries", this);
		RefreshGraphics();
	}

	public bool NoSignal()
	{
		if (target == null)
		{
			return false;
		}
		return target.NoSignal;
	}

	internal void Setup(MiniMapDisplay master, IMiniMapEntry cur, bool showGraphics = true)
	{
		Master = master;
		target = cur;
		if (cur.Sprite != null)
		{
			image.sprite = cur.Sprite;
			rectTransform.sizeDelta = Vector2.one * cur.Sprite.texture.width * cur.PixelSize;
			this.showGraphics = showGraphics;
		}
		else
		{
			this.showGraphics = false;
		}
		if (cur.Hide)
		{
			this.showGraphics = false;
		}
		rectTransform.anchoredPosition = cur.Offset;
		sceneID = cur.SceneID;
		isCombined = false;
		RefreshGraphics();
	}

	internal void SetupCombined(MiniMapDisplay master, IMiniMapDataProvider dataProvider)
	{
		target = null;
		Master = master;
		if (dataProvider != null && !(dataProvider.CombinedSprite == null))
		{
			image.sprite = dataProvider.CombinedSprite;
			rectTransform.sizeDelta = Vector2.one * dataProvider.CombinedSprite.texture.width * dataProvider.PixelSize;
			rectTransform.anchoredPosition = dataProvider.CombinedCenter;
			sceneID = "";
			image.enabled = true;
			showGraphics = true;
			isCombined = true;
			RefreshGraphics();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right && !string.IsNullOrEmpty(sceneID))
		{
			RectTransformUtility.ScreenPointToWorldPointInRectangle(base.transform as RectTransform, eventData.position, null, out var _);
			if (Master.TryConvertToWorldPosition(eventData.position, out var result))
			{
				MiniMapView.RequestMarkPOI(result);
				eventData.Use();
			}
		}
	}

	private void RefreshGraphics()
	{
		bool flag = ShouldShow();
		if (flag)
		{
			image.color = Color.white;
		}
		else
		{
			image.color = Color.clear;
		}
		image.enabled = flag;
	}

	public bool ShouldShow()
	{
		if (!showGraphics)
		{
			return false;
		}
		if (isCombined)
		{
			return showGraphics;
		}
		if (MultiSceneCore.ActiveSubSceneID == SceneID)
		{
			return true;
		}
		return false;
	}
}
