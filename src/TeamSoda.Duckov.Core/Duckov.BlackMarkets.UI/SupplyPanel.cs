using Duckov.Utilities;
using UnityEngine;

namespace Duckov.BlackMarkets.UI;

public class SupplyPanel : MonoBehaviour
{
	[SerializeField]
	private SupplyPanel_Entry entryTemplate;

	private PrefabPool<SupplyPanel_Entry> _entryPool;

	public BlackMarket Target { get; private set; }

	private PrefabPool<SupplyPanel_Entry> EntryPool
	{
		get
		{
			if (_entryPool == null)
			{
				_entryPool = new PrefabPool<SupplyPanel_Entry>(entryTemplate, null, null, null, null, collectionCheck: true, 10, 10000, OnCreateEntry);
			}
			return _entryPool;
		}
	}

	private void OnCreateEntry(SupplyPanel_Entry entry)
	{
		entry.onDealButtonClicked += OnEntryClicked;
	}

	private void OnEntryClicked(SupplyPanel_Entry entry)
	{
		Debug.Log("Supply entry clicked");
		Target.Buy(entry.Target);
	}

	internal void Setup(BlackMarket target)
	{
		if (target == null)
		{
			Debug.LogError("加载 BlackMarket 的 Supply Panel 失败。Black Market 对象不存在。");
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
		foreach (BlackMarket.DemandSupplyEntry supply in Target.Supplies)
		{
			EntryPool.Get().Setup(supply);
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
