using System;
using ItemStatsSystem;
using UnityEngine;

namespace Fishing;

[Serializable]
internal struct FishingPoolEntry
{
	[SerializeField]
	[ItemTypeID]
	private int id;

	[SerializeField]
	private float weight;

	public int ID => id;

	public float Weight => weight;
}
