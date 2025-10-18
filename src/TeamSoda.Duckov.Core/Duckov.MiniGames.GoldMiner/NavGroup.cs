using System;
using System.Collections.Generic;
using Duckov.MiniGames.GoldMiner.UI;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class NavGroup : MiniGameBehaviour
{
	[SerializeField]
	private GoldMiner master;

	[SerializeField]
	public List<NavEntry> entries;

	public static Action OnNavGroupChanged;

	private int _navIndex;

	public static NavGroup ActiveNavGroup { get; private set; }

	public bool active => ActiveNavGroup == this;

	public int NavIndex
	{
		get
		{
			return _navIndex;
		}
		set
		{
			int navIndex = _navIndex;
			_navIndex = value;
			CleanupIndex();
			int navIndex2 = _navIndex;
			RefreshEntry(navIndex);
			RefreshEntry(navIndex2);
		}
	}

	public void SetAsActiveNavGroup()
	{
		NavGroup activeNavGroup = ActiveNavGroup;
		ActiveNavGroup = this;
		RefreshAll();
		if (activeNavGroup != null)
		{
			activeNavGroup.RefreshAll();
		}
		OnNavGroupChanged?.Invoke();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		RefreshAll();
	}

	private void CleanupIndex()
	{
		if (_navIndex < 0)
		{
			_navIndex = entries.Count - 1;
		}
		if (_navIndex >= entries.Count)
		{
			_navIndex = 0;
		}
	}

	private void RefreshAll()
	{
		for (int i = 0; i < entries.Count; i++)
		{
			RefreshEntry(i);
		}
	}

	private void RefreshEntry(int index)
	{
		if (index >= 0 && index < entries.Count)
		{
			entries[index].NotifySelectionState(active && NavIndex == index);
		}
	}

	public NavEntry GetSelectedEntry()
	{
		if (NavIndex < 0 || NavIndex >= entries.Count)
		{
			return null;
		}
		return entries[NavIndex];
	}

	private void Awake()
	{
		if (master == null)
		{
			master = GetComponentInParent<GoldMiner>();
		}
		GoldMiner goldMiner = master;
		goldMiner.onLevelBegin = (Action<GoldMiner>)Delegate.Combine(goldMiner.onLevelBegin, new Action<GoldMiner>(OnLevelBegin));
	}

	private void OnLevelBegin(GoldMiner miner)
	{
		RefreshAll();
	}

	internal void Remove(NavEntry navEntry)
	{
		entries.Remove(navEntry);
		CleanupIndex();
		RefreshAll();
	}

	internal void Add(NavEntry navEntry)
	{
		entries.Add(navEntry);
		CleanupIndex();
		RefreshAll();
	}

	internal void TrySelect(NavEntry navEntry)
	{
		if (entries.Contains(navEntry))
		{
			int navIndex = entries.IndexOf(navEntry);
			SetAsActiveNavGroup();
			NavIndex = navIndex;
		}
	}
}
