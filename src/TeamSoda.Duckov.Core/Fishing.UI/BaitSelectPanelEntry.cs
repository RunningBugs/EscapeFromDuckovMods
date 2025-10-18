using Duckov.UI;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fishing.UI;

public class BaitSelectPanelEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private GameObject selectedIndicator;

	[SerializeField]
	private ItemDisplay itemDisplay;

	private BaitSelectPanel master;

	private Item targetItem;

	public Item Target => targetItem;

	private bool Selected
	{
		get
		{
			if (master == null)
			{
				return false;
			}
			return master.GetSelection() == this;
		}
	}

	internal void Setup(BaitSelectPanel master, Item cur)
	{
		UnregisterEvents();
		this.master = master;
		targetItem = cur;
		itemDisplay.Setup(targetItem);
		RegisterEvents();
		Refresh();
	}

	private void RegisterEvents()
	{
		if (!(master == null))
		{
			master.onSetSelection += Refresh;
		}
	}

	private void UnregisterEvents()
	{
		if (!(master == null))
		{
			master.onSetSelection -= Refresh;
		}
	}

	private void Refresh()
	{
		selectedIndicator.SetActive(Selected);
	}

	private void Awake()
	{
		itemDisplay.onPointerClick += OnPointerClick;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		eventData.Use();
		master.NotifySelect(this);
	}

	private void OnPointerClick(ItemDisplay display, PointerEventData data)
	{
		OnPointerClick(data);
	}
}
