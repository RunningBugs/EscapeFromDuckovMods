using System;
using Duckov;
using Duckov.Scenes;
using Duckov.UI;
using Duckov.Weathers;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.Serialization;

public class TimeOfDayController : MonoBehaviour
{
	private TimeOfDayConfig config;

	private bool atNight;

	[FormerlySerializedAs("volumeControl")]
	[SerializeField]
	private TimeOfDayVolumeControl weatherVolumeControl;

	private TimeOfDayPhase currentPhase;

	private Weather currentWeather;

	public float morningStart = 5f;

	public float dawnStart = 16f;

	public float nightStart = 19f;

	public static float NightViewAngleFactor;

	public static float NightViewDistanceFactor;

	public static float NightSenseRangeFactor;

	[LocalizationKey("Default")]
	public string timePhaseKey_Day;

	[LocalizationKey("Default")]
	public string timePhaseKey_Dawn;

	[LocalizationKey("Default")]
	public string timePhaseKey_Night;

	[LocalizationKey("Default")]
	public string WeatherKey_Sunny;

	[LocalizationKey("Default")]
	public string WeatherKey_Cloudy;

	[LocalizationKey("Default")]
	public string WeatherKey_Rainy;

	[LocalizationKey("Default")]
	public string WeatherKey_Storm_I;

	[LocalizationKey("Default")]
	public string WeatherKey_Storm_II;

	private string stormPhaseISoundKey = "Music/Stinger/stg_storm_1";

	private string stormPhaseIISoundKey = "Music/Stinger/stg_storm_2";

	public GameObject stormIObject;

	public GameObject stormIIObject;

	private float time;

	public static TimeOfDayController Instance
	{
		get
		{
			if (!LevelManager.Instance)
			{
				return null;
			}
			return LevelManager.Instance.TimeOfDayController;
		}
	}

	public bool AtNight => atNight;

	public TimeOfDayPhase CurrentPhase => currentPhase;

	public Weather CurrentWeather => currentWeather;

	public float Time => time;

	private void Start()
	{
		config = LevelConfig.Instance.timeOfDayConfig;
		if (config.forceSetTime)
		{
			TimeSpan timeSpan = new TimeSpan(0, config.forceSetTimeTo, 0, 0);
			GameClock.Instance.StepTimeTil(timeSpan);
		}
		if (config.forceSetWeather)
		{
			WeatherManager.SetForceWeather(forceWeather: true, config.forceSetWeatherTo);
		}
		time = (float)GameClock.TimeOfDay.TotalHours % 24f;
		TimePhaseTags timePhaseTagByTime = GetTimePhaseTagByTime(time);
		atNight = timePhaseTagByTime == TimePhaseTags.night;
		currentWeather = WeatherManager.GetWeather();
		OnWeatherChanged(currentWeather);
		currentPhase = config.GetCurrentEntry(CurrentWeather).GetPhase(timePhaseTagByTime);
		weatherVolumeControl.ForceSetProfile(currentPhase.volumeProfile);
	}

	private void Update()
	{
		time = (float)GameClock.TimeOfDay.TotalHours % 24f;
		TimePhaseTags timePhaseTagByTime = GetTimePhaseTagByTime(time);
		atNight = timePhaseTagByTime == TimePhaseTags.night;
		Weather weather = WeatherManager.GetWeather();
		if (weather != currentWeather)
		{
			currentWeather = weather;
			OnWeatherChanged(currentWeather);
		}
		currentPhase = config.GetCurrentEntry(CurrentWeather).GetPhase(timePhaseTagByTime);
		if (weatherVolumeControl.CurrentProfile != currentPhase.volumeProfile && weatherVolumeControl.BufferTargetProfile != currentPhase.volumeProfile)
		{
			weatherVolumeControl.SetTargetProfile(currentPhase.volumeProfile);
		}
	}

	private void OnWeatherChanged(Weather newWeather)
	{
		bool flag = false;
		if ((bool)MultiSceneCore.Instance)
		{
			SubSceneEntry subSceneInfo = MultiSceneCore.Instance.GetSubSceneInfo();
			if (subSceneInfo != null)
			{
				flag = subSceneInfo.IsInDoor;
			}
		}
		switch (newWeather)
		{
		case Weather.Stormy_I:
			stormIObject.SetActive(value: true);
			stormIIObject.SetActive(value: false);
			NotificationText.Push("Weather_Storm_I".ToPlainText());
			if (!flag && LevelManager.AfterInit)
			{
				AudioManager.Post(stormPhaseISoundKey, base.gameObject);
			}
			break;
		case Weather.Stormy_II:
			stormIObject.SetActive(value: false);
			stormIIObject.SetActive(value: true);
			NotificationText.Push("Weather_Storm_II".ToPlainText());
			if (!flag && LevelManager.AfterInit)
			{
				AudioManager.Post(stormPhaseIISoundKey, base.gameObject);
			}
			break;
		default:
			stormIObject.SetActive(value: false);
			stormIIObject.SetActive(value: false);
			break;
		}
	}

	private TimePhaseTags GetTimePhaseTagByTime(float hourTime)
	{
		hourTime %= 24f;
		if (hourTime < morningStart || hourTime >= nightStart)
		{
			return TimePhaseTags.night;
		}
		if (hourTime >= morningStart && hourTime < dawnStart)
		{
			return TimePhaseTags.day;
		}
		if (hourTime >= dawnStart && hourTime < nightStart)
		{
			return TimePhaseTags.dawn;
		}
		return TimePhaseTags.day;
	}

	public static string GetTimePhaseNameByPhaseTag(TimePhaseTags phaseTag)
	{
		TimeOfDayController instance = Instance;
		if (!instance)
		{
			return string.Empty;
		}
		return phaseTag switch
		{
			TimePhaseTags.day => instance.timePhaseKey_Day.ToPlainText(), 
			TimePhaseTags.dawn => instance.timePhaseKey_Dawn.ToPlainText(), 
			TimePhaseTags.night => instance.timePhaseKey_Night.ToPlainText(), 
			_ => instance.timePhaseKey_Day.ToPlainText(), 
		};
	}

	public static string GetWeatherNameByWeather(Weather weather)
	{
		TimeOfDayController instance = Instance;
		if (!instance)
		{
			return string.Empty;
		}
		return weather switch
		{
			Weather.Sunny => instance.WeatherKey_Sunny.ToPlainText(), 
			Weather.Rainy => instance.WeatherKey_Rainy.ToPlainText(), 
			Weather.Cloudy => instance.WeatherKey_Cloudy.ToPlainText(), 
			Weather.Stormy_I => instance.WeatherKey_Storm_I.ToPlainText(), 
			Weather.Stormy_II => instance.WeatherKey_Storm_II.ToPlainText(), 
			_ => instance.WeatherKey_Sunny.ToPlainText(), 
		};
	}
}
