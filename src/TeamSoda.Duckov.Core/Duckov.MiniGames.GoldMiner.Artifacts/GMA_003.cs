using System;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_003 : GoldMinerArtifactBehaviour
{
	private int streak;

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
				Debug.Log("Enity is Rock ", entity);
				streak++;
			}
			else
			{
				streak = 0;
			}
			if (streak > 1)
			{
				base.Run.levelScoreFactor += 0.1f;
			}
		}
	}
}
