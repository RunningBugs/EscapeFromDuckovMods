using System;
using Duckov.Utilities;
using ItemStatsSystem;

namespace Duckov.Crops;

[Serializable]
public struct SeedInfo
{
	[ItemTypeID]
	public int itemTypeID;

	public RandomContainer<string> cropIDs;

	public string GetRandomCropID()
	{
		return cropIDs.GetRandom();
	}
}
