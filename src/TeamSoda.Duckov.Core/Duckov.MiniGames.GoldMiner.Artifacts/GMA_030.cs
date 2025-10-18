using UnityEngine;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_030 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.extraRocks = Mathf.MoveTowards(base.Run.extraRocks, 5f, 1f);
		}
	}
}
