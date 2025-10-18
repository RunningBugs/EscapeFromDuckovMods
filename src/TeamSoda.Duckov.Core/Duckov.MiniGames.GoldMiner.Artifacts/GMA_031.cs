using UnityEngine;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_031 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.extraGold = Mathf.MoveTowards(base.Run.extraGold, 5f, 1f);
		}
	}
}
