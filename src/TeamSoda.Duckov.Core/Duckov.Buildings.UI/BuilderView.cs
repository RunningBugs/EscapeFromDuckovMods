using Cinemachine;
using Cinemachine.Utility;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.UI.Animations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Duckov.Buildings.UI;

public class BuilderView : View, IPointerClickHandler, IEventSystemHandler
{
	private enum Mode
	{
		None,
		Placing,
		Destroying
	}

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private BuildingSelectionPanel selectionPanel;

	[SerializeField]
	private BuildingContextMenu contextMenu;

	[SerializeField]
	private GameObject placingModeInputIndicator;

	[SerializeField]
	private RectTransform followCursorUI;

	[SerializeField]
	private FadeGroup hoveringBuildingFadeGroup;

	[SerializeField]
	private CinemachineVirtualCamera vcam;

	[SerializeField]
	private float cameraSpeed = 10f;

	[SerializeField]
	private float pitch = 45f;

	[SerializeField]
	private float cameraDistance = 10f;

	[SerializeField]
	private float yaw = -45f;

	[SerializeField]
	private Vector3 cameraCursor;

	[SerializeField]
	private BuildingInfo placingBuildingInfo;

	[SerializeField]
	private InputActionReference input_Rotate;

	[SerializeField]
	private InputActionReference input_RequestContextMenu;

	[SerializeField]
	private InputActionReference input_MoveCamera;

	[SerializeField]
	private GridDisplay gridDisplay;

	[SerializeField]
	private BuildingArea targetArea;

	[SerializeField]
	private Mode mode;

	private Building previewBuilding;

	[SerializeField]
	private BuildingRotation previewRotation;

	public static BuilderView Instance => View.GetViewInstance<BuilderView>();

	public void SetupAndShow(BuildingArea targetArea)
	{
		this.targetArea = targetArea;
		Open();
	}

	protected override void Awake()
	{
		base.Awake();
		input_Rotate.action.actionMap.Enable();
		input_MoveCamera.action.actionMap.Enable();
		selectionPanel.onButtonSelected += OnButtonSelected;
		selectionPanel.onRecycleRequested += OnRecycleRequested;
		BuildingManager.OnBuildingListChanged += OnBuildingListChanged;
	}

	private void OnRecycleRequested(BuildingBtnEntry entry)
	{
		BuildingManager.ReturnBuildingsOfType(entry.Info.id).Forget();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		BuildingManager.OnBuildingListChanged -= OnBuildingListChanged;
	}

	private void OnBuildingListChanged()
	{
		selectionPanel.Refresh();
	}

	private void OnButtonSelected(BuildingBtnEntry entry)
	{
		if (!entry.CostEnough)
		{
			NotifyCostNotEnough(entry);
		}
		else if (!entry.Info.ReachedAmountLimit)
		{
			BeginPlacing(entry.Info);
		}
	}

	private void NotifyCostNotEnough(BuildingBtnEntry entry)
	{
		Debug.Log("Resource not enough " + entry.Info.DisplayName);
	}

	private void SetMode(Mode mode)
	{
		placingModeInputIndicator.SetActive(value: false);
		OnExitMode(this.mode);
		this.mode = mode;
		switch (mode)
		{
		case Mode.Placing:
			placingModeInputIndicator.SetActive(value: true);
			break;
		case Mode.None:
		case Mode.Destroying:
			break;
		}
	}

	private void OnExitMode(Mode mode)
	{
		contextMenu.Hide();
		switch (mode)
		{
		case Mode.Placing:
			OnExitPlacing();
			break;
		case Mode.None:
		case Mode.Destroying:
			break;
		}
	}

	public void BeginPlacing(BuildingInfo info)
	{
		if (previewBuilding != null)
		{
			Object.Destroy(previewBuilding.gameObject);
		}
		placingBuildingInfo = info;
		SetMode(Mode.Placing);
		if (info.Prefab == null)
		{
			Debug.LogError("建筑 " + info.DisplayName + " 没有prefab");
		}
		previewBuilding = Object.Instantiate(info.Prefab);
		if (previewBuilding.ID != info.id)
		{
			Debug.LogError("建筑 " + info.DisplayName + " 的 prefab 上的 ID 设置错误");
		}
		SetupPreview(previewBuilding);
		UpdatePlacing();
	}

