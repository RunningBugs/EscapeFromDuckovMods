using System;
using UnityEngine;

namespace Duckov.Weathers;

[Serializable]
public class Storm
{
	[SerializeField]
	[TimeSpan]
	private long offset;

	[SerializeField]
	[TimeSpan]
	private long sleepTime;

	[SerializeField]
	[TimeSpan]
	private long stage1Time;

	[SerializeField]
	[TimeSpan]
	private long stage2Time;

	[TimeSpan]
	private long Period => sleepTime + stage1Time + stage2Time;

	public int GetStormLevel(TimeSpan dayAndTime)
	{
		long num = (dayAndTime.Ticks + offset) % Period;
		if (num < sleepTime)
		{
			return 0;
		}
		if (num < sleepTime + stage1Time)
		{
			return 1;
		}
		return 2;
	}

	public TimeSpan GetStormETA(TimeSpan dayAndTime)
	{
		long num = (dayAndTime.Ticks + offset) % Period;
		if (num < sleepTime)
		{
			return TimeSpan.FromTicks(sleepTime - num);
		}
		return TimeSpan.Zero;
	}

	public TimeSpan GetStormIOverETA(TimeSpan dayAndTime)
	{
		long num = (dayAndTime.Ticks + offset) % Period;
		return TimeSpan.FromTicks(sleepTime + stage1Time - num);
	}

	public TimeSpan GetStormIIOverETA(TimeSpan dayAndTime)
	{
		long num = (dayAndTime.Ticks + offset) % Period;
		return TimeSpan.FromTicks(Period - num);
	}

	public float GetSleepPercent(TimeSpan dayAndTime)
	{
		return (float)((dayAndTime.Ticks + offset) % Period) / (float)sleepTime;
	}

	public float GetStormRemainPercent(TimeSpan dayAndTime)
	{
		long num = (dayAndTime.Ticks + offset) % Period - sleepTime;
		return 1f - (float)num / ((float)stage1Time + (float)stage2Time);
	}
}
