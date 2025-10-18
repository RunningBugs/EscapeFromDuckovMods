using ItemStatsSystem.Stats;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_026 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.shopRefreshPrice.AddModifier(new Modifier(ModifierType.PercentageMultiply, -1f, this));
		}
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.shopRefreshPrice.RemoveAllModifiersFromSource(this);
		}
	}
}
