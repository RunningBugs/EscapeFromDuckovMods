using System;
using UnityEngine;

namespace Duckov.Weathers;

[Serializable]
public class Precipitation
{
	[SerializeField]
	private int seed;

	[SerializeField]
	[Range(0f, 1f)]
	private float cloudyThreshold;

	[SerializeField]
	[Range(0f, 1f)]
	private float rainyThreshold;

	[SerializeField]
	private float frequency = 1f;

	[SerializeField]
	private float offset;

	[SerializeField]
	private float contrast = 1f;

	public float CloudyThreshold => cloudyThreshold;

	public float RainyThreshold => rainyThreshold;

	public bool IsRainy(TimeSpan dayAndTime)
	{
		return Get(dayAndTime) > rainyThreshold;
	}

	public bool IsCloudy(TimeSpan dayAndTime)
	{
		return Get(dayAndTime) > cloudyThreshold;
	}

	public float Get(TimeSpan dayAndTime)
	{
		Vector2 perlinNoiseCoord = GetPerlinNoiseCoord(dayAndTime);
		return Mathf.Clamp01(((Mathf.PerlinNoise(perlinNoiseCoord.x, perlinNoiseCoord.y) + Mathf.PerlinNoise(perlinNoiseCoord.x + 0.5f + 123.4f, perlinNoiseCoord.y - 567.8f)) / 2f - 0.5f) * contrast + 0.5f + offset);
	}

	public Vector2 GetPerlinNoiseCoord(TimeSpan dayAndTime)
	{
		float num = (float)(dayAndTime.Days % 3650) * 24f + (float)dayAndTime.Hours + (float)dayAndTime.Minutes / 60f;
		int num2 = dayAndTime.Days / 3650;
		return new Vector2(num * frequency, seed + num2);
	}

	internal void SetSeed(int seed)
	{
		this.seed = seed;
	}

	public float Get()
	{
		TimeSpan now = GameClock.Now;
		return Get(now);
	}

	public bool IsRainy()
	{
		TimeSpan now = GameClock.Now;
		return IsRainy(now);
	}

	public bool IsCloudy()
	{
		TimeSpan now = GameClock.Now;
		return IsCloudy(now);
	}
}
