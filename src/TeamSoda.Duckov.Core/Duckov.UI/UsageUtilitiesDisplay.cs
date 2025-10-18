using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;

namespace Duckov.UI;

public class UsageUtilitiesDisplay : MonoBehaviour
{
	[SerializeField]
	private UsageUtilitiesDisplay_Entry entryTemplate;

	private PrefabPool<UsageUtilitiesDisplay_Entry> _entryPool;

	public UsageUtilities Target { get; private set; }

	private PrefabPool<UsageUtilitiesDisplay_Entry> EntryPool
	{
		get
		{
			if (_entryPool == null)
			{
				_entryPool = new PrefabPool<UsageUtilitiesDisplay_Entry>(entryTemplate);
			}
			return _entryPool;
		}
	}

	public void Setup(Item item)
	{
		if (!(item == null))
		{
			UsageUtilities component = item.GetComponent<UsageUtilities>();
			if (!(component == null))
			{
				Target = component;
				base.gameObject.SetActive(value: true);
				Refresh();
				return;
			}
		}
		base.gameObject.SetActive(value: false);
	}

	private void Refresh()
	{
		EntryPool.ReleaseAll();
		foreach (UsageBehavior behavior in Target.behaviors)
		{
			if (!(behavior == null) && behavior.DisplaySettings.display)
			{
				EntryPool.Get().Setup(behavior);
			}
		}
		if (EntryPool.ActiveEntries.Count <= 0)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
