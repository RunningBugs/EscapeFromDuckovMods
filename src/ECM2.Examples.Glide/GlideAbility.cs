using UnityEngine;

namespace ECM2.Examples.Glide;

public class GlideAbility : MonoBehaviour
{
	public bool canEverGlide = true;

	public float maxFallSpeedGliding = 1f;

	private Character _character;

	protected bool _glideInputPressed;

	protected bool _isGliding;

	public bool glideInputPressed => _glideInputPressed;

	public virtual bool IsGliding()
	{
		return _isGliding;
	}

	public virtual void Glide()
	{
		_glideInputPressed = true;
	}

	public virtual void StopGliding()
	{
		_glideInputPressed = false;
	}

	protected virtual bool IsGlideAllowed()
	{
		if (canEverGlide)
		{
			return _character.IsFalling();
		}
		return false;
	}

	protected virtual bool CanGlide()
	{
		bool flag = IsGlideAllowed();
		if (flag)
		{
			Vector3 rhs = -_character.GetGravityDirection();
			flag = Vector3.Dot(_character.GetVelocity(), rhs) < 0f;
		}
		return flag;
	}

	protected virtual void CheckGlideInput()
	{
		if (!_isGliding && _glideInputPressed && CanGlide())
		{
			_isGliding = true;
			_character.maxFallSpeed = maxFallSpeedGliding;
		}
		else if (_isGliding && (!_glideInputPressed || !CanGlide()))
		{
			_isGliding = false;
			_character.maxFallSpeed = 40f;
		}
	}

	private void OnBeforeCharacterSimulationUpdated(float deltaTime)
	{
		CheckGlideInput();
	}

	private void Awake()
	{
		_character = GetComponent<Character>();
	}

	private void OnEnable()
	{
		_character.BeforeSimulationUpdated += OnBeforeCharacterSimulationUpdated;
	}

	private void OnDisable()
	{
		_character.BeforeSimulationUpdated -= OnBeforeCharacterSimulationUpdated;
	}
}
