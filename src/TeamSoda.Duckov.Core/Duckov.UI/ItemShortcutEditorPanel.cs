using Duckov.Utilities;
using UnityEngine;

namespace Duckov.UI;

public class ItemShortcutEditorPanel : MonoBehaviour
{
	[SerializeField]
	private ItemShortcutEditorEntry entryTemplate;

	private PrefabPool<ItemShortcutEditorEntry> _entryPool;

	private PrefabPool<ItemShortcutEditorEntry> EntryPool
	{
		get
		{
			if (_entryPool == null)
			{
				_entryPool = new PrefabPool<ItemShortcutEditorEntry>(entryTemplate, entryTemplate.transform.parent);
				entryTemplate.gameObject.SetActive(value: false);
			}
			return _entryPool;
		}
	}

	private void OnEnable()
	{
		Setup();
	}

	private void Setup()
	{
		EntryPool.ReleaseAll();
		for (int i = 0; i <= ItemShortcut.MaxIndex; i++)
		{
			ItemShortcutEditorEntry itemShortcutEditorEntry = EntryPool.Get(entryTemplate.transform.parent);
			itemShortcutEditorEntry.Setup(i);
			itemShortcutEditorEntry.transform.SetAsLastSibling();
		}
	}
}
