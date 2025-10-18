using Duckov.Economy;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.Quests.Tasks;

public class QuestTask_SubmitMoney : Task
{
	[SerializeField]
	private int money;

	[SerializeField]
	[LocalizationKey("Default")]
	private string decriptionFormatKey = "QuestTask_SubmitMoney";

	[SerializeField]
	[LocalizationKey("Default")]
	private string interactTextKey = "QuestTask_SubmitMoney_Interact";

	private bool submitted;

	public string DescriptionFormat => decriptionFormatKey.ToPlainText();

	public override string Description => DescriptionFormat.Format(new { money });

	public override bool Interactable => true;

	public override bool PossibleValidInteraction => CheckMoneyEnough();

	public override string InteractText => interactTextKey.ToPlainText();

	public override void Interact()
	{
		if (new Cost(money).Pay())
		{
			submitted = true;
			ReportStatusChanged();
		}
	}

	private bool CheckMoneyEnough()
	{
		return EconomyManager.Money >= money;
	}

	public override object GenerateSaveData()
	{
		return submitted;
	}

	public override void SetupSaveData(object data)
	{
		if (data is bool flag)
		{
			submitted = flag;
		}
	}

	protected override bool CheckFinished()
	{
		return submitted;
	}
}
