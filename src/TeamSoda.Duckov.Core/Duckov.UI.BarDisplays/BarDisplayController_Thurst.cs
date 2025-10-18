namespace Duckov.UI.BarDisplays;

public class BarDisplayController_Thurst : BarDisplayController
{
	private CharacterMainControl _target;

	private float displayingCurrent = -1f;

	private float displayingMax = -1f;

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
			return Target.CurrentWater;
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
			return Target.MaxWater;
		}
	}

	private void Update()
	{
		float current = Current;
		float max = Max;
		if (displayingCurrent != current || displayingMax != max)
		{
			Refresh();
			displayingCurrent = current;
			displayingMax = max;
		}
	}
}
