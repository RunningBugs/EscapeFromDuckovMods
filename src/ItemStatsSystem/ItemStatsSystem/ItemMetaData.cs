using System;
using System.Linq;
using Duckov.Utilities;
using SodaCraft.Localizations;
using UnityEngine;

namespace ItemStatsSystem;

[Serializable]
public struct ItemMetaData
{
	[ItemTypeID]
	public int id;

	public int quality;

	public Tag[] tags;

	[SerializeField]
	private string name;

	[SerializeField]
	private string displayName;

	[SerializeField]
	private string description;

	public int maxStackCount;

	public int defaultStackCount;

	public Sprite icon;

	public DisplayQuality displayQuality;

	public int priceEach;

	public string caliber;

	public string Catagory
	{
		get
		{
			if (tags != null && tags.Length != 0)
			{
				return tags[0].name;
			}
			return "None";
		}
	}

	public string Name => name;

	public string DisplayNameKey => displayName;

	public string DisplayName => displayName.ToPlainText();

	public string Description => description.ToPlainText();

	public ItemMetaData(int id, int quality, Tag[] tags, string name, string displayName, Sprite icon, string caliber = "", string description = "", DisplayQuality displayQuality = DisplayQuality.None, int maxStackCount = 1, int defaultStackCount = 1, int priceEach = 0)
	{
		this.id = id;
		this.quality = quality;
		this.tags = tags;
		this.name = name;
		this.displayName = displayName;
		this.icon = icon;
		this.caliber = caliber;
		this.description = description;
		this.displayQuality = displayQuality;
		this.maxStackCount = maxStackCount;
		this.defaultStackCount = defaultStackCount;
		this.priceEach = priceEach;
	}

	public ItemMetaData(Item from)
	{
		id = from.TypeID;
		quality = from.Quality;
		tags = from.Tags.ToArray();
		name = from.name;
		displayName = from.DisplayNameRaw;
		icon = from.Icon;
		string text = from.Constants.GetString("Caliber", "");
		caliber = text;
		description = from.DescriptionRaw;
		displayQuality = from.DisplayQuality;
		maxStackCount = from.MaxStackCount;
		defaultStackCount = from.StackCount;
		priceEach = from.Value;
	}
}
