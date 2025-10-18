using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Duckov.UI;

public class ItemShortcutEditorEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IItemDragSource, IBeginDragHandler, IEndDragHandler, IDragHandler
{
	[SerializeField]
	private ItemDisplay itemDisplay;

	[SerializeField]
	private GameObject hoveringIndicator;

	[SerializeField]
	private int index;

	[SerializeField]
	private InputIndicator indicator;

	private Item displayingItem;

	private bool dirty;

	private Item TargetItem => ItemShortcut.Get(index);

	private void Awake()
	{
		itemDisplay.onPointerClick += OnItemDisplayClicked;
		itemDisplay.onReceiveDrop += OnDrop;
		ItemShortcut.OnSetItem += OnSetItem;
		hoveringIndicator.SetActive(value: false);
	}

	private void OnSetItem(int index)
	{
		if (index == this.index)
		{
			Refresh();
		}
	}

	private void OnItemDisplayClicked(ItemDisplay display, PointerEventData data)
	{
		OnPointerClick(data);
		data.Use();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (ItemUIUtilities.SelectedItem != null && ItemShortcut.Set(index, ItemUIUtilities.SelectedItem))
		{
			Refresh();
		}
	}

	internal void Refresh()
	{
		UnregisterEvents();
		if (displayingItem != TargetItem)
		{
			itemDisplay.Punch();
		}
		displayingItem = TargetItem;
		itemDisplay.Setup(displayingItem);
		itemDisplay.ShowOperationButtons = false;
		RegisterEvents();
	}

	private void RegisterEvents()
	{
		if (displayingItem != null)
		{
			displayingItem.onParentChanged += OnTargetParentChanged;
			displayingItem.onSetStackCount += OnTargetStackCountChanged;
		}
	}

	private void UnregisterEvents()
	{
		if (displayingItem != null)
		{
			displayingItem.onParentChanged -= OnTargetParentChanged;
			displayingItem.onSetStackCount -= OnTargetStackCountChanged;
		}
	}

	private void OnTargetStackCountChanged(Item item)
	{
		SetDirty();
	}

	private void OnTargetParentChanged(Item item)
	{
		SetDirty();
	}

	private void SetDirty()
	{
		dirty = true;
	}

	private void Update()
	{
		if (dirty)
		{
			Refresh();
		}
	}

	private void OnDestroy()
	{
		UnregisterEvents();
		ItemShortcut.OnSetItem -= OnSetItem;
	}

	internal void Setup(int i)
	{
		index = i;
		Refresh();
		InputActionReference inputActionRef = InputActionReference.Create(GameplayDataSettings.InputActions[$"Character/ItemShortcut{i + 3}"]);
		indicator.Setup(inputActionRef);
	}

	public void OnDrop(PointerEventData eventData)
	{
		eventData.Use();
		IItemDragSource component = eventData.pointerDrag.gameObject.GetComponent<IItemDragSource>();
		if (component == null || !component.IsEditable())
		{
			return;
		}
		Item item = component.GetItem();
		if (!(item == null))
		{
			if (!item.IsInPlayerCharacter())
			{
				ItemUtilities.SendToPlayer(item, dontMerge: false, sendToStorage: false);
			}
			if (ItemShortcut.Set(index, item))
			{
				Refresh();
				AudioManager.Post("UI/click");
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hoveringIndicator.SetActive(value: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hoveringIndicator.SetActive(value: false);
	}

	public bool IsEditable()
	{
		return TargetItem != null;
	}

	public Item GetItem()
	{
		return TargetItem;
	}

	public void OnDrag(PointerEventData eventData)
	{
	}
}
