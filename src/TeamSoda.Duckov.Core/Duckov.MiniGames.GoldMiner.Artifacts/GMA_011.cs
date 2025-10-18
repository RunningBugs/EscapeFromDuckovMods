namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_011 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.forceLevelSuccessFuncs.Add(ForceAndDetach);
		}
	}

	private bool ForceAndDetach()
	{
		base.Run.DetachArtifact(master);
		return true;
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.forceLevelSuccessFuncs.Remove(ForceAndDetach);
		}
	}
}
