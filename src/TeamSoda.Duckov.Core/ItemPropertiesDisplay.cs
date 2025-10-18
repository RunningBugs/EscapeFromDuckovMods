using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;

public class ItemPropertiesDisplay : MonoBehaviour
{
	[SerializeField]
	private LabelAndValue entryTemplate;

	private PrefabPool<LabelAndValue> _entryPool;

	private PrefabPool<LabelAndValue> EntryPool
	{
		get
		{
			if (_entryPool == null)
			{
				_entryPool = new PrefabPool<LabelAndValue>(entryTemplate);
			}
			return _entryPool;
		}
	}

	private void Awake()
	{
		entryTemplate.gameObject.SetActive(value: false);
	}

	internal void Setup(Item targetItem)
	{
		EntryPool.ReleaseAll();
		if (targetItem == null)
		{
			return;
		}
		foreach (var item in targetItem.GetPropertyValueTextPair())
		{
			EntryPool.Get().Setup(item.name, item.value, item.polarity);
		}
	}
}
