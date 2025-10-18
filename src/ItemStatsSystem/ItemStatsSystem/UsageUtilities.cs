using System;
using System.Collections.Generic;
using UnityEngine;

namespace ItemStatsSystem;

public class UsageUtilities : ItemComponent
{
	[SerializeField]
	private float useTime;

	public List<UsageBehavior> behaviors = new List<UsageBehavior>();

	public bool hasSound;

	public string actionSound;

	public string useSound;

	public bool useDurability;

	public int durabilityUsage = 1;

	public float UseTime => useTime;

	public static event Action<Item> OnItemUsedStaticEvent;

	public bool IsUsable(Item item, object user)
	{
		if (!item)
		{
			return false;
		}
		if (useDurability && item.Durability < (float)durabilityUsage)
		{
			return false;
		}
		foreach (UsageBehavior behavior in behaviors)
		{
			if (!(behavior == null) && behavior.CanBeUsed(item, user))
			{
				return true;
			}
		}
		return false;
	}

	public void Use(Item item, object user)
	{
		foreach (UsageBehavior behavior in behaviors)
		{
			if (!(behavior == null) && behavior.CanBeUsed(item, user))
			{
				behavior.Use(item, user);
			}
		}
		if (useDurability && item.Durability > 0f)
		{
			item.Durability -= durabilityUsage;
		}
		UsageUtilities.OnItemUsedStaticEvent?.Invoke(item);
	}
}
