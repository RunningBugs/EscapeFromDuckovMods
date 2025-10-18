using System.Linq;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_006 : GoldMinerArtifactBehaviour
{
	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.isGoldPredicators.Add(SmallRockIsGold);
		}
	}

	private bool SmallRockIsGold(GoldMinerEntity entity)
	{
		if (entity.tags.Contains(GoldMinerEntity.Tag.Rock) && entity.size < GoldMinerEntity.Size.M)
		{
			return true;
		}
		return false;
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			base.Run.isGoldPredicators.Remove(SmallRockIsGold);
		}
	}
}
