using Duckov.Utilities;
using UnityEngine;

namespace Duckov.BlackMarkets.UI;

public class DemandPanel : MonoBehaviour
{
	[SerializeField]
	private DemandPanel_Entry entryTemplate;

	private PrefabPool<DemandPanel_Entry> _entryPool;

	public BlackMarket Target { get; private set; }

	private PrefabPool<DemandPanel_Entry> EntryPool
	{
		get
		{
			if (_entryPool == null)
			{
				_entryPool = new PrefabPool<DemandPanel_Entry>(entryTemplate, null, null, null, null, collectionCheck: true, 10, 10000, OnCreateEntry);
			}
			return _entryPool;
		}
	}

	private void OnCreateEntry(DemandPanel_Entry entry)
	{
		entry.onDealButtonClicked += OnEntryClicked;
	}

	private void OnEntryClicked(DemandPanel_Entry entry)
	{
		Target.Sell(entry.Target);
	}

	internal void Setup(BlackMarket target)
	{
		if (target == null)
		{
			Debug.LogError("加载 BlackMarket 的 DemandPanel 失败。Black Market 对象不存在。");
			return;
		}
		Target = target;
		Refresh();
		if (base.isActiveAndEnabled)
		{
			RegisterEvents();
		}
	}

	private void OnEnable()
	{
		RegisterEvents();
		Refresh();
	}

	private void OnDisable()
	{
		UnregsiterEvents();
	}

	private void Refresh()
	{
		if (Target == null)
		{
			return;
		}
		EntryPool.ReleaseAll();
		foreach (BlackMarket.DemandSupplyEntry demand in Target.Demands)
		{
			EntryPool.Get().Setup(demand);
		}
	}

	private void UnregsiterEvents()
	{
		if (!(Target == null))
		{
			Target.onAfterGenerateEntries -= OnAfterTargetGenerateEntries;
		}
	}

	private void RegisterEvents()
	{
		if (!(Target == null))
		{
			UnregsiterEvents();
			Target.onAfterGenerateEntries += OnAfterTargetGenerateEntries;
		}
	}

	private void OnAfterTargetGenerateEntries()
	{
		Refresh();
	}
}
