using System;
using Saves;
using UnityEngine;

public class GameClock : MonoBehaviour
{
	[Serializable]
	private struct SaveData
	{
		public long days;

		public double secondsOfDay;

		public long realTimePlayedTicks;

		public TimeSpan RealTimePlayed => TimeSpan.FromTicks(realTimePlayedTicks);
	}

	public float clockTimeScale = 60f;

	private long days;

	private double secondsOfDay;

	private TimeSpan realTimePlayed;

	private const double SecondsPerDay = 86300.0;

	public static GameClock Instance { get; private set; }

	private static string SaveKey => "GameClock";

	private TimeSpan RealTimePlayed => realTimePlayed;

	private static double SecondsOfDay
	{
		get
		{
			if (Instance == null)
			{
				return 0.0;
			}
			return Instance.secondsOfDay;
		}
	}

	[TimeSpan]
	private long _TimeOfDayTicks => TimeOfDay.Ticks;

	public static TimeSpan TimeOfDay => TimeSpan.FromSeconds(SecondsOfDay);

	public static long Day
	{
		get
		{
			if (Instance == null)
			{
				return 0L;
			}
			return Instance.days;
		}
	}

	public static TimeSpan Now => TimeOfDay + TimeSpan.FromDays(Day);

	public static int Hour => TimeOfDay.Hours;

	public static int Minut => TimeOfDay.Minutes;

	public static int Seconds => TimeOfDay.Seconds;

	public static int Milliseconds => TimeOfDay.Milliseconds;

	public static event Action OnGameClockStep;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("检测到多个Game Clock");
			return;
		}
		Instance = this;
		SavesSystem.OnCollectSaveData += Save;
		Load();
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= Save;
	}

	private void Save()
	{
		SavesSystem.Save(SaveKey, new SaveData
		{
			days = days,
			secondsOfDay = secondsOfDay,
			realTimePlayedTicks = RealTimePlayed.Ticks
		});
	}

	private void Load()
	{
		SaveData saveData = SavesSystem.Load<SaveData>(SaveKey);
		days = saveData.days;
		secondsOfDay = saveData.secondsOfDay;
		realTimePlayed = saveData.RealTimePlayed;
		GameClock.OnGameClockStep?.Invoke();
	}

	public static TimeSpan GetRealTimePlayedOfSaveSlot(int saveSlot)
	{
		return SavesSystem.Load<SaveData>(SaveKey, saveSlot).RealTimePlayed;
	}

	private void Update()
	{
		StepTime(Time.deltaTime * clockTimeScale);
		realTimePlayed += TimeSpan.FromSeconds(Time.unscaledDeltaTime);
	}

	private void StepTime(float deltaTime)
	{
		secondsOfDay += deltaTime;
		while (secondsOfDay > 86300.0)
		{
			days++;
			secondsOfDay -= 86300.0;
		}
		GameClock.OnGameClockStep?.Invoke();
	}

	public void StepTimeTil(TimeSpan time)
	{
		if (time.Days > 0)
		{
			time = new TimeSpan(time.Hours, time.Minutes, time.Seconds);
		}
		StepTime((float)((!(time > TimeOfDay)) ? (time + TimeSpan.FromDays(1.0) - TimeOfDay) : (time - TimeOfDay)).TotalSeconds);
	}

	internal static void Step(float seconds)
	{
		if (!(Instance == null))
		{
			Instance.StepTime(seconds);
		}
	}
}
