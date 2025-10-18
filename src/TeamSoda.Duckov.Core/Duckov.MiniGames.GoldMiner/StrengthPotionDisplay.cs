using System;
using Duckov.MiniGames.GoldMiner.UI;
using TMPro;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class StrengthPotionDisplay : MonoBehaviour
{
	[SerializeField]
	private GoldMiner master;

	[SerializeField]
	private TextMeshProUGUI amountText;

	[SerializeField]
	private NavEntry navEntry;

	private void Awake()
	{
		NavEntry obj = navEntry;
		obj.onInteract = (Action<NavEntry>)Delegate.Combine(obj.onInteract, new Action<NavEntry>(OnInteract));
		GoldMiner goldMiner = master;
		goldMiner.onEarlyLevelPlayTick = (Action<GoldMiner>)Delegate.Combine(goldMiner.onEarlyLevelPlayTick, new Action<GoldMiner>(OnEarlyLevelPlayTick));
	}

	private void OnEarlyLevelPlayTick(GoldMiner miner)
	{
		if (!(master == null) && master.run != null)
		{
			amountText.text = $"{master.run.strengthPotion}";
		}
	}

	private void OnInteract(NavEntry entry)
	{
		master.UseStrengthPotion();
	}
}
