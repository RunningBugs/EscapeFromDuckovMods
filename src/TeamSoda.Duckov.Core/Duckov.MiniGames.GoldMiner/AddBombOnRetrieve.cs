using System;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class AddBombOnRetrieve : MiniGameBehaviour
{
	[SerializeField]
	private GoldMinerEntity master;

	[SerializeField]
	private int amount = 1;

	private void Awake()
	{
		if (master == null)
		{
			master = GetComponent<GoldMinerEntity>();
		}
		GoldMinerEntity goldMinerEntity = master;
		goldMinerEntity.OnResolved = (Action<GoldMinerEntity, GoldMiner>)Delegate.Combine(goldMinerEntity.OnResolved, new Action<GoldMinerEntity, GoldMiner>(OnResolved));
	}

	private void OnResolved(GoldMinerEntity entity, GoldMiner game)
	{
		game.run.bomb += amount;
	}
}
