using System;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_018 : GoldMinerArtifactBehaviour
{
	private int remaining;

	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (!(base.GoldMiner == null))
		{
			GoldMiner goldMiner = base.GoldMiner;
			goldMiner.onLevelBegin = (Action<GoldMiner>)Delegate.Combine(goldMiner.onLevelBegin, new Action<GoldMiner>(OnLevelBegin));
			GoldMiner goldMiner2 = base.GoldMiner;
			goldMiner2.onResolveEntity = (Action<GoldMiner, GoldMinerEntity>)Delegate.Combine(goldMiner2.onResolveEntity, new Action<GoldMiner, GoldMinerEntity>(OnResolveEntity));
		}
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (!(base.GoldMiner == null))
		{
			GoldMiner goldMiner = base.GoldMiner;
			goldMiner.onLevelBegin = (Action<GoldMiner>)Delegate.Remove(goldMiner.onLevelBegin, new Action<GoldMiner>(OnLevelBegin));
			GoldMiner goldMiner2 = base.GoldMiner;
			goldMiner2.onResolveEntity = (Action<GoldMiner, GoldMinerEntity>)Delegate.Remove(goldMiner2.onResolveEntity, new Action<GoldMiner, GoldMinerEntity>(OnResolveEntity));
		}
	}

	private void OnLevelBegin(GoldMiner miner)
	{
		remaining = 5;
	}

	private void OnResolveEntity(GoldMiner miner, GoldMinerEntity entity)
	{
		if ((bool)entity && remaining >= 1)
		{
			remaining--;
			entity.Value = 200;
		}
	}
}
