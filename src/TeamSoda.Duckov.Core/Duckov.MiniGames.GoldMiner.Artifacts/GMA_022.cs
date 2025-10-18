using ItemStatsSystem.Stats;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_022 : GoldMinerArtifactBehaviour
{
	[SerializeField]
	private float amount = 0.1f;

	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.charm.AddModifier(new Modifier(ModifierType.Add, amount, this));
		}
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.charm.RemoveAllModifiersFromSource(this);
		}
	}
}
