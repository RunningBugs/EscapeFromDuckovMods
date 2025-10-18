using System;
using Duckov.Economy;
using ItemStatsSystem;

[Serializable]
public struct DecomposeFormula
{
	[ItemTypeID]
	public int item;

	public bool valid;

	public Cost result;
}
