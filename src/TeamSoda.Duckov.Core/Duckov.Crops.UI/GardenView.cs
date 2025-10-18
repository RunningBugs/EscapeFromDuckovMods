using System;
using Duckov.Economy;
using Duckov.UI;
using Duckov.UI.Animations;
using ItemStatsSystem;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.Crops.UI;

public class GardenView : View, IPointerClickHandler, IEventSystemHandler, IPointerMoveHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ICursorDataProvider
{
	public enum ToolType
	{
		None,
		Plant,
		Harvest,
		Water,
		Destroy
	}

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private GameObject mainEventReceiver;

	[SerializeField]
	private Button btn_ChangePlant;

	[SerializeField]
	private GameObject plantModePanel;

	[SerializeField]
	private ItemMetaDisplay seedItemDisplay;

	[SerializeField]
	private GameObject seedItemPlaceHolder;

	[SerializeField]
	private TextMeshProUGUI seedAmountText;

	[SerializeField]
	private GardenViewCropSelector cropSelector;

	[SerializeField]
	private Transform cellHoveringGizmos;

	[SerializeField]
	[LocalizationKey("Default")]
	private string textKey_Plant = "Garden_Plant";

	[SerializeField]
	[LocalizationKey("Default")]
	private string textKey_Harvest = "Garden_Harvest";

	[SerializeField]
	[LocalizationKey("Default")]
	private string textKey_Destroy = "Garden_Destroy";

	[SerializeField]
	[LocalizationKey("Default")]
	private string textKey_Water = "Garden_Water";

	[SerializeField]
	[LocalizationKey("Default")]
	private string textKey_TargetOccupied = "Garden_TargetOccupied";

	[SerializeField]
	private Transform cameraRig;

	[SerializeField]
	private Image cursorIcon;

	[SerializeField]
	private TextMeshProUGUI cursorAmountDisplay;

	[SerializeField]
	private ItemMetaDisplay cursorItemDisplay;

	[SerializeField]
	private Sprite iconPlant;

	[SerializeField]
	private Sprite iconHarvest;

	[SerializeField]
	private Sprite iconWater;

	[SerializeField]
	private Sprite iconDestroy;

	[SerializeField]
	private CursorData cursorPlant;

	[SerializeField]
	private CursorData cursorHarvest;

	[SerializeField]
	private CursorData cursorWater;

	[SerializeField]
	private CursorData cursorDestroy;

	[SerializeField]
	private Transform cursor3DTransform;

	[SerializeField]
	private Vector3 cursor3DOffset = Vector3.up;

	[SerializeField]
	private GameObject cursor3D_Plant;

	[SerializeField]
	private GameObject cursor3D_Harvest;

	[SerializeField]
	private GameObject cursor3D_Water;

	[SerializeField]
	private GameObject cursor3D_Destory;

	private Vector3 camFocusPos;

	private int _plantingSeedTypeID;

	private bool enabledCursor;

	private bool show3DCursor;

	private bool hoveringBG;

	private int seedAmount;

	private bool dragging;

	public static GardenView Instance { get; private set; }

	public Garden Target { get; private set; }

	public bool SeedSelected { get; private set; }

	public int PlantingSeedTypeID
	{
		get
		{
			return _plantingSeedTypeID;
		}
		private set
		{
			_plantingSeedTypeID = value;
			SeedMeta = ItemAssetsCollection.GetMetaData(value);
		}
	}

	public ItemMetaData SeedMeta { get; private set; }

	public ToolType Tool { get; private set; }

	public bool Hovering { get; private set; }

	public Vector2Int HoveringCoord { get; private set; }

	public Crop HoveringCrop { get; private set; }

	public string ToolDisplayName => Tool switch
	{
		ToolType.None => "...", 
		ToolType.Plant => textKey_Plant.ToPlainText(), 
		ToolType.Harvest => textKey_Harvest.ToPlainText(), 
		ToolType.Water => textKey_Water.ToPlainText(), 
		ToolType.Destroy => textKey_Destroy.ToPlainText(), 
		_ => "?", 
	};

