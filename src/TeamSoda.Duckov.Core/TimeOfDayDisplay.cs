using System;
using Duckov.Weathers;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

public class TimeOfDayDisplay : MonoBehaviour
{
	private TimePhaseTags currentPhaseTag;

	private Weather currentWeather;

	public TextMeshProUGUI phaseText;

	public TextMeshProUGUI weatherText;

	public TextMeshProUGUI stormTitleText;

	public TextMeshProUGUI stormText;

	[LocalizationKey("Default")]
	public string StormComingETAKey = "StormETA";

	[LocalizationKey("Default")]
	public string StormComingOneDayKey = "StormOneDayETA";

	[LocalizationKey("Default")]
	public string StormPhaseIIETAKey = "StormPhaseIIETA";

	[LocalizationKey("Default")]
	public string StormOverETAKey = "StormOverETA";

	public GameObject stormDescObject;

	private float refreshTimeSpace = 0.5f;

	private float refreshTimer;

	public Animator stormIndicatorAnimator;

	public ProceduralImage stormFillImage;

	private void Start()
	{
		RefreshPhase(TimeOfDayController.Instance.CurrentPhase.timePhaseTag);
		RefreshWeather(TimeOfDayController.Instance.CurrentWeather);
	}

	private void Update()
	{
		refreshTimer -= Time.unscaledDeltaTime;
		if (!(refreshTimer > 0f))
		{
			refreshTimer = refreshTimeSpace;
			TimePhaseTags timePhaseTag = TimeOfDayController.Instance.CurrentPhase.timePhaseTag;
			if (currentPhaseTag != timePhaseTag)
			{
				RefreshPhase(timePhaseTag);
			}
			Weather weather = TimeOfDayController.Instance.CurrentWeather;
			if (currentWeather != weather)
			{
				RefreshWeather(weather);
			}
			RefreshStormText(weather);
		}
	}

	private void RefreshStormText(Weather _weather)
	{
		TimeSpan timeSpan = default(TimeSpan);
		float num = 0f;
		switch (_weather)
		{
		case Weather.Stormy_I:
			stormIndicatorAnimator.SetBool("Grow", value: false);
			stormTitleText.text = StormPhaseIIETAKey.ToPlainText();
			timeSpan = WeatherManager.Instance.Storm.GetStormIOverETA(GameClock.Now);
			num = WeatherManager.Instance.Storm.GetStormRemainPercent(GameClock.Now);
			stormDescObject.SetActive(LevelManager.Instance.IsBaseLevel);
			break;
		case Weather.Stormy_II:
			stormIndicatorAnimator.SetBool("Grow", value: false);
			stormTitleText.text = StormOverETAKey.ToPlainText();
			timeSpan = WeatherManager.Instance.Storm.GetStormIIOverETA(GameClock.Now);
			num = WeatherManager.Instance.Storm.GetStormRemainPercent(GameClock.Now);
			stormDescObject.SetActive(LevelManager.Instance.IsBaseLevel);
			break;
		default:
			stormIndicatorAnimator.SetBool("Grow", value: true);
			num = WeatherManager.Instance.Storm.GetSleepPercent(GameClock.Now);
			timeSpan = WeatherManager.Instance.Storm.GetStormETA(GameClock.Now);
			if (timeSpan.TotalHours < 24.0)
			{
				stormTitleText.text = StormComingOneDayKey.ToPlainText();
				stormDescObject.SetActive(LevelManager.Instance.IsBaseLevel);
			}
			else
			{
				stormTitleText.text = StormComingETAKey.ToPlainText();
				stormDescObject.SetActive(value: false);
			}
			break;
		}
		stormFillImage.fillAmount = num;
		stormText.text = $"{Mathf.FloorToInt((float)timeSpan.TotalHours):000}:{timeSpan.Minutes:00}";
	}

	private void RefreshPhase(TimePhaseTags _phase)
	{
		currentPhaseTag = _phase;
		phaseText.text = TimeOfDayController.GetTimePhaseNameByPhaseTag(_phase);
	}

	private void RefreshWeather(Weather _weather)
	{
		currentWeather = _weather;
		weatherText.text = TimeOfDayController.GetWeatherNameByWeather(_weather);
	}
}
