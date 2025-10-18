using UnityEngine;

namespace ECM2.Walkthrough.Ex33;

public class RootMotionToggle : MonoBehaviour
{
	private Character _character;

	private void OnMovementModeChanged(Character.MovementMode prevMovementMode, int prevCustomMovementMode)
	{
		_character.useRootMotion = _character.IsWalking();
	}

	private void Awake()
	{
		_character = GetComponent<Character>();
	}

	private void OnEnable()
	{
		_character.MovementModeChanged += OnMovementModeChanged;
	}

	private void OnDisable()
	{
		_character.MovementModeChanged -= OnMovementModeChanged;
	}
}
