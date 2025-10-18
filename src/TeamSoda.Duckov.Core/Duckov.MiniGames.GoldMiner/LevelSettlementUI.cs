using Duckov.UI;
using Duckov.UI.Animations;
using TMPro;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class LevelSettlementUI : MonoBehaviour
{
	[SerializeField]
	private GoldMiner goldMiner;

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private PunchReceiver moneyPunch;

	[SerializeField]
	private PunchReceiver factorPunch;

	[SerializeField]
	private PunchReceiver scorePunch;

	[SerializeField]
	private TextMeshProUGUI moneyText;

	[SerializeField]
	private TextMeshProUGUI factorText;

	[SerializeField]
	private TextMeshProUGUI scoreText;

	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private TextMeshProUGUI targetScoreText;

	[SerializeField]
	private GameObject clearIndicator;

	[SerializeField]
	private GameObject failIndicator;

	private int targetScore;

	private int money;

	private int score;

	private float factor;

	internal void Reset()
	{
		clearIndicator.SetActive(value: false);
		failIndicator.SetActive(value: false);
		money = 0;
		score = 0;
		factor = 0f;
		RefreshTexts();
	}

	public void SetTargetScore(int targetScore)
	{
		this.targetScore = targetScore;
		RefreshTexts();
	}

	public void StepResolveEntity(GoldMinerEntity entity)
	{
	}

	public void StepResult(bool clear)
	{
		clearIndicator.SetActive(clear);
		failIndicator.SetActive(!clear);
	}

	public void Step(int money, float factor, int score)
	{
		bool num = money > this.money;
		bool flag = factor > this.factor;
		bool flag2 = score > this.score;
		this.money = money;
		this.factor = factor;
		this.score = score;
		RefreshTexts();
		if (num)
		{
			moneyPunch.Punch();
		}
		if (flag)
		{
			factorPunch.Punch();
		}
		if (flag2)
		{
			scorePunch.Punch();
		}
	}

	private void RefreshTexts()
	{
		levelText.text = $"LEVEL {goldMiner.run.level + 1}";
		targetScoreText.text = $"{targetScore}";
		moneyText.text = $"${money}";
		factorText.text = $"{factor}";
		scoreText.text = $"{score}";
	}

	public void Show()
	{
		fadeGroup.Show();
	}

	public void Hide()
	{
		fadeGroup.Hide();
	}
}
