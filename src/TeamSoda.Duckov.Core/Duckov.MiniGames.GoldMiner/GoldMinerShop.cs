using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.Utilities;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class GoldMinerShop : MiniGameBehaviour
{
	[SerializeField]
	private GoldMiner master;

	[SerializeField]
	private GoldMinerShopUI ui;

	[SerializeField]
	private RandomContainer<int> qualityDistribute;

	public List<ShopEntity> stock = new List<ShopEntity>();

	public Action onAfterOperation;

	private int capacity;

	private List<GoldMinerArtifact>[] validCandidateLists = new List<GoldMinerArtifact>[5];

	private bool complete;

	private int refreshPrice = 100;

	public int refreshChance { get; private set; }

	private void Clear()
	{
		capacity = master.run.shopCapacity;
		for (int i = 0; i < stock.Count; i++)
		{
			ShopEntity shopEntity = stock[i];
			if (shopEntity != null && (shopEntity.sold || !shopEntity.locked))
			{
				stock[i] = null;
			}
		}
		for (int j = capacity; j < stock.Count; j++)
		{
			if (stock[j] == null)
			{
				stock.RemoveAt(j);
			}
		}
	}

	private void Refill()
	{
		capacity = master.run.shopCapacity;
		for (int i = 0; i < capacity; i++)
		{
			if (stock.Count <= i)
			{
				stock.Add(null);
			}
			ShopEntity shopEntity = stock[i];
			if (shopEntity == null || shopEntity.sold)
			{
				stock[i] = GenerateNewShopItem();
			}
		}
	}

	private void RefreshStock()
	{
		Clear();
		CacheValidCandiateLists();
		Refill();
		onAfterOperation?.Invoke();
	}

	private void CacheValidCandiateLists()
	{
		for (int i = 0; i < 5; i++)
		{
			int quality = i + 1;
			List<GoldMinerArtifact> list = SearchValidCandidateArtifactIDs(quality).ToList();
			validCandidateLists[i] = list;
		}
		foreach (ShopEntity item in stock)
		{
			if (item != null && !(item.artifact == null) && !item.artifact.AllowMultiple)
			{
				List<GoldMinerArtifact>[] array = validCandidateLists;
				for (int j = 0; j < array.Length; j++)
				{
					array[j]?.Remove(item.artifact);
				}
			}
		}
	}

	private IEnumerable<GoldMinerArtifact> SearchValidCandidateArtifactIDs(int quality)
	{
		foreach (GoldMinerArtifact artifact in master.ArtifactPrefabs)
		{
			if (artifact.Quality == quality && (artifact.AllowMultiple || (master.run.GetArtifactCount(artifact.ID) <= 0 && !stock.Any((ShopEntity e) => e != null && !e.sold && e.ID == artifact.ID))))
			{
				yield return artifact;
			}
		}
	}

	private List<GoldMinerArtifact> GetValidCandidateArtifactIDs(int q)
	{
		return validCandidateLists[q - 1];
	}

	private ShopEntity GenerateNewShopItem()
	{
		int num = qualityDistribute.GetRandom();
		List<GoldMinerArtifact> list = null;
		for (int num2 = num; num2 >= 1; num2--)
		{
			list = GetValidCandidateArtifactIDs(num2);
			if (list.Count > 0)
			{
				num = num2;
				break;
			}
		}
		GoldMinerArtifact random = list.GetRandom(master.run.shopRandom);
		if (random != null && !random.AllowMultiple)
		{
			GetValidCandidateArtifactIDs(num)?.Remove(random);
		}
		if (random == null)
		{
			Debug.Log($"{num} failed to generate");
		}
		return new ShopEntity
		{
			artifact = random
		};
	}

	public bool Buy(ShopEntity entity)
	{
		if (!stock.Contains(entity))
		{
			Debug.LogError("Buying entity that doesn't exist in shop stock");
			return false;
		}
		if (entity.sold)
		{
			return false;
		}
		bool useTicket;
		int price = CalculateDealPrice(entity, out useTicket);
		if (master.run.shopTicket > 0)
		{
			master.run.shopTicket--;
		}
		else if (!master.PayMoney(price))
		{
			return false;
		}
		master.run.AttachArtifactFromPrefab(entity.artifact);
		entity.sold = true;
		onAfterOperation?.Invoke();
		return true;
	}

	public int CalculateDealPrice(ShopEntity entity, out bool useTicket)
	{
		useTicket = false;
		if (entity == null)
		{
			return 0;
		}
		if (master.run.shopTicket > 0)
		{
			useTicket = true;
			return 0;
		}
		GoldMinerArtifact artifact = entity.artifact;
		if (artifact == null)
		{
			return 0;
		}
		return Mathf.CeilToInt((float)artifact.BasePrice * entity.priceFactor * master.GlobalPriceFactor);
	}

	public async UniTask Execute()
	{
		RefreshStock();
		if (stock.Count > 0)
		{
			stock[0].priceFactor = master.run.shopPriceCut.Value;
		}
		refreshPrice = Mathf.RoundToInt(master.run.shopRefreshPrice.Value);
		refreshChance = Mathf.RoundToInt(master.run.shopRefreshChances.Value);
		complete = false;
		ui.gameObject.SetActive(value: true);
		ui.enableInput = false;
		ui.Setup(this);
		await UniTask.WaitForSeconds(0.1f);
		ui.enableInput = true;
		while (!complete)
		{
			await UniTask.Yield();
		}
		ui.gameObject.SetActive(value: false);
	}

	internal void Continue()
	{
		complete = true;
	}

	internal bool TryRefresh()
	{
		if (refreshChance <= 0)
		{
			return false;
		}
		int refreshCost = GetRefreshCost();
		if (!master.PayMoney(refreshCost))
		{
			return false;
		}
		refreshChance--;
		refreshPrice += Mathf.RoundToInt(master.run.shopRefreshPriceIncrement.Value);
		RefreshStock();
		return true;
	}

	internal int GetRefreshCost()
	{
		return refreshPrice;
	}
}
