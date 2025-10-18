using UnityEngine;

namespace ItemStatsSystem;

public class LogWhenUsed : UsageBehavior
{
	public override bool CanBeUsed(Item item, object user)
	{
		return true;
	}

	protected override void OnUse(Item item, object user)
	{
		Debug.Log(item.name);
	}
}
