using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;

namespace Duckov.UI;

public class TagsDisplay : MonoBehaviour
{
	[SerializeField]
	private TagsDisplayEntry entryTemplate;

	private PrefabPool<TagsDisplayEntry> _entryPool;

	private PrefabPool<TagsDisplayEntry> EntryPool
	{
		get
		{
			if (_entryPool == null)
			{
				_entryPool = new PrefabPool<TagsDisplayEntry>(entryTemplate);
			}
			return _entryPool;
		}
	}

	private void Awake()
	{
		entryTemplate.gameObject.SetActive(value: false);
	}

	public void Setup(Item item)
	{
		EntryPool.ReleaseAll();
		if (item == null)
		{
			return;
		}
		foreach (Tag tag in item.Tags)
		{
			if (!(tag == null) && tag.Show)
			{
				EntryPool.Get().Setup(tag);
			}
		}
	}

	internal void Clear()
	{
		EntryPool.ReleaseAll();
	}
}
