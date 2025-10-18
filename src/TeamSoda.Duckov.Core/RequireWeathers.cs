using System.Collections.Generic;
using Duckov.Quests;
using Duckov.Weathers;

public class RequireWeathers : Condition
{
	public List<Weather> weathers;

	public override bool Evaluate()
	{
		if (!LevelManager.LevelInited)
		{
			return false;
		}
		Weather currentWeather = LevelManager.Instance.TimeOfDayController.CurrentWeather;
		return weathers.Contains(currentWeather);
	}
}
