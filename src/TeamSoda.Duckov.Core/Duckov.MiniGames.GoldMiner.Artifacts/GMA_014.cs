using System;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_014 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (!(base.GoldMiner == null))
		{
			GoldMiner goldMiner = base.GoldMiner;
			goldMiner.onAfterResolveEntity = (Action<GoldMiner, GoldMinerEntity>)Delegate.Combine(goldMiner.onAfterResolveEntity, new Action<GoldMiner, GoldMinerEntity>(OnAfterResolveEntity));
		}
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (!(base.GoldMiner == null))
		{
			GoldMiner goldMiner = base.GoldMiner;
			goldMiner.onAfterResolveEntity = (Action<GoldMiner, GoldMinerEntity>)Delegate.Remove(goldMiner.onAfterResolveEntity, new Action<GoldMiner, GoldMinerEntity>(OnAfterResolveEntity));
		}
	}

	private void OnAfterResolveEntity(GoldMiner miner, GoldMinerEntity entity)
	{
		if (!(entity == null) && base.Run != null && base.Run.IsPig(entity))
		{
			base.Run.stamina += 2f;
		}
	}
}
