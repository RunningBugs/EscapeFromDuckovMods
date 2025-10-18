using System;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class NavGroupLinearController : MiniGameBehaviour
{
	[SerializeField]
	private GoldMiner master;

	[SerializeField]
	private NavGroup navGroup;

	[SerializeField]
	private NavGroup otherNavGroup;

	[SerializeField]
	private bool setActiveWhenLevelBegin;

	private bool changeLock;

	private void Awake()
	{
		GoldMiner goldMiner = master;
		goldMiner.onLevelBegin = (Action<GoldMiner>)Delegate.Combine(goldMiner.onLevelBegin, new Action<GoldMiner>(OnLevelBegin));
		NavGroup.OnNavGroupChanged = (Action)Delegate.Combine(NavGroup.OnNavGroupChanged, new Action(OnNavGroupChanged));
	}

	private void OnLevelBegin(GoldMiner miner)
	{
		if (setActiveWhenLevelBegin)
		{
			navGroup.SetAsActiveNavGroup();
		}
	}

	private void OnNavGroupChanged()
	{
		changeLock = true;
	}
}
