using System;
using Duckov.MiniGames.GoldMiner.UI;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class GoldMinerShopUIContinueBtn : MonoBehaviour
{
	[SerializeField]
	private GoldMinerShop shop;

	[SerializeField]
	private NavEntry navEntry;

	private void Awake()
	{
		if (!navEntry)
		{
			navEntry = GetComponent<NavEntry>();
		}
		NavEntry obj = navEntry;
		obj.onInteract = (Action<NavEntry>)Delegate.Combine(obj.onInteract, new Action<NavEntry>(OnInteract));
	}

	private void OnInteract(NavEntry entry)
	{
		shop.Continue();
	}
}
