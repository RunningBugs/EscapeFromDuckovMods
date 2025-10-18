using System;
using Duckov.Economy;
using ItemStatsSystem;

namespace Duckov.PerkTrees;

[Serializable]
public class PerkRequirement
{
	[Serializable]
	public class RequireItemEntry
	{
		[ItemTypeID]
		public int id = 1;

		public int amount = 1;
	}

	public int level;

	public Cost cost;

	[TimeSpan]
	public long requireTime;

	public TimeSpan RequireTime => TimeSpan.FromTicks(requireTime);

	internal bool AreSatisfied()
	{
		if (level > EXPManager.Level)
		{
			return false;
		}
		if (!cost.Enough)
		{
			return false;
		}
		return true;
	}
}
