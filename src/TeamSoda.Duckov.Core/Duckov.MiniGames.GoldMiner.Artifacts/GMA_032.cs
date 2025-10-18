using UnityEngine;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_032 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.extraDiamond = Mathf.MoveTowards(base.Run.extraDiamond, 5f, 0.5f);
		}
	}
}
