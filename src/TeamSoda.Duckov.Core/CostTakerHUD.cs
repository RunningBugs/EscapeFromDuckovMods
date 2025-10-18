using Duckov.Utilities;
using UnityEngine;

public class CostTakerHUD : MonoBehaviour
{
	[SerializeField]
	private CostTakerHUD_Entry entryTemplate;

	private PrefabPool<CostTakerHUD_Entry> _entryPool;

	private PrefabPool<CostTakerHUD_Entry> EntryPool
	{
		get
		{
			if (_entryPool == null)
			{
				_entryPool = new PrefabPool<CostTakerHUD_Entry>(entryTemplate);
			}
			return _entryPool;
		}
	}

	private void Awake()
	{
		entryTemplate.gameObject.SetActive(value: false);
		ShowAll();
		CostTaker.OnCostTakerRegistered += OnCostTakerRegistered;
		CostTaker.OnCostTakerUnregistered += OnCostTakerUnregistered;
	}

	private void OnDestroy()
	{
		CostTaker.OnCostTakerRegistered -= OnCostTakerRegistered;
		CostTaker.OnCostTakerUnregistered -= OnCostTakerUnregistered;
	}

	private void OnCostTakerRegistered(CostTaker taker)
	{
		ShowHUD(taker);
	}

	private void OnCostTakerUnregistered(CostTaker taker)
	{
		HideHUD(taker);
	}

	private void Start()
	{
	}

	private void ShowAll()
	{
		EntryPool.ReleaseAll();
		foreach (CostTaker activeCostTaker in CostTaker.ActiveCostTakers)
		{
			ShowHUD(activeCostTaker);
		}
	}

	private void ShowHUD(CostTaker costTaker)
	{
		EntryPool.Get().Setup(costTaker);
	}

	private void HideHUD(CostTaker costTaker)
	{
		CostTakerHUD_Entry costTakerHUD_Entry = EntryPool.Find((CostTakerHUD_Entry e) => e.gameObject.activeSelf && e.Target == costTaker);
		if (!(costTakerHUD_Entry == null))
		{
			EntryPool.Release(costTakerHUD_Entry);
		}
	}
}
