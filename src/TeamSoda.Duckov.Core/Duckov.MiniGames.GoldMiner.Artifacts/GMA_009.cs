using System;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_009 : GoldMinerArtifactBehaviour
{
	private bool effectActive;

	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (!(base.GoldMiner == null))
		{
			GoldMiner goldMiner = base.GoldMiner;
			goldMiner.onResolveEntity = (Action<GoldMiner, GoldMinerEntity>)Delegate.Combine(goldMiner.onResolveEntity, new Action<GoldMiner, GoldMinerEntity>(OnResolveEntity));
		}
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (!(base.GoldMiner == null))
		{
			GoldMiner goldMiner = base.GoldMiner;
			goldMiner.onResolveEntity = (Action<GoldMiner, GoldMinerEntity>)Delegate.Remove(goldMiner.onResolveEntity, new Action<GoldMiner, GoldMinerEntity>(OnResolveEntity));
		}
	}

	private void OnResolveEntity(GoldMiner miner, GoldMinerEntity entity)
	{
		if (!(entity == null))
		{
			if (base.Run.IsRock(entity))
			{
				effectActive = true;
			}
			if (effectActive && base.Run.IsGold(entity))
			{
				effectActive = false;
				entity.Value *= 2;
			}
		}
	}
}
