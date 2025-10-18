using System;
using System.Collections.Generic;
using System.Text;
using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;

namespace Duckov.UI;

public static class ItemUIUtilities
{
	private static ItemDisplay selectedItemDisplay;

	private static bool cacheGunSelected;

	private static int selectedItemTypeID;

	private static ItemMetaData cachedSelectedItemMeta;

	public static ItemDisplay SelectedItemDisplayRaw => selectedItemDisplay;

	public static ItemDisplay SelectedItemDisplay
	{
		get
		{
			if (selectedItemDisplay == null)
			{
				return null;
			}
			if (selectedItemDisplay.Target == null)
			{
				return null;
			}
			return selectedItemDisplay;
		}
		private set
		{
			selectedItemDisplay?.NotifyUnselected();
			selectedItemDisplay = value;
			Item selectedItem = SelectedItem;
			if (selectedItem == null)
			{
				selectedItemTypeID = -1;
			}
			else
			{
				selectedItemTypeID = selectedItem.TypeID;
				cachedSelectedItemMeta = ItemAssetsCollection.GetMetaData(selectedItemTypeID);
				cacheGunSelected = selectedItem.Tags.Contains("Gun");
			}
			selectedItemDisplay?.NotifySelected();
			ItemUIUtilities.OnSelectionChanged?.Invoke();
		}
	}

	public static Item SelectedItem
	{
		get
		{
			if (SelectedItemDisplay == null)
			{
				return null;
			}
			return SelectedItemDisplay.Target;
		}
	}

	public static bool IsGunSelected
	{
		get
		{
			if (SelectedItem == null)
			{
				return false;
			}
			return cacheGunSelected;
		}
	}

	public static string SelectedItemCaliber => cachedSelectedItemMeta.caliber;

	public static event Action OnSelectionChanged;

	public static event Action<Item> OnOrphanRaised;

	public static event Action<Item, bool> OnPutItem;

	public static void Select(ItemDisplay itemDisplay)
	{
		SelectedItemDisplay = itemDisplay;
	}

	public static void RaiseOrphan(Item orphan)
	{
		if (!(orphan == null))
		{
			ItemUIUtilities.OnOrphanRaised?.Invoke(orphan);
			Debug.LogWarning($"游戏中出现了孤儿Item {orphan}。");
		}
	}

	public static void NotifyPutItem(Item item, bool pickup = false)
	{
		ItemUIUtilities.OnPutItem?.Invoke(item, pickup);
	}

	public static string GetPropertiesDisplayText(this Item item)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (item.Variables != null)
		{
			foreach (CustomData variable in item.Variables)
			{
				if (variable.Display)
				{
					stringBuilder.AppendLine(variable.DisplayName + "\t" + variable.GetValueDisplayString());
				}
			}
		}
		if (item.Constants != null)
		{
			foreach (CustomData constant in item.Constants)
			{
				if (constant.Display)
				{
					stringBuilder.AppendLine(constant.DisplayName + "\t" + constant.GetValueDisplayString());
				}
			}
		}
		if (item.Stats != null)
		{
			foreach (Stat stat in item.Stats)
			{
				if (stat.Display)
				{
					stringBuilder.AppendLine($"{stat.DisplayName}\t{stat.Value}");
				}
			}
		}
		if (item.Modifiers != null)
		{
			foreach (ModifierDescription modifier in item.Modifiers)
			{
				if (modifier.Display)
				{
					stringBuilder.AppendLine(modifier.DisplayName + "\t" + modifier.GetDisplayValueString());
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static List<(string name, string value, Polarity polarity)> GetPropertyValueTextPair(this Item item)
	{
		List<(string, string, Polarity)> list = new List<(string, string, Polarity)>();
		if (item.Variables != null)
		{
			foreach (CustomData variable in item.Variables)
			{
				if (variable.Display)
				{
					list.Add((variable.DisplayName, variable.GetValueDisplayString(), Polarity.Neutral));
				}
			}
		}
		if (item.Constants != null)
		{
			foreach (CustomData constant in item.Constants)
			{
				if (constant.Display)
				{
					list.Add((constant.DisplayName, constant.GetValueDisplayString(), Polarity.Neutral));
				}
			}
		}
		if (item.Stats != null)
		{
			foreach (Stat stat in item.Stats)
			{
				if (stat.Display)
				{
					list.Add((stat.DisplayName, stat.Value.ToString(), Polarity.Neutral));
				}
			}
		}
		if (item.Modifiers != null)
		{
			foreach (ModifierDescription modifier in item.Modifiers)
			{
				if (modifier.Display)
				{
					Polarity polarity = StatInfoDatabase.GetPolarity(modifier.Key);
					if (modifier.Value < 0f)
					{
						polarity = (Polarity)(0 - polarity);
					}
					list.Add((modifier.DisplayName, modifier.GetDisplayValueString(), polarity));
				}
			}
		}
		return list;
	}
}
