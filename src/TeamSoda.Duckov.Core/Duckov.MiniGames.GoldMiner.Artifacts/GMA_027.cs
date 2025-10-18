using ItemStatsSystem.Stats;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_027 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.shopRefreshChances.AddModifier(new Modifier(ModifierType.Add, 1f, this));
		}
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.shopRefreshChances.RemoveAllModifiersFromSource(this);
		}
	}
}
