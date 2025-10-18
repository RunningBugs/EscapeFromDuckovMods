using Duckov.BlackMarkets;
using UnityEngine;

namespace Duckov.PerkTrees.Behaviours;

public class ChangeBlackMarketRefreshTimeFactor : PerkBehaviour
{
	[SerializeField]
	private float amount = -0.1f;

	protected override void OnAwake()
	{
		base.OnAwake();
		BlackMarket.onRequestRefreshTime += HandleEvent;
	}

	protected override void OnOnDestroy()
	{
		base.OnOnDestroy();
		BlackMarket.onRequestRefreshTime -= HandleEvent;
	}

	private void HandleEvent(BlackMarket.OnRequestRefreshTimeFactorEventContext context)
	{
		if (!(base.Master == null) && base.Master.Unlocked)
		{
			context.Add(amount);
		}
	}
}
