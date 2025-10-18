using System;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_015 : GoldMinerArtifactBehaviour
{
	[SerializeField]
	private int amount = 20;

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
		if (base.Run != null && base.Run.IsPig(entity))
		{
			entity.Value += amount;
		}
	}
}
