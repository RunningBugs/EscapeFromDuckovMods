using ItemStatsSystem;

public class HealAction : EffectAction
{
	private CharacterMainControl _mainControl;

	public int healValue = 10;

	private CharacterMainControl MainControl
	{
		get
		{
			if (_mainControl == null)
			{
				_mainControl = base.Master?.Item?.GetCharacterMainControl();
			}
			return _mainControl;
		}
	}

	protected override void OnTriggered(bool positive)
	{
		if ((bool)MainControl)
		{
			MainControl.Health.AddHealth(healValue);
		}
	}
}