	public event Action onContextChanged;

	public event Action onToolChanged;

	protected override void Awake()
	{
		base.Awake();
		btn_ChangePlant.onClick.AddListener(OnBtnChangePlantClicked);
		ItemUtilities.OnPlayerItemOperation += OnPlayerItemOperation;
		Instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ItemUtilities.OnPlayerItemOperation -= OnPlayerItemOperation;
	}

	private void OnDisable()
	{
		if ((bool)cellHoveringGizmos)
		{
			cellHoveringGizmos.gameObject.SetActive(value: false);
		}
	}

	private void OnPlayerItemOperation()
	{
		if (base.gameObject.activeSelf && SeedSelected)
		{
			RefreshSeedAmount();
		}
	}

	public static void Show(Garden target)
	{
		Instance.Target = target;
		Instance.Open();
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		if (Target == null)
		{
			Target = UnityEngine.Object.FindObjectOfType<Garden>();
		}
		if (Target == null)
		{
			Debug.Log("No Garden instance found. Aborting..");
			Close();
		}
		fadeGroup.Show();
		RefreshSeedInfoDisplay();
		EnableCursor();
		SetTool(Tool);
		CenterCamera();
	}

	protected override void OnClose()
	{
		base.OnClose();
		cropSelector.Hide();
		fadeGroup.Hide();
		ReleaseCursor();
	}

	private void EnableCursor()
	{
		CursorManager.Register(this);
	}

	private void ReleaseCursor()
	{
		CursorManager.Unregister(this);
	}

	private void ChangeCursor()
	{
		CursorManager.NotifyRefresh();
	}

	private void Update()
	{
		UpdateContext();
		UpdateCursor3D();
	}

	private void OnBtnChangePlantClicked()
	{
		cropSelector.Show();
	}

	private void OnContextChanged()
	{
		this.onContextChanged?.Invoke();
		RefreshHoveringGizmos();
		RefreshCursor();
		if (dragging && Hovering)
		{
			ExecuteTool(HoveringCoord);
		}
		ChangeCursor();
		RefreshCursor3DActive();
	}

	private void RefreshCursor()
	{
		cursorIcon.gameObject.SetActive(value: false);
		cursorAmountDisplay.gameObject.SetActive(value: false);
		cursorItemDisplay.gameObject.SetActive(value: false);
		switch (Tool)
		{
		case ToolType.Plant:
			cursorAmountDisplay.gameObject.SetActive(SeedSelected);
			cursorItemDisplay.gameObject.SetActive(SeedSelected);
			cursorIcon.sprite = iconPlant;
			break;
		case ToolType.Harvest:
			cursorIcon.gameObject.SetActive(value: true);
			cursorIcon.sprite = iconHarvest;
			break;
		case ToolType.Water:
			cursorIcon.gameObject.SetActive(value: true);
			cursorIcon.sprite = iconWater;
			break;
		case ToolType.Destroy:
			cursorIcon.gameObject.SetActive(value: true);
			cursorIcon.sprite = iconDestroy;
			break;
		case ToolType.None:
			break;
		}
	}

	private void RefreshHoveringGizmos()
	{
		if ((bool)cellHoveringGizmos)
		{
			if (!Hovering || !base.enabled)
			{
				cellHoveringGizmos.gameObject.SetActive(value: false);
				return;
			}
			cellHoveringGizmos.gameObject.SetActive(value: true);
			cellHoveringGizmos.SetParent(null);
			cellHoveringGizmos.localScale = Vector3.one;
			cellHoveringGizmos.position = Target.CoordToWorldPosition(HoveringCoord);
			cellHoveringGizmos.rotation = Quaternion.LookRotation(-Vector3.up);
		}
	}

