namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_021 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.eagleEyePotion += 3;
		}
	}
}
