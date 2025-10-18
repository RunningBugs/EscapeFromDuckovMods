namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_029 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null && base.Run.shopCapacity < 6)
		{
			base.Run.shopCapacity++;
		}
	}
}
