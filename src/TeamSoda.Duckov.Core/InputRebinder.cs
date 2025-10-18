using System;
using Cysharp.Threading.Tasks;
using Saves;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputRebinder : MonoBehaviour
{
	[Header("Debug")]
	[SerializeField]
	private string action = "MoveAxis";

	[SerializeField]
	private int index = 2;

	[SerializeField]
	private string[] excludes = new string[5] { "<Mouse>/leftButton", "<Mouse>/rightButton", "<Pointer>/position", "<Pointer>/delta", "<Pointer>/Press" };

	public static Action<InputAction> OnRebindBegin;

	public static Action<InputAction> OnRebindComplete;

	public static Action OnBindingChanged;

	private static InputActionRebindingExtensions.RebindingOperation operation = new InputActionRebindingExtensions.RebindingOperation();

	private const string SaveKey = "InputBinding";

	private static PlayerInput PlayerInput => GameManager.MainPlayerInput;

	private static bool OperationPending
	{
		get
		{
			if (operation.started)
			{
				if (!operation.canceled)
				{
					return !operation.completed;
				}
				return false;
			}
			return false;
		}
	}

	public void Rebind()
	{
		RebindAsync(action, index, excludes).Forget();
	}

	private void Awake()
	{
		Load();
		UIInputManager.OnCancelEarly += OnUICancel;
	}

	private void OnDestroy()
	{
		UIInputManager.OnCancelEarly -= OnUICancel;
	}

	private void OnUICancel(UIInputEventData data)
	{
		if (OperationPending)
		{
			data.Use();
		}
	}

	public static void Load()
	{
		string text = SavesSystem.LoadGlobal<string>("InputBinding");
		string.IsNullOrEmpty(text);
		try
		{
			PlayerInput.actions.LoadBindingOverridesFromJson(text);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			PlayerInput.actions.RemoveAllBindingOverrides();
		}
	}

	public static void Save()
	{
		string text = PlayerInput.actions.SaveBindingOverridesAsJson();
		SavesSystem.SaveGlobal("InputBinding", text);
		Debug.Log(text);
	}

	public static void Clear()
	{
		PlayerInput.actions.RemoveAllBindingOverrides();
		OnBindingChanged?.Invoke();
		InputIndicator.NotifyBindingChanged();
	}

	private static void Rebind(string name, int index, string[] excludes = null)
	{
		if (OperationPending)
		{
			return;
		}
		InputAction inputAction = PlayerInput.actions[name];
		if (inputAction == null)
		{
			Debug.LogError("找不到名为 " + name + " 的 action");
			return;
		}
		OnRebindBegin?.Invoke(inputAction);
		Debug.Log("Resetting");
		operation.Reset();
		Debug.Log("Settingup");
		inputAction.actionMap.Disable();
		operation.WithCancelingThrough("<Keyboard>/escape").WithAction(inputAction).WithTargetBinding(index)
			.OnComplete(OnComplete)
			.OnCancel(OnCancel);
		if (excludes != null)
		{
			foreach (string path in excludes)
			{
				operation.WithControlsExcluding(path);
			}
		}
		Debug.Log("Starting");
		operation.Start();
	}

	public static async UniTask<bool> RebindAsync(string name, int index, string[] excludes = null, bool save = false)
	{
		if (OperationPending)
		{
			return false;
		}
		Rebind(name, index, excludes);
		while (OperationPending)
		{
			await UniTask.Yield();
		}
		if (save && operation.completed)
		{
			Save();
		}
		return operation.completed;
	}

	private static void OnCancel(InputActionRebindingExtensions.RebindingOperation operation)
	{
		Debug.Log(operation.action.name + " binding canceled");
		operation.action.actionMap.Enable();
		OnRebindComplete?.Invoke(operation.action);
	}

	private static void OnComplete(InputActionRebindingExtensions.RebindingOperation operation)
	{
		Debug.Log(operation.action.name + " bind to " + operation.selectedControl.name);
		operation.action.actionMap.Enable();
		OnRebindComplete?.Invoke(operation.action);
		OnBindingChanged?.Invoke();
		InputIndicator.NotifyRebindComplete(operation.action);
	}
}