	public void BeginDestroying()
	{
		SetMode(Mode.Destroying);
	}

	private void SetupPreview(Building previewBuilding)
	{
		if (!(previewBuilding == null))
		{
			previewBuilding.SetupPreview();
		}
	}

	private void OnExitPlacing()
	{
		if (previewBuilding != null)
		{
			Object.Destroy(previewBuilding.gameObject);
		}
		GridDisplay.HidePreview();
	}

	private void Update()
	{
		switch (mode)
		{
		case Mode.None:
			UpdateNone();
			break;
		case Mode.Placing:
			UpdatePlacing();
			break;
		case Mode.Destroying:
			UpdateDestroying();
			break;
		}
		UpdateCamera();
		UpdateContextMenuIndicator();
	}

	private void UpdateContextMenuIndicator()
	{
		TryGetPointingCoord(out var coord);
		bool num = targetArea.GetBuildingInstanceAt(coord);
		bool flag = contextMenu.isActiveAndEnabled;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(followCursorUI.parent as RectTransform, Mouse.current.position.value, null, out var localPoint);
		followCursorUI.localPosition = localPoint;
		int num2;
		if (num)
		{
			num2 = ((!flag) ? 1 : 0);
			if (num2 != 0 && !hoveringBuildingFadeGroup.IsShown)
			{
				hoveringBuildingFadeGroup.Show();
			}
		}
		else
		{
			num2 = 0;
		}
		if (num2 == 0 && hoveringBuildingFadeGroup.IsShown)
		{
			hoveringBuildingFadeGroup.Hide();
		}
	}

	private void UpdateNone()
	{
		if (input_RequestContextMenu.action.WasPressedThisFrame() && TryGetPointingCoord(out var coord))
		{
			Building buildingInstanceAt = targetArea.GetBuildingInstanceAt(coord);
			if (buildingInstanceAt == null)
			{
				contextMenu.Hide();
			}
			else
			{
				contextMenu.Setup(buildingInstanceAt);
			}
		}
	}

	private void UpdateDestroying()
	{
		if (!TryGetPointingCoord(out var coord))
		{
			GridDisplay.HidePreview();
			return;
		}
		BuildingManager.BuildingData buildingAt = targetArea.AreaData.GetBuildingAt(coord);
		if (buildingAt == null)
		{
			GridDisplay.HidePreview();
		}
		else
		{
			gridDisplay.SetBuildingPreviewCoord(buildingAt.Coord, buildingAt.Dimensions, buildingAt.Rotation, validPlacement: false);
		}
	}

	private void ConfirmDestroy()
	{
		if (TryGetPointingCoord(out var coord))
		{
			BuildingManager.BuildingData buildingAt = targetArea.AreaData.GetBuildingAt(coord);
			if (buildingAt != null)
			{
				BuildingManager.ReturnBuilding(buildingAt.GUID).Forget();
				SetMode(Mode.None);
			}
		}
	}

	private void ConfirmPlacement()
	{
		Vector2Int coord;
		if (previewBuilding == null)
		{
			Debug.Log("No Previewing Building");
		}
		else if (!TryGetPointingCoord(out coord, previewBuilding))
		{
			previewBuilding.gameObject.SetActive(value: false);
			Debug.Log("Mouse Not in Plane!");
		}
		else if (!IsValidPlacement(previewBuilding.Dimensions, previewRotation, coord))
		{
			Debug.Log("Invalid Placement!");
		}
		else
		{
			BuildingManager.BuyAndPlace(targetArea.AreaID, previewBuilding.ID, coord, previewRotation);
			SetMode(Mode.None);
		}
	}

