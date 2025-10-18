using Duckov.MiniGames.BubblePoppers;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.Quests;

public class QuestTask_BubblePopper_Level : Task
{
	[SerializeField]
	private int targetLevel;

	private bool finished;

	[LocalizationKey("Default")]
	private string descriptionKey
	{
		get
		{
			return "Task_BubblePopper_Level";
		}
		set
		{
		}
	}

	public override string Description => descriptionKey.ToPlainText().Format(new
	{
		level = targetLevel
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
		return BubblePopper.HighLevel >= targetLevel;
	}
}
