using System;

namespace Duckov.MiniGames.GoldMiner;

[Serializable]
public class ShopEntity
{
	public GoldMinerArtifact artifact;

	public bool locked;

	public bool sold;

	public float priceFactor = 1f;

	public string ID
	{
		get
		{
			if (!artifact)
			{
				return null;
			}
			return artifact.ID;
		}
	}
}
