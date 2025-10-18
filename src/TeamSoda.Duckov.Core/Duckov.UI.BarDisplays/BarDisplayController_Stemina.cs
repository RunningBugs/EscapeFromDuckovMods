namespace Duckov.UI.BarDisplays;

public class BarDisplayController_Stemina : BarDisplayController
{
	private CharacterMainControl _target;

	private float displayingStemina = -1f;

	private float displayingMaxStemina = -1f;

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

	protected override float Current
	{
		get
		{
			if (Target == null)
			{
				return base.Current;
			}
			return Target.CurrentStamina;
		}
	}

	protected override float Max
	{
		get
		{
			if (Target == null)
			{
				return base.Max;
			}
			return Target.MaxStamina;
		}
	}

	private void Update()
	{
		float current = Current;
		float max = Max;
		if (displayingStemina != current || displayingMaxStemina != max)
		{
			Refresh();
			displayingStemina = current;
			displayingMaxStemina = max;
		}
	}
}
