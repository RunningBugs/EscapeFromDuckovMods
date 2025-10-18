using System;
using Saves;
using UnityEngine;

namespace Duckov.Weathers;

public class WeatherManager : MonoBehaviour
{
	[Serializable]
	private struct SaveData
	{
		public bool valid;

		public int seed;

		public SaveData(WeatherManager weatherManager)
		{
			this = default(SaveData);
			seed = weatherManager.seed;
			valid = true;
		}

		internal void Setup(WeatherManager weatherManager)
		{
			weatherManager.seed = seed;
		}
	}

	private int seed = -1;

	[SerializeField]
	private Storm storm = new Storm();

	[SerializeField]
	private Precipitation precipitation = new Precipitation();

	private const string SaveKey = "WeatherManagerData";

	private Weather _cachedWeather;

	private TimeSpan _cachedDayAndTime;

	private bool _weatherDirty;

	public static WeatherManager Instance { get; private set; }

	public bool ForceWeather { get; set; }

	public Weather ForceWeatherValue { get; set; }

	public Storm Storm => storm;

	private void Awake()
	{
		Instance = this;
		SavesSystem.OnCollectSaveData += Save;
		Load();
		_weatherDirty = true;
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= Save;
	}

	private void Save()
	{
		SavesSystem.Save("WeatherManagerData", new SaveData(this));
	}

	private void Load()
	{
		SaveData saveData = SavesSystem.Load<SaveData>("WeatherManagerData");
		if (!saveData.valid)
		{
			SetRandomKey();
		}
		else
		{
			saveData.Setup(this);
		}
		SetupModules();
	}

	private void SetRandomKey()
	{
		seed = UnityEngine.Random.Range(0, 100000);
	}

	private void SetupModules()
	{
		precipitation.SetSeed(seed);
	}

	private Weather M_GetWeather(TimeSpan dayAndTime)
	{
		if (ForceWeather)
		{
			return ForceWeatherValue;
		}
		if (!_weatherDirty && dayAndTime == _cachedDayAndTime)
		{
			return _cachedWeather;
		}
		int stormLevel = storm.GetStormLevel(dayAndTime);
		Weather weather;
		if (stormLevel > 0)
		{
			weather = ((stormLevel != 1) ? Weather.Stormy_II : Weather.Stormy_I);
		}
		else
		{
			float num = precipitation.Get(dayAndTime);
			weather = ((num > precipitation.RainyThreshold) ? Weather.Rainy : ((num > precipitation.CloudyThreshold) ? Weather.Cloudy : Weather.Sunny));
		}
		_cachedDayAndTime = dayAndTime;
		_cachedWeather = weather;
		_weatherDirty = false;
		return weather;
	}

	private void M_SetForceWeather(bool forceWeather, Weather value = Weather.Sunny)
	{
		ForceWeather = forceWeather;
		ForceWeatherValue = value;
	}

	public static Weather GetWeather(TimeSpan dayAndTime)
	{
		if (Instance == null)
		{
			return Weather.Sunny;
		}
		return Instance.M_GetWeather(dayAndTime);
	}

	public static Weather GetWeather()
	{
		TimeSpan now = GameClock.Now;
		if ((bool)Instance && Instance.ForceWeather)
		{
			return Instance.ForceWeatherValue;
		}
		return GetWeather(now);
	}

	public static void SetForceWeather(bool forceWeather, Weather value = Weather.Sunny)
	{
		if (!(Instance == null))
		{
			Instance.M_SetForceWeather(forceWeather, value);
		}
	}
}
