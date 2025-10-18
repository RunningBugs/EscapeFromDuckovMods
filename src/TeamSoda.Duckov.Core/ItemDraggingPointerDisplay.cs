using Duckov.UI;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemDraggingPointerDisplay : MonoBehaviour
{
	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private RectTransform parentRectTransform;

	[SerializeField]
	private ItemDisplay display;

	private Item target;

	private void Awake()
	{
		rectTransform = base.transform as RectTransform;
		parentRectTransform = base.transform.parent as RectTransform;
		IItemDragSource.OnStartDragItem += OnStartDragItem;
		IItemDragSource.OnEndDragItem += OnEndDragItem;
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		IItemDragSource.OnStartDragItem -= OnStartDragItem;
		IItemDragSource.OnEndDragItem -= OnEndDragItem;
	}

	private void Update()
	{
		RefreshPosition();
		if (Mouse.current.leftButton.wasReleasedThisFrame)
		{
			OnEndDragItem(null);
		}
	}

	private void RefreshPosition()
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, Pointer.current.position.value, null, out var localPoint);
		rectTransform.localPosition = localPoint;
	}

	private void OnStartDragItem(Item item)
	{
		target = item;
		if (!(target == null))
		{
			display.Setup(target);
			RefreshPosition();
			base.gameObject.SetActive(value: true);
		}
	}

	private void OnEndDragItem(Item item)
	{
		target = null;
		base.gameObject.SetActive(value: false);
	}
}
