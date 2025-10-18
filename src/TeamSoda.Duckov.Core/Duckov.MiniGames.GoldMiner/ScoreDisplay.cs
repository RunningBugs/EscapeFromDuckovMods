using System;
using TMPro;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class ScoreDisplay : MonoBehaviour
{
	[SerializeField]
	private GoldMiner master;

	[SerializeField]
	private TextMeshProUGUI moneyText;

	[SerializeField]
	private TextMeshProUGUI factorText;

	[SerializeField]
	private TextMeshProUGUI scoreText;

	[SerializeField]
	private TextMeshProUGUI targetScoreText;

	private void Awake()
	{
		GoldMiner goldMiner = master;
		goldMiner.onLevelBegin = (Action<GoldMiner>)Delegate.Combine(goldMiner.onLevelBegin, new Action<GoldMiner>(OnLevelBegin));
		GoldMiner goldMiner2 = master;
		goldMiner2.onAfterResolveEntity = (Action<GoldMiner, GoldMinerEntity>)Delegate.Combine(goldMiner2.onAfterResolveEntity, new Action<GoldMiner, GoldMinerEntity>(OnAfterResolveEntity));
	}

	private void OnAfterResolveEntity(GoldMiner miner, GoldMinerEntity entity)
	{
		Refresh();
	}

	private void OnLevelBegin(GoldMiner miner)
	{
		Refresh();
	}

	private void Refresh()
	{
		GoldMinerRunData run = master.run;
		if (run == null)
		{
			return;
		}
		int num = 0;
		float num2 = run.scoreFactorBase.Value + run.levelScoreFactor;
		int targetScore = run.targetScore;
		foreach (GoldMinerEntity resolvedEntity in master.resolvedEntities)
		{
			int num3 = Mathf.CeilToInt((float)resolvedEntity.Value * run.charm.Value);
			if (num3 != 0)
			{
				num += num3;
			}
		}
		moneyText.text = $"${num}";
		factorText.text = $"{num2}";
		scoreText.text = $"{Mathf.CeilToInt((float)num * num2)}";
		targetScoreText.text = $"{targetScore}";
	}
}
