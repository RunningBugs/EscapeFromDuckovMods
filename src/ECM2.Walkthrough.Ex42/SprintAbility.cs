using UnityEngine;

namespace ECM2.Walkthrough.Ex42;

public class SprintAbility : MonoBehaviour
{
	[Space(15f)]
	public float maxSprintSpeed = 10f;

	private Character _character;

	private bool _isSprinting;

	private bool _sprintInputPressed;

	private float _cachedMaxWalkSpeed;

	public void Sprint()
	{
		_sprintInputPressed = true;
	}

	public void StopSprinting()
	{
		_sprintInputPressed = false;
	}

	public bool IsSprinting()
	{
		return _isSprinting;
	}

	private bool CanSprint()
	{
		if (_character.IsWalking())
		{
			return !_character.IsCrouched();
		}
		return false;
	}

	private void CheckSprintInput()
	{
		if (!_isSprinting && _sprintInputPressed && CanSprint())
		{
			_isSprinting = true;
			_cachedMaxWalkSpeed = _character.maxWalkSpeed;
			_character.maxWalkSpeed = maxSprintSpeed;
		}
		else if (_isSprinting && (!_sprintInputPressed || !CanSprint()))
		{
			_isSprinting = false;
			_character.maxWalkSpeed = _cachedMaxWalkSpeed;
		}
	}

	private void OnBeforeSimulationUpdated(float deltaTime)
	{
		CheckSprintInput();
	}

	private void Awake()
	{
		_character = GetComponent<Character>();
	}

	private void OnEnable()
	{
		_character.BeforeSimulationUpdated += OnBeforeSimulationUpdated;
	}

	private void OnDisable()
	{
		_character.BeforeSimulationUpdated -= OnBeforeSimulationUpdated;
	}
}
