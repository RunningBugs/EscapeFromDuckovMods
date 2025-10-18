using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Duckov.MiniGames;

public class MiniGameInputHandler : MonoBehaviour
{
	[SerializeField]
	private MiniGame game;

	private InputAction inputActionMove;

	private InputAction inputActionButtonA;

	private InputAction inputActionButtonB;

	private InputAction inputActionSelect;

	private InputAction inputActionStart;

	private InputAction inputActionMouseDelta;

	private List<Action> unbindCommands = new List<Action>();

	private void Awake()
	{
		InputActionAsset actions = GameManager.MainPlayerInput.actions;
		inputActionMove = actions["MoveAxis"];
		inputActionButtonA = actions["MiniGameA"];
		inputActionButtonB = actions["MiniGameB"];
		inputActionSelect = actions["MiniGameSelect"];
		inputActionStart = actions["MiniGameStart"];
		inputActionMouseDelta = actions["MouseDelta"];
		inputActionButtonA.actionMap.Enable();
		Bind(inputActionMove, OnMove);
		Bind(inputActionButtonA, OnButtonA);
		Bind(inputActionButtonB, OnButtonB);
		Bind(inputActionSelect, OnSelect);
		Bind(inputActionStart, OnStart);
		Bind(inputActionMouseDelta, OnMouseDelta);
	}

	private void OnMouseDelta(InputAction.CallbackContext context)
	{
		if (base.isActiveAndEnabled && !(game == null))
		{
			game.SetInputAxis(context.ReadValue<Vector2>(), 1);
		}
	}

	public void ClearInput()
	{
		game?.ClearInput();
	}

	private void OnDisable()
	{
		ClearInput();
	}

	private void SetGameButtonByContext(MiniGame.Button button, InputAction.CallbackContext context)
	{
		if (context.started)
		{
			game.SetButton(button, down: true);
		}
		else if (context.canceled)
		{
			game.SetButton(button, down: false);
		}
	}

	private void OnStart(InputAction.CallbackContext context)
	{
		if (base.isActiveAndEnabled && !(game == null))
		{
			SetGameButtonByContext(MiniGame.Button.Start, context);
		}
	}

	private void OnSelect(InputAction.CallbackContext context)
	{
		if (base.isActiveAndEnabled && !(game == null))
		{
			SetGameButtonByContext(MiniGame.Button.Select, context);
		}
	}

	private void OnButtonB(InputAction.CallbackContext context)
	{
		if (base.isActiveAndEnabled && !(game == null))
		{
			SetGameButtonByContext(MiniGame.Button.B, context);
		}
	}

	private void OnButtonA(InputAction.CallbackContext context)
	{
		if (base.isActiveAndEnabled && !(game == null))
		{
			SetGameButtonByContext(MiniGame.Button.A, context);
		}
	}

	private void OnMove(InputAction.CallbackContext context)
	{
		if (base.isActiveAndEnabled && !(game == null))
		{
			game.SetInputAxis(context.ReadValue<Vector2>());
		}
	}

	private void OnDestroy()
	{
		foreach (Action unbindCommand in unbindCommands)
		{
			unbindCommand?.Invoke();
		}
	}

	private void Bind(InputAction inputAction, Action<InputAction.CallbackContext> action)
	{
		inputAction.Enable();
		inputAction.started += action;
		inputAction.performed += action;
		inputAction.canceled += action;
		unbindCommands.Add(delegate
		{
			inputAction.started -= action;
			inputAction.performed -= action;
			inputAction.canceled -= action;
		});
	}

	internal void SetGame(MiniGame game)
	{
		this.game = game;
	}
}
