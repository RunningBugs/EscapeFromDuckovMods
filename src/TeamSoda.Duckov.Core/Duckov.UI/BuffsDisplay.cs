using System.Collections.Generic;
using Duckov.Buffs;
using Duckov.Utilities;
using UnityEngine;

namespace Duckov.UI;

public class BuffsDisplay : MonoBehaviour
{
	[SerializeField]
	private BuffsDisplayEntry prefab;

	private PrefabPool<BuffsDisplayEntry> _entryPool;

	private List<BuffsDisplayEntry> activeEntries = new List<BuffsDisplayEntry>();

	private CharacterBuffManager buffManager;

	private PrefabPool<BuffsDisplayEntry> EntryPool
	{
		get
		{
			if (_entryPool == null)
			{
				_entryPool = new PrefabPool<BuffsDisplayEntry>(prefab, base.transform, delegate(BuffsDisplayEntry e)
				{
					activeEntries.Add(e);
				}, delegate(BuffsDisplayEntry e)
				{
					activeEntries.Remove(e);
				});
			}
			return _entryPool;
		}
	}

	public void ReleaseEntry(BuffsDisplayEntry entry)
	{
		EntryPool.Release(entry);
	}

	private void Awake()
	{
		LevelManager.OnLevelInitialized += OnLevelInitialized;
		if (LevelManager.LevelInited)
		{
			OnLevelInitialized();
		}
	}

	private void OnDestroy()
	{
		UnregisterEvents();
		LevelManager.OnLevelInitialized -= OnLevelInitialized;
	}

	private void OnLevelInitialized()
	{
		UnregisterEvents();
		buffManager = LevelManager.Instance.MainCharacter.GetBuffManager();
		foreach (Buff buff in buffManager.Buffs)
		{
			OnAddBuff(buffManager, buff);
		}
		RegisterEvents();
	}

	private void RegisterEvents()
	{
		if (!(buffManager == null))
		{
			buffManager.onAddBuff += OnAddBuff;
			buffManager.onRemoveBuff += OnRemoveBuff;
		}
	}

	private void UnregisterEvents()
	{
		if (!(buffManager == null))
		{
			buffManager.onAddBuff -= OnAddBuff;
			buffManager.onRemoveBuff -= OnRemoveBuff;
		}
	}

	private void OnAddBuff(CharacterBuffManager manager, Buff buff)
	{
		if (!buff.Hide)
		{
			EntryPool.Get().Setup(this, buff);
		}
	}

	private void OnRemoveBuff(CharacterBuffManager manager, Buff buff)
	{
		BuffsDisplayEntry buffsDisplayEntry = activeEntries.Find((BuffsDisplayEntry e) => e.Target == buff);
		if (!(buffsDisplayEntry == null))
		{
			buffsDisplayEntry.Release();
		}
	}
}
