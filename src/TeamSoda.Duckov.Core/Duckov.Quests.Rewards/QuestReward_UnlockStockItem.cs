using Duckov.Economy;
using ItemStatsSystem;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.Quests.Rewards;

public class QuestReward_UnlockStockItem : Reward
{
	[SerializeField]
	[ItemTypeID]
	private int unlockItem;

	private bool claimed;

	public int UnlockItem => unlockItem;

	public override Sprite Icon => ItemAssetsCollection.GetMetaData(unlockItem).icon;

	private string descriptionFormatKey => "Reward_UnlockStockItem";

	private string DescriptionFormat => descriptionFormatKey.ToPlainText();

	private string ItemDisplayName => ItemAssetsCollection.GetMetaData(unlockItem).DisplayName;

	public override string Description => DescriptionFormat.Format(new { ItemDisplayName });

	public override bool Claimed => claimed;

	public override bool AutoClaim => true;

	private ItemMetaData GetItemMeta()
	{
		return ItemAssetsCollection.GetMetaData(unlockItem);
	}

	public override object GenerateSaveData()
	{
		return claimed;
	}

	public override void OnClaim()
	{
		EconomyManager.Unlock(unlockItem);
		claimed = true;
		ReportStatusChanged();
	}

	public override void SetupSaveData(object data)
	{
		if (data is bool flag)
		{
			claimed = flag;
		}
	}

	public override void NotifyReload(Quest questInstance)
	{
		if (questInstance.Complete)
		{
			EconomyManager.Unlock(unlockItem);
		}
	}
}
