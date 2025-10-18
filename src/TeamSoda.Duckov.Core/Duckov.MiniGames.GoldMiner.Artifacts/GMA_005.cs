using System;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_005 : GoldMinerArtifactBehaviour
{
	private int remaining = 3;

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
		if (remaining >= 1 && !(entity == null) && base.Run.IsRock(entity) && entity.size < GoldMinerEntity.Size.M)
		{
			entity.Value += 500;
			remaining--;
		}
	}
}
