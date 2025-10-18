using System;
using Duckov.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputManager : MonoBehaviour
{
	private static bool instantiated;

	private InputAction inputActionNavigate;

	private InputAction inputActionConfirm;

	private InputAction inputActionCancel;

	private InputAction inputActionPoint;

	private InputAction inputActionMouseDelta;

	private InputAction inputActionMouseClick;

	private InputAction inputActionFastPick;

	private InputAction inputActionDropItem;

	private InputAction inputActionUseItem;

	private InputAction inputActionToggleIndicatorHUD;

	private InputAction inputActionToggleCameraMode;

	private InputAction inputActionWishlistHoveringItem;

	private InputAction inputActionNextPage;

	private InputAction inputActionPreviousPage;

	private InputAction inputActionLockInventoryIndex;

	private InputAction inputActionInteract;

	public static UIInputManager Instance => GameManager.UiInputManager;

	public static bool Ctrl
	{
		get
		{
			if (Keyboard.current == null)
			{
				return false;
			}
			return Keyboard.current.ctrlKey.isPressed;
		}
	}

	public static bool Alt
	{
		get
		{
			if (Keyboard.current == null)
			{
				return false;
			}
			return Keyboard.current.altKey.isPressed;
		}
	}

	public static bool Shift
	{
		get
		{
			if (Keyboard.current == null)
			{
				return false;
			}
			return Keyboard.current.shiftKey.isPressed;
		}
	}

	public static Vector2 Point
	{
		get
		{
			if (!Application.isPlaying)
			{
				return default(Vector2);
			}
			if (Instance == null)
			{
				return default(Vector2);
			}
			if (Instance.inputActionPoint == null)
			{
				return default(Vector2);
			}
			return Instance.inputActionPoint.ReadValue<Vector2>();
		}
	}

	public static Vector2 MouseDelta
	{
		get
		{
			if (!Application.isPlaying)
			{
				return default(Vector2);
			}
			if (Instance == null)
			{
				return default(Vector2);
			}
			if (Instance.inputActionMouseDelta == null)
			{
				return default(Vector2);
			}
			return Instance.inputActionMouseDelta.ReadValue<Vector2>();
		}
	}

	public static bool WasClickedThisFrame
	{
		get
		{
			if (!Application.isPlaying)
			{
				return false;
			}
			if (Instance == null)
			{
				return false;
			}
			if (Instance.inputActionMouseClick == null)
			{
				return false;
			}
			return Instance.inputActionMouseClick.WasPressedThisFrame();
		}
	}

	public static event Action<UIInputEventData> OnNavigate;

	public static event Action<UIInputEventData> OnConfirm;

	public static event Action<UIInputEventData> OnToggleIndicatorHUD;

	public static event Action<UIInputEventData> OnCancelEarly;

	public static event Action<UIInputEventData> OnCancel;

	public static event Action<UIInputEventData> OnFastPick;

	public static event Action<UIInputEventData> OnDropItem;

	public static event Action<UIInputEventData> OnUseItem;

	public static event Action<UIInputEventData> OnToggleCameraMode;

	public static event Action<UIInputEventData> OnWishlistHoveringItem;

	public static event Action<UIInputEventData> OnNextPage;

	public static event Action<UIInputEventData> OnPreviousPage;

	public static event Action<UIInputEventData> OnLockInventoryIndex;

	public static event Action<UIInputEventData, int> OnShortcutInput;

	public static event Action<InputAction.CallbackContext> OnInteractInputContext;

	public static Ray GetPointRay()
	{
		if (Instance == null)
		{
			return default(Ray);
		}
		GameCamera instance = GameCamera.Instance;
		if (instance == null)
		{
			return default(Ray);
		}
		return instance.renderCamera.ScreenPointToRay(Point);
	}

	private void Awake()
	{
		if (!(Instance != this))
		{
			InputActionAsset actions = GameManager.MainPlayerInput.actions;
			inputActionNavigate = actions["UI_Navigate"];
			inputActionConfirm = actions["UI_Confirm"];
			inputActionCancel = actions["UI_Cancel"];
			inputActionPoint = actions["Point"];
			inputActionFastPick = actions["Interact"];
			inputActionDropItem = actions["UI_Item_Drop"];
			inputActionUseItem = actions["UI_Item_use"];
			inputActionToggleIndicatorHUD = actions["UI_ToggleIndicatorHUD"];
			inputActionToggleCameraMode = actions["UI_ToggleCameraMode"];
			inputActionWishlistHoveringItem = actions["UI_WishlistHoveringItem"];
			inputActionNextPage = actions["UI_NextPage"];
			inputActionPreviousPage = actions["UI_PreviousPage"];
			inputActionLockInventoryIndex = actions["UI_LockInventoryIndex"];
			inputActionMouseDelta = actions["MouseDelta"];
			inputActionMouseClick = actions["Click"];
			inputActionInteract = actions["Interact"];
			Bind(inputActionNavigate, OnInputActionNavigate);
			Bind(inputActionConfirm, OnInputActionConfirm);
			Bind(inputActionCancel, OnInputActionCancel);
			Bind(inputActionFastPick, OnInputActionFastPick);
			Bind(inputActionDropItem, OnInputActionDropItem);
			Bind(inputActionUseItem, OnInputActionUseItem);
			Bind(inputActionToggleIndicatorHUD, OnInputActionToggleIndicatorHUD);
			Bind(inputActionToggleCameraMode, OnInputActionToggleCameraMode);
			Bind(inputActionWishlistHoveringItem, OnInputWishlistHoveringItem);
			Bind(inputActionNextPage, OnInputActionNextPage);
			Bind(inputActionPreviousPage, OnInputActionPrevioursPage);
			Bind(inputActionLockInventoryIndex, OnInputActionLockInventoryIndex);
			Bind(inputActionInteract, OnInputActionInteract);
		}
	}

	private void OnDestroy()
	{
		UnBind(inputActionNavigate, OnInputActionNavigate);
		UnBind(inputActionConfirm, OnInputActionConfirm);
		UnBind(inputActionCancel, OnInputActionCancel);
		UnBind(inputActionFastPick, OnInputActionFastPick);
		UnBind(inputActionUseItem, OnInputActionUseItem);
		UnBind(inputActionToggleIndicatorHUD, OnInputActionToggleIndicatorHUD);
		UnBind(inputActionToggleCameraMode, OnInputActionToggleCameraMode);
		UnBind(inputActionWishlistHoveringItem, OnInputWishlistHoveringItem);
		UnBind(inputActionNextPage, OnInputActionNextPage);
		UnBind(inputActionPreviousPage, OnInputActionPrevioursPage);
		UnBind(inputActionLockInventoryIndex, OnInputActionLockInventoryIndex);
		UnBind(inputActionInteract, OnInputActionInteract);
	}

	private void OnInputActionInteract(InputAction.CallbackContext context)
	{
		UIInputManager.OnInteractInputContext?.Invoke(context);
	}

	private void OnInputActionLockInventoryIndex(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			UIInputManager.OnLockInventoryIndex?.Invoke(new UIInputEventData());
		}
	}

	private void OnInputActionNextPage(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			UIInputManager.OnNextPage?.Invoke(new UIInputEventData());
		}
	}

	private void OnInputActionPrevioursPage(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			UIInputManager.OnPreviousPage?.Invoke(new UIInputEventData());
		}
	}

	private void OnInputWishlistHoveringItem(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			UIInputManager.OnWishlistHoveringItem?.Invoke(new UIInputEventData());
		}
	}

	private void OnInputActionToggleCameraMode(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			UIInputManager.OnToggleCameraMode?.Invoke(new UIInputEventData());
		}
	}

	private void OnInputActionDropItem(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			UIInputManager.OnDropItem?.Invoke(new UIInputEventData());
		}
	}

	private void OnInputActionUseItem(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			UIInputManager.OnUseItem?.Invoke(new UIInputEventData());
		}
	}

	private void OnInputActionFastPick(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			UIInputManager.OnFastPick?.Invoke(new UIInputEventData());
		}
	}

	private void OnInputActionCancel(InputAction.CallbackContext context)
	{
		if (!context.started)
		{
			return;
		}
		UIInputEventData uIInputEventData = new UIInputEventData
		{
			cancel = true
		};
		UIInputManager.OnCancelEarly?.Invoke(uIInputEventData);
		if (!uIInputEventData.Used)
		{
			UIInputManager.OnCancel?.Invoke(uIInputEventData);
			if (!uIInputEventData.Used && LevelManager.Instance != null && View.ActiveView == null)
			{
				PauseMenu.Toggle();
			}
		}
	}

	private void OnInputActionConfirm(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			UIInputManager.OnConfirm?.Invoke(new UIInputEventData
			{
				confirm = true
			});
		}
	}

	private void OnInputActionNavigate(InputAction.CallbackContext context)
	{
		Vector2 vector = context.ReadValue<Vector2>();
		UIInputManager.OnNavigate?.Invoke(new UIInputEventData
		{
			vector = vector
		});
	}

	private void OnInputActionToggleIndicatorHUD(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			UIInputManager.OnToggleIndicatorHUD?.Invoke(new UIInputEventData());
		}
	}

	private void Bind(InputAction inputAction, Action<InputAction.CallbackContext> action)
	{
		inputAction.Enable();
		inputAction.started += action;
		inputAction.performed += action;
		inputAction.canceled += action;
	}

	private void UnBind(InputAction inputAction, Action<InputAction.CallbackContext> action)
	{
		if (inputAction != null)
		{
			inputAction.started -= action;
			inputAction.performed -= action;
			inputAction.canceled -= action;
		}
	}

	internal static void NotifyShortcutInput(int index)
	{
		UIInputManager.OnShortcutInput(new UIInputEventData
		{
			confirm = true
		}, index);
	}
}
