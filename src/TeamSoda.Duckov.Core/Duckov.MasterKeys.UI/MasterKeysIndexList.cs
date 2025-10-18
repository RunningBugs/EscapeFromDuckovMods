using System;
using Duckov.Utilities;
using UnityEngine;

namespace Duckov.MasterKeys.UI;

public class MasterKeysIndexList : MonoBehaviour, ISingleSelectionMenu<MasterKeysIndexEntry>
{
	[SerializeField]
	private MasterKeysIndexEntry entryPrefab;

	[SerializeField]
	private RectTransform entryContainer;

	private PrefabPool<MasterKeysIndexEntry> _pool;

	private MasterKeysIndexEntry selection;

	private PrefabPool<MasterKeysIndexEntry> Pool
	{
		get
		{
			if (_pool == null)
			{
				_pool = new PrefabPool<MasterKeysIndexEntry>(entryPrefab, entryContainer, OnGetEntry, OnReleaseEntry);
			}
			return _pool;
		}
	}

	internal event Action<MasterKeysIndexEntry> onEntryPointerClicked;

	private void OnGetEntry(MasterKeysIndexEntry entry)
	{
		entry.onPointerClicked += OnEntryPointerClicked;
	}

	private void OnReleaseEntry(MasterKeysIndexEntry entry)
	{
		entry.onPointerClicked -= OnEntryPointerClicked;
	}

	private void OnEntryPointerClicked(MasterKeysIndexEntry entry)
	{
		this.onEntryPointerClicked?.Invoke(entry);
	}

	private void Awake()
	{
		entryPrefab.gameObject.SetActive(value: false);
	}

	internal void Refresh()
	{
		Pool.ReleaseAll();
		foreach (int allPossibleKey in MasterKeysManager.AllPossibleKeys)
		{
			Populate(allPossibleKey);
		}
	}

	private void Populate(int itemID)
	{
		MasterKeysIndexEntry masterKeysIndexEntry = Pool.Get(entryContainer);
		masterKeysIndexEntry.gameObject.SetActive(value: true);
		masterKeysIndexEntry.Setup(itemID, this);
	}

	public MasterKeysIndexEntry GetSelection()
	{
		return selection;
	}

	public bool SetSelection(MasterKeysIndexEntry selection)
	{
		this.selection = selection;
		return true;
	}
}
