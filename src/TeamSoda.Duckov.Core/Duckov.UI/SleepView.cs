using System;
using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class SleepView : View
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private TextMeshProUGUI willWakeUpAtText;

	[SerializeField]
	private TextMeshProUGUI sleepTimeSpanText;

	[SerializeField]
	private GameObject nextDayIndicator;

	[SerializeField]
	private Button confirmButton;

	private int sleepForMinuts;

	public static Action OnAfterSleep;

	private bool sleeping;

	public static SleepView Instance => View.GetViewInstance<SleepView>();

	private TimeSpan SleepTimeSpan => TimeSpan.FromMinutes(sleepForMinuts);

	private TimeSpan WillWakeUpAt => GameClock.TimeOfDay + SleepTimeSpan;

	private bool WillWakeUpNextDay => WillWakeUpAt.Days > 0;

	protected override void OnOpen()
	{
		base.OnOpen();
		fadeGroup.Show();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	protected override void Awake()
	{
		base.Awake();
		slider.onValueChanged.AddListener(OnSliderValueChanged);
		confirmButton.onClick.AddListener(OnConfirmButtonClicked);
	}

	private void OnConfirmButtonClicked()
	{
		Sleep(sleepForMinuts).Forget();
	}

	private async UniTask Sleep(float minuts)
	{
		if (!sleeping)
		{
			sleeping = true;
			float seconds = minuts * 60f;
			await BlackScreen.ShowAndReturnTask();
			GameClock.Step(seconds);
			await UniTask.WaitForSeconds(0.5f, ignoreTimeScale: true);
			OnAfterSleep?.Invoke();
			if (View.ActiveView == this)
			{
				Close();
			}
			await BlackScreen.HideAndReturnTask();
			sleeping = false;
		}
	}

	private void OnGameClockStep()
	{
		Refresh();
	}

	private void OnEnable()
	{
		InitializeUI();
		GameClock.OnGameClockStep += OnGameClockStep;
	}

	private void OnDisable()
	{
		GameClock.OnGameClockStep -= OnGameClockStep;
	}

	private void OnSliderValueChanged(float newValue)
	{
		sleepForMinuts = Mathf.RoundToInt(newValue);
		Refresh();
	}

	private void InitializeUI()
	{
		slider.SetValueWithoutNotify(sleepForMinuts);
	}

	private void Update()
	{
		Refresh();
	}

	private void Refresh()
	{
		TimeSpan willWakeUpAt = WillWakeUpAt;
		willWakeUpAtText.text = $"{willWakeUpAt.Hours:00}:{willWakeUpAt.Minutes:00}";
		TimeSpan sleepTimeSpan = SleepTimeSpan;
		sleepTimeSpanText.text = $"{(int)sleepTimeSpan.TotalHours:00} h {sleepTimeSpan.Minutes:00} min";
		nextDayIndicator.gameObject.SetActive(willWakeUpAt.Days > 0);
	}

	public static void Show()
	{
		if (!(Instance == null))
		{
			Instance.Open();
		}
	}
}
