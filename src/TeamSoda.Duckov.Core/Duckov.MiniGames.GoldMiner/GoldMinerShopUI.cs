using System;
using TMPro;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class GoldMinerShopUI : MiniGameBehaviour
{
	[SerializeField]
	private GoldMiner master;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private GoldMinerShopUIEntry[] entries;

	public int navIndex;

	public bool enableInput;

	public GoldMinerShopUIEntry hoveringEntry;

	public GoldMinerShop target { get; private set; }

	private void UnregisterEvent()
	{
		if (!(target == null))
		{
			GoldMinerShop goldMinerShop = target;
			goldMinerShop.onAfterOperation = (Action)Delegate.Remove(goldMinerShop.onAfterOperation, new Action(OnAfterOperation));
		}
	}

	private void RegisterEvent()
	{
		if (!(target == null))
		{
			GoldMinerShop goldMinerShop = target;
			goldMinerShop.onAfterOperation = (Action)Delegate.Combine(goldMinerShop.onAfterOperation, new Action(OnAfterOperation));
		}
	}

	private void OnAfterOperation()
	{
		RefreshEntries();
	}

	private void RefreshEntries()
	{
		for (int i = 0; i < entries.Length; i++)
		{
			GoldMinerShopUIEntry goldMinerShopUIEntry = entries[i];
			if (i >= target.stock.Count)
			{
				goldMinerShopUIEntry.gameObject.SetActive(value: false);
				continue;
			}
			goldMinerShopUIEntry.gameObject.SetActive(value: true);
			ShopEntity shopEntity = target.stock[i];
			goldMinerShopUIEntry.Setup(this, shopEntity);
		}
	}

	public void Setup(GoldMinerShop shop)
	{
		UnregisterEvent();
		target = shop;
		RegisterEvent();
		RefreshEntries();
	}

	protected override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);
		RefreshDescriptionText();
	}

	private void RefreshDescriptionText()
	{
		string text = "";
		if (hoveringEntry != null && hoveringEntry.target != null && hoveringEntry.target.artifact != null)
		{
			text = hoveringEntry.target.artifact.Description;
		}
		descriptionText.text = text;
	}
}
