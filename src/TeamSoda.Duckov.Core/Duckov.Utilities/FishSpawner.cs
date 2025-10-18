using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.Weathers;
using ItemStatsSystem;
using UnityEngine;

namespace Duckov.Utilities;

public class FishSpawner : MonoBehaviour
{
	[Serializable]
	private struct SpecialPair
	{
		[ItemTypeID]
		public int baitID;

		[ItemTypeID]
		public int fishID;

		[Range(0f, 1f)]
		public float chance;
	}

	[SerializeField]
	private List<SpecialPair> specialPairs;

	[SerializeField]
	private RandomContainer<Tag> tags;

	[SerializeField]
	private List<Tag> excludeTags;

	[SerializeField]
	private RandomContainer<int> qualities;

	private List<Tag> excludeTagsReal;

	[SerializeField]
	private Tag Fish_OnlyDay;

	[SerializeField]
	private Tag Fish_OnlyNight;

	[SerializeField]
	private Tag Fish_OnlySunDay;

	[SerializeField]
	private Tag Fish_OnlyRainDay;

	[SerializeField]
	private Tag Fish_OnlyStorm;

	public void CalculateChances()
	{
		tags.RefreshPercent();
		qualities.RefreshPercent();
	}

	private void Awake()
	{
		excludeTagsReal = new List<Tag>();
	}

	private void Start()
	{
	}

	public async UniTask<Item> Spawn(int baitID, float luck)
	{
		int num = -1;
		bool atNight = TimeOfDayController.Instance.AtNight;
		Weather currentWeather = TimeOfDayController.Instance.CurrentWeather;
		foreach (SpecialPair specialPair in specialPairs)
		{
			if (baitID == specialPair.baitID && UnityEngine.Random.Range(0f, 1f) < specialPair.chance && CheckFishDayNightAndWeather(specialPair.fishID, atNight, currentWeather))
			{
				num = specialPair.fishID;
				break;
			}
		}
		if (num == -1)
		{
			luck = Mathf.Max(luck, 0.1f);
			float lowPercent = 1f - 1f / luck;
			Tag random = tags.GetRandom();
			int random2 = qualities.GetRandom(lowPercent);
			CalculateTags(atNight, currentWeather);
			int[] array = Search(new ItemFilter
			{
				requireTags = new Tag[1] { random },
				excludeTags = excludeTagsReal.ToArray(),
				minQuality = random2,
				maxQuality = random2
			});
			if (array.Length < 1)
			{
				Debug.Log($"LootBox未找到任何合适的随机物品\n Tag:{random.DisplayName} Quality:{random2}");
				return null;
			}
			num = array.GetRandom();
		}
		return await ItemAssetsCollection.InstantiateAsync(num);
	}

	public static int[] Search(ItemFilter filter)
	{
		return ItemAssetsCollection.Search(filter);
	}

	private void CalculateTags(bool atNight, Weather weather)
	{
		excludeTagsReal.Clear();
		excludeTagsReal.AddRange(excludeTags);
		if (atNight)
		{
			excludeTagsReal.Add(Fish_OnlyDay);
		}
		else
		{
			excludeTagsReal.Add(Fish_OnlyNight);
		}
		excludeTagsReal.Add(Fish_OnlySunDay);
		excludeTagsReal.Add(Fish_OnlyRainDay);
		excludeTagsReal.Add(Fish_OnlyStorm);
		switch (weather)
		{
		case Weather.Sunny:
			excludeTagsReal.Remove(Fish_OnlySunDay);
			break;
		case Weather.Rainy:
			excludeTagsReal.Remove(Fish_OnlyRainDay);
			break;
		case Weather.Stormy_I:
			excludeTagsReal.Remove(Fish_OnlyStorm);
			break;
		case Weather.Stormy_II:
			excludeTagsReal.Remove(Fish_OnlyStorm);
			break;
		case Weather.Cloudy:
			break;
		}
	}

	private bool CheckFishDayNightAndWeather(int fishID, bool atNight, Weather currentWeather)
	{
		ItemMetaData metaData = ItemAssetsCollection.GetMetaData(fishID);
		if (metaData.tags.Contains(Fish_OnlyNight) && !atNight)
		{
			return false;
		}
		if (metaData.tags.Contains(Fish_OnlyDay) && atNight)
		{
			return false;
		}
		if (metaData.tags.Contains(Fish_OnlyRainDay) && currentWeather != Weather.Rainy)
		{
			return false;
		}
		if (metaData.tags.Contains(Fish_OnlySunDay) && currentWeather != Weather.Sunny)
		{
			return false;
		}
		if (metaData.tags.Contains(Fish_OnlyStorm) && currentWeather != Weather.Stormy_I && currentWeather != Weather.Stormy_II)
		{
			return false;
		}
		return true;
	}
}
