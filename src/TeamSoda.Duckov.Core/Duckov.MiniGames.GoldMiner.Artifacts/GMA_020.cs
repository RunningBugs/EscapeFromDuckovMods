namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_020 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.strengthPotion += 3;
		}
	}
}
