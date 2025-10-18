using UnityEngine;

namespace Duckov.DeathLotteries;

public class DeathLotteryInteractable : InteractableBase
{
	[SerializeField]
	private DeathLottery deathLottery;

	protected override bool IsInteractable()
	{
		if (deathLottery == null)
		{
			return false;
		}
		if (!deathLottery.CurrentStatus.valid)
		{
			return false;
		}
		if (deathLottery.Loading)
		{
			return false;
		}
		return true;
	}

	protected override void OnInteractFinished()
	{
		deathLottery.RequestUI();
	}
}
