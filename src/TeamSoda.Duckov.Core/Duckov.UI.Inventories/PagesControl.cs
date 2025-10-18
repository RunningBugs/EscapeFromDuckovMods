using Duckov.Utilities;
using UnityEngine;

namespace Duckov.UI.Inventories;

public class PagesControl : MonoBehaviour
{
	[SerializeField]
	private InventoryDisplay target;

	[SerializeField]
	private PagesControl_Entry template;

	[SerializeField]
	private GameObject inputIndicators;

	private PrefabPool<PagesControl_Entry> _pool;

	private PrefabPool<PagesControl_Entry> Pool
	{
		get
		{
			if (_pool == null)
			{
				_pool = new PrefabPool<PagesControl_Entry>(template);
			}
			return _pool;
		}
	}

	private void Start()
	{
		if (target != null)
		{
			Setup(target);
		}
	}

	public void Setup(InventoryDisplay target)
	{
		UnregisterEvents();
		this.target = target;
		RegisterEvents();
		Refresh();
	}

	private void RegisterEvents()
	{
		UnregisterEvents();
		if (!(target == null))
		{
			target.onPageInfoRefreshed += OnPageInfoRefreshed;
		}
	}

	private void UnregisterEvents()
	{
		if (!(target == null))
		{
			target.onPageInfoRefreshed -= OnPageInfoRefreshed;
		}
	}

	private void OnPageInfoRefreshed()
	{
		Refresh();
	}

	private void Refresh()
	{
		Pool.ReleaseAll();
		if ((bool)inputIndicators)
		{
			inputIndicators?.SetActive(value: false);
		}
		if (!(target == null) && target.UsePages && target.MaxPage > 1)
		{
			for (int i = 0; i < target.MaxPage; i++)
			{
				Pool.Get().Setup(this, i, target.SelectedPage == i);
			}
			if ((bool)inputIndicators)
			{
				inputIndicators?.SetActive(value: true);
			}
		}
	}

	internal void NotifySelect(int i)
	{
		if (!(target == null))
		{
			target.SetPage(i);
		}
	}
}
