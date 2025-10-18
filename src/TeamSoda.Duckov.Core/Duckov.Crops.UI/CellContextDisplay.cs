using Duckov.Economy;
using TMPro;
using UnityEngine;

namespace Duckov.Crops.UI;

public class CellContextDisplay : MonoBehaviour
{
	[SerializeField]
	private GardenView master;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private GameObject plantInfo;

	[SerializeField]
	private TextMeshProUGUI plantingCropNameText;

	[SerializeField]
	private CostDisplay plantCostDisplay;

	[SerializeField]
	private GameObject currentCropInfo;

	[SerializeField]
	private TextMeshProUGUI cropNameText;

	[SerializeField]
	private TextMeshProUGUI cropCountdownText;

	[SerializeField]
	private GameObject noWaterIndicator;

	[SerializeField]
	private GameObject ripenIndicator;

	[SerializeField]
	private GameObject operationInfo;

	[SerializeField]
	private TextMeshProUGUI operationNameText;

	private Garden Garden
	{
		get
		{
			if (master == null)
			{
				return null;
			}
			return master.Target;
		}
	}

	private Vector2Int HoveringCoord
	{
		get
		{
			if (master == null)
			{
				return default(Vector2Int);
			}
			return master.HoveringCoord;
		}
	}

	private Crop HoveringCrop
	{
		get
		{
			if (master == null)
			{
				return null;
			}
			return master.HoveringCrop;
		}
	}

	private bool AnyContent
	{
		get
		{
			if (!plantInfo.activeSelf && !currentCropInfo.activeSelf)
			{
				return operationInfo.activeSelf;
			}
			return true;
		}
	}

	private void Show()
	{
		canvasGroup.alpha = 1f;
	}

	private void Hide()
	{
		canvasGroup.alpha = 0f;
	}

	private void Awake()
	{
		master.onContextChanged += OnContextChanged;
	}

	private void Start()
	{
		Refresh();
	}

	private void Update()
	{
		if (master.Hovering && AnyContent)
		{
			Show();
		}
		else
		{
			Hide();
		}
		if ((bool)HoveringCrop)
		{
			UpdateCurrentCropInfo();
		}
	}

	private void LateUpdate()
	{
		Vector3 worldPoint = Garden.CoordToWorldPosition(HoveringCoord) + Vector3.up * 2f;
		Vector2 vector = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPoint);
		base.transform.position = vector;
	}

	private void OnContextChanged()
	{
		Refresh();
	}

	private void Refresh()
	{
		HideAll();
		switch (master.Tool)
		{
		case GardenView.ToolType.Plant:
			if ((bool)HoveringCrop)
			{
				SetupCurrentCropInfo();
				break;
			}
			SetupPlantInfo();
			if (master.PlantingSeedTypeID > 0)
			{
				SetupOperationInfo();
			}
			break;
		case GardenView.ToolType.Harvest:
			if (!(HoveringCrop == null))
			{
				SetupCurrentCropInfo();
				if (HoveringCrop.Ripen)
				{
					SetupOperationInfo();
				}
			}
			break;
		case GardenView.ToolType.Water:
			if (!(HoveringCrop == null))
			{
				SetupCurrentCropInfo();
				SetupOperationInfo();
			}
			break;
		case GardenView.ToolType.Destroy:
			if (!(HoveringCrop == null))
			{
				SetupCurrentCropInfo();
				SetupOperationInfo();
			}
			break;
		case GardenView.ToolType.None:
			break;
		}
	}

	private void SetupCurrentCropInfo()
	{
		currentCropInfo.SetActive(value: true);
		cropNameText.text = HoveringCrop.DisplayName;
		UpdateCurrentCropInfo();
	}

	private void UpdateCurrentCropInfo()
	{
		if (!(HoveringCrop == null))
		{
			cropCountdownText.text = HoveringCrop.RemainingTime.ToString("hh\\:mm\\:ss");
			cropCountdownText.gameObject.SetActive(!HoveringCrop.Ripen && HoveringCrop.Data.watered);
			noWaterIndicator.SetActive(!HoveringCrop.Data.watered);
			ripenIndicator.SetActive(HoveringCrop.Ripen);
		}
	}

	private void SetupOperationInfo()
	{
		operationInfo.SetActive(value: true);
		operationNameText.text = master.ToolDisplayName;
	}

	private void SetupPlantInfo()
	{
		if (master.SeedSelected)
		{
			plantInfo.SetActive(value: true);
			plantingCropNameText.text = master.SeedMeta.DisplayName;
			plantCostDisplay.Setup(new Cost((master.PlantingSeedTypeID, 1L)));
		}
	}

	private void HideAll()
	{
		plantInfo.SetActive(value: false);
		currentCropInfo.SetActive(value: false);
		operationInfo.SetActive(value: false);
		Hide();
	}
}
