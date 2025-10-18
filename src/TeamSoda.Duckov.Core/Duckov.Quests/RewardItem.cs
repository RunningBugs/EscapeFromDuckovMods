using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using UnityEngine;

namespace Duckov.Quests;

public class RewardItem : Reward
{
	[ItemTypeID]
	public int itemTypeID;

	public int amount = 1;

	private bool claimed;

	private bool claiming;

	private ItemMetaData? _cachedMeta;

	public override bool Claimed => claimed;

	public override bool Claiming => claiming;

	private ItemMetaData CachedMeta
	{
		get
		{
			if (!_cachedMeta.HasValue)
			{
				_cachedMeta = ItemAssetsCollection.GetMetaData(itemTypeID);
			}
			return _cachedMeta.Value;
		}
	}

	public override Sprite Icon => CachedMeta.icon;

	public override string Description => $"{CachedMeta.DisplayName} x{amount}";

	public override object GenerateSaveData()
	{
		return claimed;
	}

	public override void SetupSaveData(object data)
	{
		claimed = (bool)data;
	}

	public override void OnClaim()
	{
		if (!claimed && !claiming)
		{
			claiming = true;
			GenerateAndGiveItems().Forget();
		}
	}

	private async UniTask GenerateAndGiveItems()
	{
		ItemMetaData meta = ItemAssetsCollection.GetMetaData(itemTypeID);
		if (meta.id <= 0)
		{
			return;
		}
		int remaining = amount;
		List<Item> generatedItems = new List<Item>();
		while (remaining > 0)
		{
			int batchAmount = Mathf.Min(remaining, meta.maxStackCount);
			if (batchAmount <= 0)
			{
				break;
			}
			remaining -= batchAmount;
			Item item = await ItemAssetsCollection.InstantiateAsync(itemTypeID);
			if (!(item == null))
			{
				if (batchAmount > 1)
				{
					item.StackCount = batchAmount;
				}
				generatedItems.Add(item);
			}
		}
		foreach (Item item2 in generatedItems)
		{
			SendItemToPlayerStorage(item2);
		}
		claimed = true;
		claiming = false;
		base.Master.NotifyRewardClaimed(this);
	}

	private void SendItemToPlayerStorage(Item item)
	{
		PlayerStorage.Push(item, toBufferDirectly: true);
	}
}
