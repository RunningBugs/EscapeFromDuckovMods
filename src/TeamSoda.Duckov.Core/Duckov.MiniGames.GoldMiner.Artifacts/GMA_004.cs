using System;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_004 : GoldMinerArtifactBehaviour
{
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
		if (!(entity == null) && base.Run.IsRock(entity) && entity.size > GoldMinerEntity.Size.M)
		{
			base.Run.levelScoreFactor += 0.3f;
		}
	}
}
