using System;
using ItemStatsSystem;

public static class InventoryExtensions
{
	private static void Sort(this Inventory inventory, Comparison<Item> comparison)
	{
		inventory.Content.Sort(comparison);
	}
}
