using System;
using Duckov.UI.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputRebinderIndicator : MonoBehaviour
{
	[SerializeField]
	private FadeGroup fadeGroup;

	private void Awake()
	{
		InputRebinder.OnRebindBegin = (Action<InputAction>)Delegate.Combine(InputRebinder.OnRebindBegin, new Action<InputAction>(OnRebindBegin));
		InputRebinder.OnRebindComplete = (Action<InputAction>)Delegate.Combine(InputRebinder.OnRebindComplete, new Action<InputAction>(OnRebindComplete));
		fadeGroup.SkipHide();
	}

	private void OnRebindComplete(InputAction action)
	{
		fadeGroup.Hide();
	}

	private void OnRebindBegin(InputAction action)
	{
		fadeGroup.Show();
	}
}