	public void SetTool(ToolType action)
	{
		Tool = action;
		OnContextChanged();
		plantModePanel.SetActive(action == ToolType.Plant);
		this.onToolChanged?.Invoke();
		RefreshSeedAmount();
	}

	private CursorData GetCursorByTool(ToolType action)
	{
		return null;
	}

	private void UpdateContext()
	{
		bool hovering = Hovering;
		Crop hoveringCrop = HoveringCrop;
		Vector2Int hoveringCoord = HoveringCoord;
		Vector2Int? pointingCoord = GetPointingCoord();
		if (!pointingCoord.HasValue)
		{
			HoveringCrop = null;
			return;
		}
		HoveringCoord = pointingCoord.Value;
		HoveringCrop = Target[HoveringCoord];
		Hovering = hoveringBG;
		if (!HoveringCrop)
		{
			Hovering &= Target.IsCoordValid(HoveringCoord);
		}
		if (hovering != (bool)HoveringCrop || hoveringCrop != HoveringCrop || hoveringCoord != HoveringCoord)
		{
			OnContextChanged();
		}
	}

	private void UpdateCursor3D()
	{
		Vector3 planePoint;
		bool flag = TryPointerOnPlanePoint(UIInputManager.Point, out planePoint);
		show3DCursor = flag && Hovering;
		cursor3DTransform.gameObject.SetActive(show3DCursor);
		if (flag)
		{
			Vector3 position = cursor3DTransform.position;
			Vector3 vector = planePoint + cursor3DOffset;
			Vector3 position2 = ((!show3DCursor) ? vector : Vector3.Lerp(position, vector, 0.25f));
			cursor3DTransform.position = position2;
		}
	}

	private void RefreshCursor3DActive()
	{
		cursor3D_Plant.SetActive(ShouldShowCursor(ToolType.Plant));
		cursor3D_Water.SetActive(ShouldShowCursor(ToolType.Water));
		cursor3D_Harvest.SetActive(ShouldShowCursor(ToolType.Harvest));
		cursor3D_Destory.SetActive(ShouldShowCursor(ToolType.Destroy));
		bool ShouldShowCursor(ToolType toolType)
		{
			if (Tool != toolType)
			{
				return false;
			}
			if (!Hovering)
			{
				return false;
			}
			switch (toolType)
			{
			case ToolType.None:
				return false;
			case ToolType.Plant:
				if (SeedSelected && seedAmount > 0)
				{
					return !HoveringCrop;
				}
				return false;
			case ToolType.Harvest:
				if ((bool)HoveringCrop)
				{
					return HoveringCrop.Ripen;
				}
				return false;
			case ToolType.Water:
				return HoveringCrop;
			case ToolType.Destroy:
				return HoveringCrop;
			default:
				return false;
			}
		}
	}

	public void SelectSeed(int seedTypeID)
	{
		PlantingSeedTypeID = seedTypeID;
		if (seedTypeID > 0)
		{
			SeedSelected = true;
		}
		RefreshSeedInfoDisplay();
		OnContextChanged();
	}

	private void RefreshSeedInfoDisplay()
	{
		if (SeedSelected)
		{
			seedItemDisplay.Setup(PlantingSeedTypeID);
			cursorItemDisplay.Setup(PlantingSeedTypeID);
		}
		seedItemDisplay.gameObject.SetActive(SeedSelected);
		seedItemPlaceHolder.gameObject.SetActive(!SeedSelected);
		RefreshSeedAmount();
	}

	private bool TryPointerOnPlanePoint(Vector2 pointerPos, out Vector3 planePoint)
	{
		planePoint = default(Vector3);
		if (Target == null)
		{
			return false;
		}
		Ray ray = RectTransformUtility.ScreenPointToRay(Camera.main, pointerPos);
		if (!new Plane(Target.transform.up, Target.transform.position).Raycast(ray, out var enter))
		{
			return false;
		}
		planePoint = ray.GetPoint(enter);
		return true;
	}

