using System;
using System.Collections.Generic;
using System.Reflection;
using Dialogues;
using Duckov;
using Duckov.MiniMaps.UI;
using Duckov.Quests.UI;
using Duckov.UI;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInputControl : MonoBehaviour
{
	private class InputActionReferences
	{
		public InputAction MoveAxis;

		public InputAction Run;

		public InputAction Aim;

		public InputAction MousePos;

		public InputAction ItemShortcut1;

		public InputAction ItemShortcut2;

		public InputAction Skill_1_StartAim;

		public InputAction Reload;

		public InputAction UI_Inventory;

		public InputAction UI_Map;

		public InputAction Interact;

		public InputAction ScrollWheel;

		public InputAction SwitchWeapon;

		public InputAction SwitchInteractAndBulletType;

		public InputAction Trigger;

		public InputAction ToggleView;

		public InputAction ToggleNightVision;

		public InputAction CancelSkill;

		public InputAction Dash;

		public InputAction ItemShortcut3;

		public InputAction ItemShortcut4;

		public InputAction ItemShortcut5;

		public InputAction ItemShortcut6;

		public InputAction ItemShortcut7;

		public InputAction ItemShortcut8;

		public InputAction ADS;

		public InputAction UI_Quest;

		public InputAction StopAction;

		public InputAction PutAway;

		public InputAction ItemShortcut_Melee;

		public InputAction MouseDelta;

		public InputAction SwitchBulletType;

		public InputActionReferences(PlayerInput playerInput)
		{
			InputActionAsset actions = playerInput.actions;
			Type typeFromHandle = typeof(InputActionReferences);
			Type typeFromHandle2 = typeof(InputAction);
			FieldInfo[] fields = typeFromHandle.GetFields();
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				if (fieldInfo.FieldType != typeFromHandle2)
				{
					Debug.LogError(fieldInfo.FieldType.Name);
					continue;
				}
				InputAction inputAction = actions[fieldInfo.Name];
				if (inputAction == null)
				{
					Debug.LogError("找不到名为 " + fieldInfo.Name + " 的input action");
				}
				else
				{
					fieldInfo.SetValue(this, inputAction);
				}
			}
			array = fields;
			foreach (FieldInfo fieldInfo2 in array)
			{
				if (!(fieldInfo2.FieldType != typeFromHandle2))
				{
					fieldInfo2.GetValue(this);
				}
			}
		}
	}

	public InputManager inputManager;

	private bool runInput;

	private bool adsInput;

	private bool aimDown;

	private Vector2 mousePos;

	private Vector2 mouseDelta;

	private bool mouseKeyboardTriggerInput;

	private bool mouseKeyboardTriggerReleaseThisFrame;

	private bool mouseKeyboardTriggerInputThisFrame;

	private CharacterMainControl character;

	private InputActionReferences inputActions;

	private Queue<Action> unbindCommands = new Queue<Action>();

	private float scollY;

	private int scollYZeroFrames;

	public static CharacterInputControl Instance { get; private set; }

	private PlayerInput PlayerInput => GameManager.MainPlayerInput;

	private bool usingMouseAndKeyboard => InputManager.InputDevice == InputManager.InputDevices.mouseKeyboard;

	private void Awake()
	{
		Instance = this;
		inputActions = new InputActionReferences(PlayerInput);
		RegisterEvents();
	}

	private void OnDestroy()
	{
		UnregisterEvent();
	}

	private void RegisterEvents()
	{
		Bind(inputActions.MoveAxis, OnPlayerMoveInput);
		Bind(inputActions.Run, OnPlayerRunInput);
		Bind(inputActions.MousePos, OnPlayerMouseMove);
		Bind(inputActions.Skill_1_StartAim, OnStartCharacterSkillAim);
		Bind(inputActions.Reload, OnReloadInput);
		Bind(inputActions.Interact, OnInteractInput);
		Bind(inputActions.ScrollWheel, OnMouseScollerInput);
		Bind(inputActions.SwitchWeapon, OnSwitchWeaponInput);
		Bind(inputActions.SwitchInteractAndBulletType, OnSwitchInteractAndBulletTypeInput);
		Bind(inputActions.Trigger, OnPlayerTriggerInputUsingMouseKeyboard);
		Bind(inputActions.ToggleView, OnToggleViewInput);
		Bind(inputActions.ToggleNightVision, OnToggleNightVisionInput);
		Bind(inputActions.CancelSkill, OnCancelSkillInput);
		Bind(inputActions.Dash, OnDashInput);
		Bind(inputActions.ItemShortcut1, OnPlayerSwitchItemAgent1);
		Bind(inputActions.ItemShortcut2, OnPlayerSwitchItemAgent2);
		Bind(inputActions.ItemShortcut3, OnShortCutInput3);
		Bind(inputActions.ItemShortcut4, OnShortCutInput4);
		Bind(inputActions.ItemShortcut5, OnShortCutInput5);
		Bind(inputActions.ItemShortcut6, OnShortCutInput6);
		Bind(inputActions.ItemShortcut7, OnShortCutInput7);
		Bind(inputActions.ItemShortcut8, OnShortCutInput8);
		Bind(inputActions.ADS, OnPlayerAdsInput);
		Bind(inputActions.UI_Inventory, OnUIInventoryInput);
		Bind(inputActions.UI_Map, OnUIMapInput);
		Bind(inputActions.UI_Quest, OnUIQuestViewInput);
		Bind(inputActions.StopAction, OnPlayerStopAction);
		Bind(inputActions.PutAway, OnPutAwayInput);
		Bind(inputActions.ItemShortcut_Melee, OnPlayerSwitchItemAgentMelee);
		Bind(inputActions.MouseDelta, OnPlayerMouseDelta);
	}

	private void UnregisterEvent()
	{
		while (unbindCommands.Count > 0)
		{
			unbindCommands.Dequeue()();
		}
	}

	private void Bind(InputAction action, Action<InputAction.CallbackContext> method)
	{
		action.performed += method;
		action.started += method;
		action.canceled += method;
		unbindCommands.Enqueue(delegate
		{
			Unbind(action, method);
		});
	}

	private void Unbind(InputAction action, Action<InputAction.CallbackContext> method)
	{
		action.performed -= method;
		action.started -= method;
		action.canceled -= method;
	}

	private void Update()
	{
		if (!character)
		{
			character = CharacterMainControl.Main;
			if (!character)
			{
				return;
			}
		}
		if (usingMouseAndKeyboard)
		{
			inputManager.SetMousePosition(mousePos);
			inputManager.SetAimInputUsingMouse(mouseDelta);
			inputManager.SetTrigger(mouseKeyboardTriggerInput, mouseKeyboardTriggerInputThisFrame, mouseKeyboardTriggerReleaseThisFrame);
			if (character.skillAction.holdItemSkillKeeper.CheckSkillAndBinding())
			{
				inputManager.SetAimType(AimTypes.handheldSkill);
				if (mouseKeyboardTriggerInputThisFrame)
				{
					inputManager.StartItemSkillAim();
				}
				else if (mouseKeyboardTriggerReleaseThisFrame)
				{
					Debug.Log("Release");
					inputManager.ReleaseItemSkill();
				}
			}
			else
			{
				inputManager.SetAimType(AimTypes.normalAim);
			}
			UpdateScollerInput();
		}
		mouseKeyboardTriggerInputThisFrame = false;
		mouseKeyboardTriggerReleaseThisFrame = false;
	}

	public void OnPlayerMoveInput(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Vector2 moveInput = context.ReadValue<Vector2>();
			inputManager.SetMoveInput(moveInput);
		}
		if (context.canceled)
		{
			inputManager.SetMoveInput(Vector2.zero);
		}
	}

	public void OnPlayerRunInput(InputAction.CallbackContext context)
	{
		runInput = false;
		if (context.started)
		{
			inputManager.SetRunInput(run: true);
			runInput = true;
		}
		if (context.canceled)
		{
			inputManager.SetRunInput(run: false);
			runInput = false;
		}
	}

	public void OnPlayerAdsInput(InputAction.CallbackContext context)
	{
		adsInput = false;
		if (context.started)
		{
			inputManager.SetAdsInput(ads: true);
			adsInput = true;
		}
		if (context.canceled)
		{
			inputManager.SetAdsInput(ads: false);
			adsInput = false;
		}
	}

	public void OnToggleViewInput(InputAction.CallbackContext context)
	{
		if (!GameManager.Paused && context.started)
		{
			inputManager.ToggleView();
		}
	}

	public void OnToggleNightVisionInput(InputAction.CallbackContext context)
	{
		if (!GameManager.Paused && context.started)
		{
			inputManager.ToggleNightVision();
		}
	}

	public void OnPlayerTriggerInputUsingMouseKeyboard(InputAction.CallbackContext context)
	{
		if (InputManager.InputDevice == InputManager.InputDevices.mouseKeyboard)
		{
			if (context.started)
			{
				mouseKeyboardTriggerInputThisFrame = true;
				mouseKeyboardTriggerInput = true;
				mouseKeyboardTriggerReleaseThisFrame = false;
			}
			else if (context.canceled)
			{
				mouseKeyboardTriggerInputThisFrame = false;
				mouseKeyboardTriggerInput = false;
				mouseKeyboardTriggerReleaseThisFrame = true;
			}
		}
	}

	public void OnPlayerMouseMove(InputAction.CallbackContext context)
	{
		mousePos = context.ReadValue<Vector2>();
	}

	public void OnPlayerMouseDelta(InputAction.CallbackContext context)
	{
		mouseDelta = context.ReadValue<Vector2>();
	}

	public void OnPlayerStopAction(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			inputManager.StopAction();
		}
	}

	public void OnPlayerSwitchItemAgent1(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			inputManager.SwitchItemAgent(1);
		}
	}

	public void OnPlayerSwitchItemAgent2(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			inputManager.SwitchItemAgent(2);
		}
	}

	public void OnPlayerSwitchItemAgentMelee(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			inputManager.SwitchItemAgent(3);
		}
	}

	public void OnStartCharacterSkillAim(InputAction.CallbackContext context)
	{
		inputManager.StartCharacterSkillAim();
	}

	public void OnCharacterSkillRelease()
	{
		inputManager.ReleaseCharacterSkill();
	}

	public void OnReloadInput(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			CharacterMainControl.Main?.TryToReload();
		}
	}

	public void OnUIInventoryInput(InputAction.CallbackContext context)
	{
		if (!context.performed || GameManager.Paused || DialogueUI.Active || SceneLoader.IsSceneLoading)
		{
			return;
		}
		if (View.ActiveView == null)
		{
			if (LevelManager.Instance.IsBaseLevel)
			{
				PlayerStorage.Instance.InteractableLootBox.InteractWithMainCharacter();
			}
			else
			{
				InventoryView.Show();
			}
		}
		else
		{
			View.ActiveView.TryQuit();
		}
	}

	public void OnUIQuestViewInput(InputAction.CallbackContext context)
	{
		if (context.performed && !GameManager.Paused && !DialogueUI.Active)
		{
			if (View.ActiveView == null)
			{
				QuestView.Show();
			}
			else if (View.ActiveView is QuestView)
			{
				View.ActiveView.TryQuit();
			}
		}
	}

	public void OnDashInput(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			inputManager.Dash();
		}
	}

	public void OnUIMapInput(InputAction.CallbackContext context)
	{
		if (context.performed && !GameManager.Paused && !SceneLoader.IsSceneLoading)
		{
			if (View.ActiveView == null)
			{
				MiniMapView.Show();
			}
			else if (View.ActiveView is MiniMapView miniMapView)
			{
				miniMapView.Close();
			}
		}
	}

	public void OnCancelSkillInput(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			inputManager.CancleSkill();
		}
	}

	public void OnInteractInput(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			inputManager.Interact();
		}
	}

	public void OnPutAwayInput(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			inputManager.PutAway();
		}
	}

	public void OnMouseScollerInput(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			scollY = context.ReadValue<Vector2>().y;
		}
	}

	private void UpdateScollerInput()
	{
		if (Mathf.Abs(scollY) > 0.5f && (float)scollYZeroFrames > 3f)
		{
			if (ScrollWheelBehaviour.CurrentBehaviour == ScrollWheelBehaviour.Behaviour.AmmoAndInteract)
			{
				inputManager.SetSwitchInteractInput((scollY > 0f) ? 1 : (-1));
				inputManager.SetSwitchBulletTypeInput((scollY > 0f) ? 1 : (-1));
			}
			else
			{
				inputManager.SetSwitchWeaponInput((scollY > 0f) ? 1 : (-1));
			}
		}
		if (Mathf.Abs(scollY) < 0.5f)
		{
			scollYZeroFrames++;
		}
		else
		{
			scollYZeroFrames = 0;
		}
	}

	public void OnSwitchWeaponInput(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			float num = context.ReadValue<float>();
			inputManager.SetSwitchWeaponInput((!(num > 0f)) ? 1 : (-1));
		}
	}

	public void OnSwitchInteractAndBulletTypeInput(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			float num = context.ReadValue<float>();
			inputManager.SetSwitchInteractInput((!(num > 0f)) ? 1 : (-1));
			inputManager.SetSwitchBulletTypeInput((!(num > 0f)) ? 1 : (-1));
		}
	}

	private void ShortCutInput(int index)
	{
		if (View.ActiveView != null)
		{
			UIInputManager.NotifyShortcutInput(index - 3);
			return;
		}
		Item item = ItemShortcut.Get(index - 3);
		if (!(item == null) && (bool)character)
		{
			if ((bool)item && (bool)item.UsageUtilities && item.UsageUtilities.IsUsable(item, character))
			{
				character.UseItem(item);
			}
			else if ((bool)item && item.GetBool("IsSkill"))
			{
				character.ChangeHoldItem(item);
			}
			else if ((bool)item && item.HasHandHeldAgent)
			{
				Debug.Log("has hand held");
				character.ChangeHoldItem(item);
			}
		}
	}

	public void OnShortCutInput3(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			ShortCutInput(3);
		}
	}

	public void OnShortCutInput4(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			ShortCutInput(4);
		}
	}

	public void OnShortCutInput5(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			ShortCutInput(5);
		}
	}

	public void OnShortCutInput6(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			ShortCutInput(6);
		}
	}

	public void OnShortCutInput7(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			ShortCutInput(7);
		}
	}

	public void OnShortCutInput8(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			ShortCutInput(8);
		}
	}

	internal static InputAction GetInputAction(string name)
	{
		if (Instance == null)
		{
			return null;
		}
		try
		{
			return Instance.PlayerInput.actions[name];
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.LogError("查找 Input Action " + name + " 时发生错误, 返回null");
			return null;
		}
	}

	public static bool GetChangeBulletTypeWasPressed()
	{
		return Instance.inputActions.SwitchBulletType.WasPressedThisFrame();
	}
}