	private void UpdatePlacing()
	{
		if ((bool)previewBuilding)
		{
			if (!TryGetPointingCoord(out var coord, previewBuilding))
			{
				previewBuilding.gameObject.SetActive(value: false);
				return;
			}
			bool validPlacement = IsValidPlacement(previewBuilding.Dimensions, previewRotation, coord);
			gridDisplay.SetBuildingPreviewCoord(coord, previewBuilding.Dimensions, previewRotation, validPlacement);
			ShowPreview(coord);
			if (input_Rotate.action.WasPressedThisFrame())
			{
				float num = input_Rotate.action.ReadValue<float>();
				previewRotation = (BuildingRotation)(((float)previewRotation + num + 4f) % 4f);
			}
			if (input_RequestContextMenu.action.WasPressedThisFrame())
			{
				SetMode(Mode.None);
			}
		}
		else
		{
			SetMode(Mode.None);
		}
	}

	private void ShowPreview(Vector2Int coord)
	{
		Vector3 position = targetArea.CoordToWorldPosition(coord, previewBuilding.Dimensions, previewRotation);
		previewBuilding.transform.position = position;
		previewBuilding.gameObject.SetActive(value: true);
		Quaternion quaternion = Quaternion.Euler(new Vector3(0f, 90 * (int)previewRotation, 0f));
		previewBuilding.transform.rotation = targetArea.transform.rotation * quaternion;
	}

	public bool TryGetPointingCoord(out Vector2Int coord, Building previewBuilding = null)
	{
		coord = default(Vector2Int);
		Ray pointRay = UIInputManager.GetPointRay();
		if (!targetArea.Plane.Raycast(pointRay, out var enter))
		{
			return false;
		}
		Vector3 point = pointRay.GetPoint(enter);
		if (previewBuilding != null)
		{
			coord = targetArea.CursorToCoord(point, previewBuilding.Dimensions, previewRotation);
			return true;
		}
		coord = targetArea.CursorToCoord(point, Vector2Int.one, BuildingRotation.Zero);
		return true;
	}

	private bool IsValidPlacement(Vector2Int dimensions, BuildingRotation rotation, Vector2Int coord)
	{
		if (!targetArea.IsPlacementWithinRange(dimensions, rotation, coord))
		{
			return false;
		}
		if (targetArea.AreaData.Collide(dimensions, rotation, coord))
		{
			return false;
		}
		if (targetArea.PhysicsCollide(dimensions, rotation, coord))
		{
			return false;
		}
		return true;
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		SetMode(Mode.None);
		fadeGroup.Show();
		selectionPanel.Setup(targetArea);
		gridDisplay.Setup(targetArea);
		cameraCursor = targetArea.transform.position;
		UpdateCamera();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
		GridDisplay.Close();
		if (previewBuilding != null)
		{
			Object.Destroy(previewBuilding.gameObject);
		}
	}

	private void UpdateCamera()
	{
		if (input_MoveCamera.action.IsPressed())
		{
			Vector2 vector = input_MoveCamera.action.ReadValue<Vector2>();
			Transform transform = vcam.transform;
			float num = Mathf.Abs(Vector3.Dot(transform.forward, Vector3.up));
			float num2 = Mathf.Abs(Vector3.Dot(transform.up, Vector3.up));
			Vector3 vector2 = ((num > num2) ? transform.up : transform.forward).ProjectOntoPlane(Vector3.up);
			Vector3 vector3 = transform.right.ProjectOntoPlane(Vector3.up);
			cameraCursor += (vector3 * vector.x + vector2 * vector.y) * cameraSpeed * Time.unscaledDeltaTime;
			cameraCursor.x = Mathf.Clamp(cameraCursor.x, targetArea.transform.position.x - (float)targetArea.Size.x, targetArea.transform.position.x + (float)targetArea.Size.x);
			cameraCursor.z = Mathf.Clamp(cameraCursor.z, targetArea.transform.position.z - (float)targetArea.Size.y, targetArea.transform.position.z + (float)targetArea.Size.y);
		}
		vcam.transform.position = cameraCursor + Quaternion.Euler(0f, yaw, 0f) * Quaternion.Euler(pitch, 0f, 0f) * Vector3.forward * cameraDistance;
		vcam.transform.LookAt(cameraCursor, Vector3.up);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			contextMenu.Hide();
			switch (mode)
			{
			case Mode.Placing:
				ConfirmPlacement();
				break;
			case Mode.Destroying:
				ConfirmDestroy();
				break;
			}
		}
	}

	public static void Show(BuildingArea target)
	{
		Instance.SetupAndShow(target);
	}
}
