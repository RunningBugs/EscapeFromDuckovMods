using System;

public class ScrollWheelOptionsProvider : OptionsProviderBase
{
	public override string Key => "Input_ScrollWheelBehaviour";

	public override string GetCurrentOption()
	{
		return ScrollWheelBehaviour.GetDisplayName(ScrollWheelBehaviour.CurrentBehaviour);
	}

	public override string[] GetOptions()
	{
		ScrollWheelBehaviour.Behaviour[] array = (ScrollWheelBehaviour.Behaviour[])Enum.GetValues(typeof(ScrollWheelBehaviour.Behaviour));
		string[] array2 = new string[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = ScrollWheelBehaviour.GetDisplayName(array[i]);
		}
		return array2;
	}

	public override void Set(int index)
	{
		ScrollWheelBehaviour.CurrentBehaviour = ((ScrollWheelBehaviour.Behaviour[])Enum.GetValues(typeof(ScrollWheelBehaviour.Behaviour)))[index];
	}
}
