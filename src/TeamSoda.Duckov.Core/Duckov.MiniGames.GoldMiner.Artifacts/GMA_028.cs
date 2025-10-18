namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_028 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.shopTicket++;
		}
	}
}
