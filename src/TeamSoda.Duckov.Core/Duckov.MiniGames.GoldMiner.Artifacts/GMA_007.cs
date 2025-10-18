using System.Linq;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_007 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.additionalFactorFuncs.Add(AddFactorIfResolved3DifferentKindsOfGold);
		}
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.additionalFactorFuncs.Remove(AddFactorIfResolved3DifferentKindsOfGold);
		}
	}

	private float AddFactorIfResolved3DifferentKindsOfGold()
	{
		if ((from e in base.GoldMiner.resolvedEntities
			where e != null && e.tags.Contains(GoldMinerEntity.Tag.Gold)
			group e by e.size).Count() >= 3)
		{
			return 0.5f;
		}
		return 0f;
	}
}
