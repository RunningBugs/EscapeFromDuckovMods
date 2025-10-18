using System;
using System.Linq;
using Duckov.MiniGames.GoldMiner.UI;
using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.MiniGames.GoldMiner;

public class PassivePropsUI : MiniGameBehaviour
{
	[SerializeField]
	private GoldMiner master;

	[SerializeField]
	private RectTransform descriptionContainer;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private PassivePropDisplay entryTemplate;

	[SerializeField]
	private NavGroup navGroup;

	[SerializeField]
	private NavGroup upNavGroup;

	[SerializeField]
	private GridLayoutGroup gridLayout;

	private PrefabPool<PassivePropDisplay> _pool;

	private bool changeLock;

	private PrefabPool<PassivePropDisplay> Pool
	{
		get
		{
			if (_pool == null)
			{
				_pool = new PrefabPool<PassivePropDisplay>(entryTemplate, null, OnGetEntry, OnReleaseEntry);
			}
			return _pool;
		}
	}

	private void OnReleaseEntry(PassivePropDisplay display)
	{
		navGroup.Remove(display.NavEntry);
	}

	private void OnGetEntry(PassivePropDisplay display)
	{
		navGroup.Add(display.NavEntry);
	}

	private void Awake()
	{
		GoldMiner goldMiner = master;
		goldMiner.onLevelBegin = (Action<GoldMiner>)Delegate.Combine(goldMiner.onLevelBegin, new Action<GoldMiner>(OnLevelBegin));
		GoldMiner goldMiner2 = master;
		goldMiner2.onArtifactChange = (Action<GoldMiner>)Delegate.Combine(goldMiner2.onArtifactChange, new Action<GoldMiner>(OnArtifactChanged));
		GoldMiner goldMiner3 = master;
		goldMiner3.onEarlyLevelPlayTick = (Action<GoldMiner>)Delegate.Combine(goldMiner3.onEarlyLevelPlayTick, new Action<GoldMiner>(OnEarlyTick));
		NavGroup.OnNavGroupChanged = (Action)Delegate.Combine(NavGroup.OnNavGroupChanged, new Action(OnNavGroupChanged));
	}

	private void OnDestroy()
	{
		NavGroup.OnNavGroupChanged = (Action)Delegate.Remove(NavGroup.OnNavGroupChanged, new Action(OnNavGroupChanged));
	}

	private void OnNavGroupChanged()
	{
		changeLock = true;
		if (navGroup.active && Pool.ActiveEntries.Count <= 0)
		{
			upNavGroup.SetAsActiveNavGroup();
		}
		RefreshDescription();
	}

	private void OnEarlyTick(GoldMiner miner)
	{
		RefreshDescription();
	}

	private void SetCoord((int x, int y) coord)
	{
		int navIndex = CoordToIndex(coord);
		navGroup.NavIndex = navIndex;
		RefreshDescription();
	}

	private void RefreshDescription()
	{
		if (!navGroup.active)
		{
			HideDescription();
			return;
		}
		if (Pool.ActiveEntries.Count <= 0)
		{
			HideDescription();
			return;
		}
		NavEntry selectedEntry = navGroup.GetSelectedEntry();
		if (selectedEntry == null)
		{
			HideDescription();
			return;
		}
		if (!selectedEntry.VCT.IsHovering)
		{
			HideDescription();
			return;
		}
		PassivePropDisplay component = selectedEntry.GetComponent<PassivePropDisplay>();
		if (component == null)
		{
			HideDescription();
		}
		else
		{
			SetupAndShowDescription(component);
		}
	}

	private void HideDescription()
	{
		descriptionContainer.gameObject.SetActive(value: false);
	}

	private void SetupAndShowDescription(PassivePropDisplay ppd)
	{
		descriptionContainer.gameObject.SetActive(value: true);
		string description = ppd.Target.Description;
		descriptionText.text = description;
		descriptionContainer.position = ppd.rectTransform.TransformPoint(ppd.rectTransform.rect.max);
	}

	private int CoordToIndex((int x, int y) coord)
	{
		int count = navGroup.entries.Count;
		if (count <= 0)
		{
			return 0;
		}
		int constraintCount = gridLayout.constraintCount;
		int num = count / constraintCount;
		if (coord.y > num)
		{
			coord.y = num;
		}
		int num2 = constraintCount;
		if (coord.y == num)
		{
			num2 = count % constraintCount;
		}
		if (coord.x < 0)
		{
			coord.x = num2 - 1;
		}
		coord.x %= num2;
		if (coord.y < 0)
		{
			coord.y = num;
		}
		coord.y %= num + 1;
		return constraintCount * coord.y + coord.x;
	}

	private (int x, int y) IndexToCoord(int index)
	{
		int constraintCount = gridLayout.constraintCount;
		int item = index / constraintCount;
		return (x: index % constraintCount, y: item);
	}

	private void OnLevelBegin(GoldMiner miner)
	{
		Refresh();
		RefreshDescription();
	}

	private void OnArtifactChanged(GoldMiner miner)
	{
		Refresh();
	}

	private void Refresh()
	{
		Pool.ReleaseAll();
		if (master == null)
		{
			return;
		}
		GoldMinerRunData run = master.run;
		if (run == null)
		{
			return;
		}
		foreach (IGrouping<string, GoldMinerArtifact> item in from e in run.artifacts
			where e != null
			group e by e.ID)
		{
			GoldMinerArtifact target = item.ElementAt(0);
			Pool.Get().Setup(target, item.Count());
		}
	}
}
