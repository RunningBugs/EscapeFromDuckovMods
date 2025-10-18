using Duckov.Utilities;
using ItemStatsSystem;
using TMPro;
using UnityEngine;

namespace Duckov.UI;

public class ItemStatEntry : MonoBehaviour, IPoolable
{
	private Stat target;

	[SerializeField]
	private TextMeshProUGUI displayName;

	[SerializeField]
	private TextMeshProUGUI value;

	public void NotifyPooled()
	{
	}

	public void NotifyReleased()
	{
		UnregisterEvents();
		target = null;
	}

	private void OnDisable()
	{
		UnregisterEvents();
	}

	internal void Setup(Stat target)
	{
		UnregisterEvents();
		this.target = target;
		RegisterEvents();
		Refresh();
	}

	private void Refresh()
	{
		StatInfoDatabase.Entry entry = StatInfoDatabase.Get(target.Key);
		displayName.text = target.DisplayName;
		value.text = target.Value.ToString(entry.DisplayFormat);
	}

	private void RegisterEvents()
	{
		if (target != null)
		{
			target.OnSetDirty += OnTargetSetDirty;
		}
	}

	private void UnregisterEvents()
	{
		if (target != null)
		{
			target.OnSetDirty -= OnTargetSetDirty;
		}
	}

	private void OnTargetSetDirty(Stat stat)
	{
		if (stat != target)
		{
			Debug.LogError("ItemStatEntry.target与事件触发者不匹配。");
		}
		else
		{
			Refresh();
		}
	}
}
