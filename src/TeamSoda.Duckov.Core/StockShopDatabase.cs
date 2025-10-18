using System;
using System.Collections.Generic;
using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;

[CreateAssetMenu(menuName = "Duckov/Stock Shop Database")]
public class StockShopDatabase : ScriptableObject
{
	[Serializable]
	public class MerchantProfile
	{
		public string merchantID;

		public List<ItemEntry> entries = new List<ItemEntry>();
	}

	[Serializable]
	public class ItemEntry
	{
		[ItemTypeID]
		public int typeID;

		public int maxStock;

		public bool forceUnlock;

		public float priceFactor;

		public float possibility;

		public bool lockInDemo;
	}

	public List<MerchantProfile> merchantProfiles;

	public static StockShopDatabase Instance => GameplayDataSettings.StockshopDatabase;

	public MerchantProfile GetMerchantProfile(string merchantID)
	{
		return merchantProfiles.Find((MerchantProfile e) => e.merchantID == merchantID);
	}
}
