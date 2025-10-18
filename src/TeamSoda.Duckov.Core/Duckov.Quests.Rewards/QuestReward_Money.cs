using Duckov.Economy;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.Quests.Rewards;

public class QuestReward_Money : Reward
{
	[Min(0f)]
	[SerializeField]
	private int amount;

	[SerializeField]
	private bool claimed;

	public int Amount => amount;

	public override bool Claimed => claimed;

	[SerializeField]
	private string descriptionFormatKey => "Reward_Money";

	private string DescriptionFormat => descriptionFormatKey.ToPlainText();

	public override bool AutoClaim => true;

	public override string Description => DescriptionFormat.Format(new { amount });

	public override object GenerateSaveData()
	{
		return claimed;
	}

	public override void OnClaim()
	{
		if (!Claimed && EconomyManager.Add(amount))
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
