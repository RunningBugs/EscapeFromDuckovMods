using System;
using Duckov.Economy;
using ItemStatsSystem;
using UnityEngine;

[Serializable]
public struct CraftingFormula
{
	[Serializable]
	public struct ItemEntry
	{
		[ItemTypeID]
		public int id;

		public int amount;
	}

	public string id;

	public ItemEntry result;

	public string[] tags;

	[SerializeField]
	public Cost cost;

	public bool unlockByDefault;

	public bool lockInDemo;

	public string requirePerk;

	public bool hideInIndex;

	public bool IDValid => !string.IsNullOrEmpty(id);
}
