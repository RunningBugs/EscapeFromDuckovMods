using ItemStatsSystem.Stats;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_017 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.goldValueFactor.AddModifier(new Modifier(ModifierType.Add, 0.2f, this));
		}
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.goldValueFactor.RemoveAllModifiersFromSource(this);
		}
	}
}
