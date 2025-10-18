using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.Quests.Rewards;

public class QuestReward_EXP : Reward
{
	[SerializeField]
	private int amount;

	[SerializeField]
	private bool claimed;

	public int Amount => amount;

	public override bool Claimed => claimed;

	private string descriptionFormatKey => "Reward_Exp";

	private string DescriptionFormat => descriptionFormatKey.ToPlainText();

	public override string Description => DescriptionFormat.Format(new { amount });

	public override bool AutoClaim => true;

	public override object GenerateSaveData()
	{
		return claimed;
	}

	public override void OnClaim()
	{
		if (!Claimed && EXPManager.AddExp(amount))
		{
			claimed = true;
		}
	}

	public override void SetupSaveData(object data)
	{
		if (data is bool flag)
		{
			claimed = flag;
		}
	}
}
