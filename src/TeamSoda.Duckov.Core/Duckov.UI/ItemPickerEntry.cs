using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.UI;

public class ItemPickerEntry : MonoBehaviour, IPoolable
{
	[SerializeField]
	private ItemDisplay itemDisplay;

	private ItemPicker master;

	private Item target;

	private void Awake()
	{
		itemDisplay.onPointerClick += OnItemDisplayClicked;
	}

	private void OnDestroy()
	{
		itemDisplay.onPointerClick -= OnItemDisplayClicked;
	}

	private void OnItemDisplayClicked(ItemDisplay display, PointerEventData eventData)
	{
		master.NotifyEntryClicked(this, target);
	}

	public void Setup(ItemPicker master, Item item)
	{
		this.master = master;
		target = item;
		if (target != null)
		{
			itemDisplay.Setup(target);
		}
		else
		{
			Debug.LogError("Item Picker不应当展示空的Item。");
		}
		itemDisplay.ShowOperationButtons = false;
	}

	public void NotifyPooled()
	{
	}

	public void NotifyReleased()
	{
	}
}
