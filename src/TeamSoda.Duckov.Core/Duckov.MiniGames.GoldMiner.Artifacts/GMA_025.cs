using ItemStatsSystem.Stats;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_025 : GoldMinerArtifactBehaviour
{
	[SerializeField]
	private float addAmount = 1f;

	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.emptySpeed.AddModifier(new Modifier(ModifierType.PercentageAdd, addAmount, this));
		}
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.emptySpeed.RemoveAllModifiersFromSource(this);
		}
	}
}
