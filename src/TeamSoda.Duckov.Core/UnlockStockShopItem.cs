using Duckov.Economy;
using Duckov.PerkTrees;
using ItemStatsSystem;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

public class UnlockStockShopItem : PerkBehaviour
{
	[ItemTypeID]
	[SerializeField]
	private int itemTypeID;

	private string DescriptionFormat => "PerkBehaviour_UnlockStockShopItem".ToPlainText();

	public override string Description => DescriptionFormat.Format(new { ItemDisplayName });

	private string ItemDisplayName => ItemAssetsCollection.GetMetaData(itemTypeID).DisplayName;

	private void Start()
	{
		if (base.Master.Unlocked && !EconomyManager.IsUnlocked(itemTypeID))
		{
			EconomyManager.Unlock(itemTypeID, needConfirm: false, showUI: false);
		}
	}

	protected override void OnUnlocked()
	{
		base.OnUnlocked();
		EconomyManager.Unlock(itemTypeID, needConfirm: false);
	}
}
