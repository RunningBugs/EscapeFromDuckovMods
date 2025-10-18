using System;
using UnityEngine;

namespace Duckov.Crops;

[Serializable]
public struct CropData
{
	public string gardenID;

	public Vector2Int coord;

	public string cropID;

	public int score;

	public bool watered;

	[TimeSpan]
	public long growTicks;

	[DateTime]
	public long lastUpdateDateTimeRaw;

	public ProductRanking Ranking
	{
		get
		{
			if (score < 33)
			{
				return ProductRanking.Poor;
			}
			if (score < 66)
			{
				return ProductRanking.Normal;
			}
			return ProductRanking.Good;
		}
	}

	public TimeSpan GrowTime => TimeSpan.FromTicks(growTicks);

	public DateTime LastUpdateDateTime
	{
		get
		{
			return DateTime.FromBinary(lastUpdateDateTimeRaw);
		}
		set
		{
			lastUpdateDateTimeRaw = value.ToBinary();
		}
	}
}