	private bool TryPointerPosToCoord(Vector2 pointerPos, out Vector2Int result)
	{
		result = default(Vector2Int);
		if (Target == null)
		{
			return false;
		}
		Ray ray = RectTransformUtility.ScreenPointToRay(Camera.main, pointerPos);
		if (!new Plane(Target.transform.up, Target.transform.position).Raycast(ray, out var enter))
		{
			return false;
		}
		Vector3 point = ray.GetPoint(enter);
		result = Target.WorldPositionToCoord(point);
		return true;
	}

	private Vector2Int? GetPointingCoord()
	{
		if (!TryPointerPosToCoord(UIInputManager.Point, out var result))
		{
			return null;
		}
		return result;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (TryPointerPosToCoord(eventData.position, out var result))
		{
			ExecuteTool(result);
		}
	}

	private void ExecuteTool(Vector2Int coord)
	{
		switch (Tool)
		{
		case ToolType.Plant:
			CropActionPlant(coord);
			break;
		case ToolType.Harvest:
			CropActionHarvest(coord);
			break;
		case ToolType.Water:
			CropActionWater(coord);
			break;
		case ToolType.Destroy:
			CropActionDestroy(coord);
			break;
		case ToolType.None:
			break;
		}
	}

	private void CropActionDestroy(Vector2Int coord)
	{
		Crop crop = Target[coord];
		if (!(crop == null))
		{
			if (crop.Ripen)
			{
				crop.Harvest();
			}
			else
			{
				crop.DestroyCrop();
			}
		}
	}

	private void CropActionWater(Vector2Int coord)
	{
		Crop crop = Target[coord];
		if (!(crop == null))
		{
			crop.Water();
		}
	}

	private void CropActionHarvest(Vector2Int coord)
	{
		Crop crop = Target[coord];
		if (!(crop == null))
		{
			crop.Harvest();
		}
	}

	private void CropActionPlant(Vector2Int coord)
	{
		if (!Target.IsCoordValid(coord) || Target[coord] != null)
		{
			return;
		}
		CropInfo? cropInfoFromSeedType = GetCropInfoFromSeedType(PlantingSeedTypeID);
		if (cropInfoFromSeedType.HasValue)
		{
			Cost cost = new Cost((PlantingSeedTypeID, 1L));
			if (cost.Pay())
			{
				Target.Plant(coord, cropInfoFromSeedType.Value.id);
			}
		}
	}

	private CropInfo? GetCropInfoFromSeedType(int plantingSeedTypeID)
	{
		SeedInfo seedInfo = CropDatabase.GetSeedInfo(plantingSeedTypeID);
		if (seedInfo.cropIDs == null)
		{
			return null;
		}
		if (seedInfo.cropIDs.Count <= 0)
		{
			return null;
		}
		return CropDatabase.GetCropInfo(seedInfo.GetRandomCropID());
	}

	public void OnPointerMove(PointerEventData eventData)
	{
		if (eventData.pointerCurrentRaycast.gameObject == mainEventReceiver)
		{
			hoveringBG = true;
		}
		else
		{
			hoveringBG = false;
		}
	}

	private void RefreshSeedAmount()
	{
		if (SeedSelected)
		{
			string text = $"x{seedAmount = ItemUtilities.GetItemCount(PlantingSeedTypeID)}";
			seedAmountText.text = text;
			cursorAmountDisplay.text = text;
		}
		else
		{
			seedAmountText.text = "";
			cursorAmountDisplay.text = "";
			seedAmount = 0;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		dragging = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		dragging = false;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		dragging = false;
	}

	private void UpdateCamera()
	{
		cameraRig.transform.position = camFocusPos;
	}

	private void CenterCamera()
	{
		if (!(Target == null))
		{
			camFocusPos = Target.transform.TransformPoint(Target.cameraRigCenter);
			UpdateCamera();
		}
	}

	public CursorData GetCursorData()
	{
		return GetCursorByTool(Tool);
	}
}
