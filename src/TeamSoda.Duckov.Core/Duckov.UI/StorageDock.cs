using System.Collections.Generic;
using Duckov.UI.Animations;
using Duckov.Utilities;
using ItemStatsSystem.Data;
using UnityEngine;

namespace Duckov.UI;

public class StorageDock : View
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private StorageDockEntry entryTemplate;

	[SerializeField]
	private GameObject placeHolder;

	private PrefabPool<StorageDockEntry> _entryPool;

	public static StorageDock Instance => View.GetViewInstance<StorageDock>();

	private PrefabPool<StorageDockEntry> EntryPool
	{
		get
		{
			if (_entryPool == null)
			{
				_entryPool = new PrefabPool<StorageDockEntry>(entryTemplate);
			}
			return _entryPool;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		entryTemplate.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		PlayerStorage.OnTakeBufferItem += OnTakeBufferItem;
	}

	private void OnDisable()
	{
		PlayerStorage.OnTakeBufferItem -= OnTakeBufferItem;
	}

	private void OnTakeBufferItem()
	{
		Refresh();
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		if (PlayerStorage.Instance == null)
		{
			Close();
			return;
		}
		fadeGroup.Show();
		Setup();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	private void Setup()
	{
		Refresh();
	}

	private void Refresh()
	{
		EntryPool.ReleaseAll();
		List<ItemTreeData> incomingItemBuffer = PlayerStorage.IncomingItemBuffer;
		for (int i = 0; i < incomingItemBuffer.Count; i++)
		{
			ItemTreeData itemTreeData = incomingItemBuffer[i];
			if (itemTreeData != null)
			{
				EntryPool.Get().Setup(i, itemTreeData);
			}
		}
		placeHolder.gameObject.SetActive(incomingItemBuffer.Count <= 0);
	}

	internal static void Show()
	{
		if (!(Instance == null))
		{
			Instance.Open();
		}
	}
}
