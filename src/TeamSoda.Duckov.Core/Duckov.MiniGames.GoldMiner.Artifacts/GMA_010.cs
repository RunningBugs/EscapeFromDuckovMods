namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_010 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.minMoneySum = 1000;
		}
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.minMoneySum = 0;
		}
	}
}
