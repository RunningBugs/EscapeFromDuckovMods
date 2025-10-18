namespace Duckov.UI.BarDisplays;

public class BarDisplayController_HP : BarDisplayController
{
	private CharacterMainControl _target;

	protected override float Current
	{
		get
		{
			if (Target == null)
			{
				return 0f;
			}
			return Target.Health.CurrentHealth;
		}
	}

	protected override float Max
	{
		get
		{
			if (Target == null)
			{
				return 0f;
			}
			return Target.Health.MaxHealth;
		}
	}

	private CharacterMainControl Target
	{
		get
		{
			if (_target == null)
			{
				_target = CharacterMainControl.Main;
			}
			return _target;
		}
	}

	private void OnEnable()
	{
		Refresh();
		RegisterEvents();
	}

	private void OnDisable()
	{
		UnregisterEvents();
	}

	private void RegisterEvents()
	{
		if (!(Target == null))
		{
			Target.Health.OnHealthChange.AddListener(OnHealthChange);
		}
	}

	private void UnregisterEvents()
	{
		if (!(Target == null))
		{
			Target.Health.OnHealthChange.RemoveListener(OnHealthChange);
		}
	}

	private void OnHealthChange(Health health)
	{
		Refresh();
	}
}
