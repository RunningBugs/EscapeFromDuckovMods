using System;
using ItemStatsSystem;
using UnityEngine;

namespace Duckov.Crops;

[Serializable]
public struct CropInfo
{
	public string id;

	public GameObject displayPrefab;

	[ItemTypeID]
	public int resultPoor;

	[ItemTypeID]
	public int resultNormal;

	[ItemTypeID]
	public int resultGood;

	private ItemMetaData? _normalMetaData;

	public int resultAmount;

	[TimeSpan]
	public long totalGrowTicks;

	public string DisplayName
	{
		get
		{
			if (!_normalMetaData.HasValue)
			{
				_normalMetaData = ItemAssetsCollection.GetMetaData(resultNormal);
			}
			return _normalMetaData.Value.DisplayName;
		}
	}

	public TimeSpan GrowTime => TimeSpan.FromTicks(totalGrowTicks);

	public int GetProduct(ProductRanking ranking)
	{
		int num = 0;
		switch (ranking)
		{
		case ProductRanking.Poor:
			num = resultPoor;
			break;
		case ProductRanking.Normal:
			num = resultNormal;
			break;
		case ProductRanking.Good:
			num = resultGood;
			break;
		}
		if (num == 0)
		{
			if (resultNormal != 0)
			{
				return resultNormal;
			}
			if (resultPoor != 0)
			{
				return resultPoor;
			}
		}
		return num;
	}
}
