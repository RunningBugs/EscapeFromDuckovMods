using System;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;

public class InventoryFilterProvider : MonoBehaviour
{
	[Serializable]
	public struct FilterEntry
	{
		[LocalizationKey("Default")]
		public string name;

		public Sprite icon;

		public Tag[] requireTags;

		public string DisplayName => name.ToPlainText();

		private bool FilterFunction(Item item)
		{
			if (item == null)
			{
				return false;
			}
			if (requireTags.Length == 0)
			{
				return true;
			}
			Tag[] array = requireTags;
			foreach (Tag tag in array)
			{
				if (!(tag == null) && item.Tags.Contains(tag))
				{
					return true;
				}
			}
			return false;
		}

		public Func<Item, bool> GetFunction()
		{
			if (requireTags.Length == 0)
			{
				return null;
			}
			return FilterFunction;
		}
	}

	public FilterEntry[] entries;
}
