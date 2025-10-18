using Duckov.MiniGames.SnakeForces;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.Quests;

public class QuestTask_Snake_Score : Task
{
	[SerializeField]
	private int targetScore;

	private bool finished;

	[LocalizationKey("Default")]
	private string descriptionKey
	{
		get
		{
			return "Task_Snake_Score";
		}
		set
		{
		}
	}

	public override string Description => descriptionKey.ToPlainText().Format(new
	{
		score = targetScore
	});

	public override object GenerateSaveData()
	{
		return finished;
	}

	public override void SetupSaveData(object data)
	{
	}

	protected override bool CheckFinished()
	{
		return SnakeForce.HighScore >= targetScore;
	}
}
