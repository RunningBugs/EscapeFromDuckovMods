using Duckov.Utilities;
using ItemStatsSystem.Items;
using UnityEngine;

namespace Duckov.UI;

public class SlotIndicator : MonoBehaviour, IPoolable
{
	[SerializeField]
	private GameObject contentIndicator;

	public Slot Target { get; private set; }

	public void Setup(Slot target)
	{
		UnregisterEvents();
		Target = target;
		RegisterEvents();
		Refresh();
	}

	private void RegisterEvents()
	{
		if (Target != null)
		{
			UnregisterEvents();
			Target.onSlotContentChanged += OnSlotContentChanged;
		}
	}

	private void UnregisterEvents()
	{
		if (Target != null)
		{
			Target.onSlotContentChanged -= OnSlotContentChanged;
		}
	}

	private void OnSlotContentChanged(Slot slot)
	{
		if (slot != Target)
		{
			Debug.LogError("Slot内容改变事件触发了，但它来自别的Slot。这说明Slot Indicator注册的事件发生了泄露，请检查代码。");
		}
		else
		{
			Refresh();
		}
	}

	private void Refresh()
	{
		if (!(contentIndicator == null) && Target != null)
		{
			contentIndicator.SetActive(Target.Content);
		}
	}

	public void NotifyPooled()
	{
		RegisterEvents();
		Refresh();
	}

	public void NotifyReleased()
	{
		UnregisterEvents();
		Target = null;
		contentIndicator.SetActive(value: false);
	}

	private void OnEnable()
	{
		RegisterEvents();
		Refresh();
	}

	private void OnDisable()
	{
		UnregisterEvents();
	}
}
