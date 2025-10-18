using ItemStatsSystem.Stats;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_016 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.rockValueFactor.AddModifier(new Modifier(ModifierType.Add, 1f, this));
		}
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.rockValueFactor.RemoveAllModifiersFromSource(this);
		}
	}
}
