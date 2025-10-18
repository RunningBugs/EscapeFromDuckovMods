using System;
using ItemStatsSystem.Stats;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_002 : GoldMinerArtifactBehaviour
{
	private Modifier modifer;

	private bool effectActive;

	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (!(master == null) && !(base.GoldMiner == null))
		{
			modifer = new Modifier(ModifierType.PercentageMultiply, -0.5f, this);
			GoldMiner goldMiner = base.GoldMiner;
			goldMiner.onResolveEntity = (Action<GoldMiner, GoldMinerEntity>)Delegate.Combine(goldMiner.onResolveEntity, new Action<GoldMiner, GoldMinerEntity>(OnResolveEntity));
			GoldMiner goldMiner2 = base.GoldMiner;
			goldMiner2.onHookBeginRetrieve = (Action<GoldMiner, Hook>)Delegate.Combine(goldMiner2.onHookBeginRetrieve, new Action<GoldMiner, Hook>(OnBeginRetrieve));
			GoldMiner goldMiner3 = base.GoldMiner;
			goldMiner3.onHookEndRetrieve = (Action<GoldMiner, Hook>)Delegate.Combine(goldMiner3.onHookEndRetrieve, new Action<GoldMiner, Hook>(OnEndRetrieve));
		}
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (!(base.GoldMiner == null))
		{
			GoldMiner goldMiner = base.GoldMiner;
			goldMiner.onResolveEntity = (Action<GoldMiner, GoldMinerEntity>)Delegate.Remove(goldMiner.onResolveEntity, new Action<GoldMiner, GoldMinerEntity>(OnResolveEntity));
			GoldMiner goldMiner2 = base.GoldMiner;
			goldMiner2.onHookBeginRetrieve = (Action<GoldMiner, Hook>)Delegate.Remove(goldMiner2.onHookBeginRetrieve, new Action<GoldMiner, Hook>(OnBeginRetrieve));
			GoldMiner goldMiner3 = base.GoldMiner;
			goldMiner3.onHookEndRetrieve = (Action<GoldMiner, Hook>)Delegate.Remove(goldMiner3.onHookEndRetrieve, new Action<GoldMiner, Hook>(OnEndRetrieve));
			if (base.Run != null)
			{
				base.Run.staminaDrain.RemoveModifier(modifer);
			}
		}
	}

	private void OnBeginRetrieve(GoldMiner miner, Hook hook)
	{
		if (effectActive)
		{
			base.Run.staminaDrain.AddModifier(modifer);
		}
	}

	private void OnEndRetrieve(GoldMiner miner, Hook hook)
	{
		base.Run.staminaDrain.RemoveModifier(modifer);
		effectActive = false;
	}

	private void OnResolveEntity(GoldMiner miner, GoldMinerEntity entity)
	{
		effectActive = true;
	}
}
