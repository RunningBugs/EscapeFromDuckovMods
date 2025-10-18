using System;
using Duckov.MiniGames.GoldMiner.UI;
using TMPro;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class GoldMinerShopUIRefreshBtn : MonoBehaviour
{
	[SerializeField]
	private GoldMinerShop shop;

	[SerializeField]
	private NavEntry navEntry;

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private TextMeshProUGUI refreshChanceText;

	[SerializeField]
	private GameObject noChanceIndicator;

	private void Awake()
	{
		if (!navEntry)
		{
			navEntry = GetComponent<NavEntry>();
		}
		NavEntry obj = navEntry;
		obj.onInteract = (Action<NavEntry>)Delegate.Combine(obj.onInteract, new Action<NavEntry>(OnInteract));
		GoldMinerShop goldMinerShop = shop;
		goldMinerShop.onAfterOperation = (Action)Delegate.Combine(goldMinerShop.onAfterOperation, new Action(OnAfterOperation));
	}

	private void OnEnable()
	{
		RefreshCostText();
	}

	private void OnAfterOperation()
	{
		RefreshCostText();
	}

	private void RefreshCostText()
	{
		costText.text = $"${shop.GetRefreshCost()}";
		refreshChanceText.text = $"{shop.refreshChance}";
		noChanceIndicator.SetActive(shop.refreshChance < 1);
	}

	private void OnInteract(NavEntry entry)
	{
		shop.TryRefresh();
	}
}
