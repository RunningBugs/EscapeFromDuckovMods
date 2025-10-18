using System;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_008 : GoldMinerArtifactBehaviour
{
	private bool triggered;

	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (!(base.GoldMiner == null))
		{
			GoldMiner goldMiner = base.GoldMiner;
			goldMiner.onLevelBegin = (Action<GoldMiner>)Delegate.Combine(goldMiner.onLevelBegin, new Action<GoldMiner>(OnLevelBegin));
			GoldMiner goldMiner2 = base.GoldMiner;
			goldMiner2.onAfterResolveEntity = (Action<GoldMiner, GoldMinerEntity>)Delegate.Combine(goldMiner2.onAfterResolveEntity, new Action<GoldMiner, GoldMinerEntity>(OnAfterResolveEntity));
		}
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (!(base.GoldMiner == null))
		{
			GoldMiner goldMiner = base.GoldMiner;
			goldMiner.onLevelBegin = (Action<GoldMiner>)Delegate.Remove(goldMiner.onLevelBegin, new Action<GoldMiner>(OnLevelBegin));
			GoldMiner goldMiner2 = base.GoldMiner;
			goldMiner2.onAfterResolveEntity = (Action<GoldMiner, GoldMinerEntity>)Delegate.Remove(goldMiner2.onAfterResolveEntity, new Action<GoldMiner, GoldMinerEntity>(OnAfterResolveEntity));
		}
	}

	private void OnLevelBegin(GoldMiner miner)
	{
		triggered = false;
	}

	private void OnAfterResolveEntity(GoldMiner miner, GoldMinerEntity entity)
	{
		if (!triggered && base.GoldMiner.activeEntities.Count <= 0)
		{
			triggered = true;
			base.Run.charm.BaseValue += 0.5f;
		}
	}
}
