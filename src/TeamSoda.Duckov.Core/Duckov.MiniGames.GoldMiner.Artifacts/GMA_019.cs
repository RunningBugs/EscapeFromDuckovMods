namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_019 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.bomb += 3;
		}
	}
}
