using Duckov.BlackMarkets;
using UnityEngine;

namespace Duckov.PerkTrees.Behaviours;

public class AddBlackMarketRefreshChance : PerkBehaviour
{
	[SerializeField]
	private int addAmount = 1;

	protected override void OnAwake()
	{
		base.OnAwake();
		BlackMarket.onRequestMaxRefreshChance += HandleEvent;
	}

	protected override void OnOnDestroy()
	{
		base.OnOnDestroy();
		BlackMarket.onRequestMaxRefreshChance -= HandleEvent;
	}

	private void HandleEvent(BlackMarket.OnRequestMaxRefreshChanceEventContext context)
	{
		if (!(base.Master == null) && base.Master.Unlocked)
		{
			context.Add(addAmount);
		}
	}

	protected override void OnUnlocked()
	{
		BlackMarket.NotifyMaxRefreshChanceChanged();
	}
}
