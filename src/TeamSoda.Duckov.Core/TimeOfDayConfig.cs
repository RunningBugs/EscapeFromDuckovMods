using System;
using Duckov.Weathers;
using UnityEngine;
using UnityEngine.Rendering;

public class TimeOfDayConfig : MonoBehaviour
{
	[SerializeField]
	private TimeOfDayEntry defaultEntry;

	[SerializeField]
	private TimeOfDayEntry cloudyEntry;

	[SerializeField]
	private TimeOfDayEntry rainyEntry;

	[SerializeField]
	private TimeOfDayEntry stormIEntry;

	[SerializeField]
	private TimeOfDayEntry stormIIEntry;

	public bool forceSetTime;

	[Range(0f, 24f)]
	public int forceSetTimeTo = 8;

	public bool forceSetWeather;

	public Weather forceSetWeatherTo;

	[SerializeField]
	private Volume lookDevVolume;

	[SerializeField]
	private TimePhaseTags debugPhase;

	[SerializeField]
	private Weather debugWeather;

	public TimeOfDayEntry GetCurrentEntry(Weather weather)
	{
		return weather switch
		{
			Weather.Sunny => defaultEntry, 
			Weather.Cloudy => cloudyEntry, 
			Weather.Rainy => rainyEntry, 
			Weather.Stormy_I => stormIEntry, 
			Weather.Stormy_II => stormIIEntry, 
			_ => defaultEntry, 
		};
	}

	public void InvokeDebug()
	{
		TimeOfDayEntry currentEntry = GetCurrentEntry(debugWeather);
		if (!currentEntry)
		{
			Debug.Log("No entry found");
			return;
		}
		TimeOfDayPhase phase = currentEntry.GetPhase(debugPhase);
		if (!Application.isPlaying)
		{
			if ((bool)lookDevVolume && lookDevVolume.profile != phase.volumeProfile)
			{
				lookDevVolume.profile = phase.volumeProfile;
			}
			return;
		}
		int num = 9;
		num = debugPhase switch
		{
			TimePhaseTags.day => 9, 
			TimePhaseTags.dawn => 17, 
			TimePhaseTags.night => 22, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		WeatherManager.SetForceWeather(forceWeather: true, debugWeather);
		TimeSpan time = new TimeSpan(num, 10, 0);
		GameClock.Instance.StepTimeTil(time);
		Debug.Log($"Set Weather to {debugWeather},and time to {num}");
	}
}
