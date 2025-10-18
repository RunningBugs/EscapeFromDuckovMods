using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;

[Serializable]
public struct RandomItemGenerateDescription
{
	[Serializable]
	public struct Entry
	{
		[ItemTypeID]
		[SerializeField]
		public int itemTypeID;
	}

	[TextArea]
	[SerializeField]
	private string comment;

	[Range(0f, 1f)]
	public float chance;

	public Vector2Int randomCount;

	public bool controlDurability;

	public Vector2 durability;

	public Vector2 durabilityIntegrity;

	public bool randomFromPool;

	[SerializeField]
	public RandomContainer<Entry> itemPool;

	public RandomContainer<Tag> tags;

	public List<Tag> addtionalRequireTags;

	public List<Tag> excludeTags;

	public RandomContainer<int> qualities;

	public async UniTask<List<Item>> Generate(int count = -1)
	{
		List<Item> items = new List<Item>();
		if (count < 0)
		{
			count = UnityEngine.Random.Range(randomCount.x, randomCount.y + 1);
		}
		if (count < 1)
		{
			return items;
		}
		List<int> list = new List<int>();
		if (randomFromPool)
		{
			if (UnityEngine.Random.Range(0f, 1f) > chance)
			{
				return items;
			}
			for (int i = 0; i < count; i++)
			{
				Item item = await ItemAssetsCollection.InstantiateAsync(itemPool.GetRandom().itemTypeID);
				if (!(item == null))
				{
					items.Add(item);
					SetDurabilityIfNeeded(item);
				}
			}
			return items;
		}
		if (!excludeTags.Contains(GameplayDataSettings.Tags.Special))
		{
			excludeTags.Add(GameplayDataSettings.Tags.Special);
		}
		if (!LevelManager.Rule.AdvancedDebuffMode && !excludeTags.Contains(GameplayDataSettings.Tags.AdvancedDebuffMode))
		{
			excludeTags.Add(GameplayDataSettings.Tags.AdvancedDebuffMode);
		}
		for (int j = 0; j < count; j++)
		{
			if (!(UnityEngine.Random.Range(0f, 1f) > chance))
			{
				Tag random = tags.GetRandom();
				int random2 = qualities.GetRandom();
				List<Tag> list2 = new List<Tag>();
				if (random != null)
				{
					list2.Add(random);
				}
				if (addtionalRequireTags.Count > 0)
				{
					list2.AddRange(addtionalRequireTags);
				}
				int[] array = ItemAssetsCollection.Search(new ItemFilter
				{
					requireTags = list2.ToArray(),
					excludeTags = excludeTags.ToArray(),
					minQuality = random2,
					maxQuality = random2
				});
				if (array.Length >= 1)
				{
					int random3 = array.GetRandom();
					list.Add(random3);
				}
			}
		}
		foreach (int item3 in list)
		{
			Item item2 = await ItemAssetsCollection.InstantiateAsync(item3);
			if (!(item2 == null))
			{
				items.Add(item2);
				SetDurabilityIfNeeded(item2);
			}
		}
		return items;
	}

	private void SetDurabilityIfNeeded(Item targetItem)
	{
		if (!(targetItem == null) && controlDurability && targetItem.UseDurability)
		{
			float num = UnityEngine.Random.Range(durabilityIntegrity.x, durabilityIntegrity.y);
			targetItem.DurabilityLoss = 1f - num;
			float num2 = UnityEngine.Random.Range(durability.x, durability.y);
			if (num2 > num)
			{
				num2 = num;
			}
			targetItem.Durability = targetItem.MaxDurability * num2;
		}
	}

	private void RefreshPercent()
	{
		itemPool.RefreshPercent();
	}
}
