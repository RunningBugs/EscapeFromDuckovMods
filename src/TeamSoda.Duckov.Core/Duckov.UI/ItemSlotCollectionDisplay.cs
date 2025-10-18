using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;

namespace Duckov.UI;

public class ItemSlotCollectionDisplay : MonoBehaviour
{
	[SerializeField]
	private Transform entriesParent;

	[SerializeField]
	private CanvasGroup notEditableIndicator;

	[SerializeField]
	private bool editable = true;

	[SerializeField]
	private bool contentSelectable = true;

	[SerializeField]
	private bool showOperationMenu = true;

	[SerializeField]
	private bool notifyNotEditable;

	[SerializeField]
	private float fadeDuration = 1f;

	[SerializeField]
	private float sustainDuration = 1f;

	private List<SlotDisplay> slots = new List<SlotDisplay>();

	private int currentToken;

	public bool Editable
	{
		get
		{
			return editable;
		}
		internal set
		{
			editable = value;
		}
	}

	public bool ContentSelectable
	{
		get
		{
			return contentSelectable;
		}
		set
		{
			contentSelectable = value;
		}
	}

	public bool ShowOperationMenu => showOperationMenu;

	public bool Movable { get; private set; }

	public Item Target { get; private set; }

	public event Action<ItemSlotCollectionDisplay, SlotDisplay> onElementClicked;

	public event Action<ItemSlotCollectionDisplay, SlotDisplay> onElementDoubleClicked;

	public void Setup(Item target, bool movable = false)
	{
		Target = target;
		Clear();
		if (Target == null || Target.Slots == null)
		{
			return;
		}
		Movable = movable;
		for (int i = 0; i < Target.Slots.Count; i++)
		{
			Slot slot = Target.Slots[i];
			if (slot != null)
			{
				SlotDisplay slotDisplay = SlotDisplay.Get();
				slotDisplay.onSlotDisplayClicked += OnSlotDisplayClicked;
				slotDisplay.onSlotDisplayDoubleClicked += OnSlotDisplayDoubleClicked;
				slotDisplay.ShowOperationMenu = ShowOperationMenu;
				slotDisplay.Setup(slot);
				slotDisplay.Editable = editable;
				slotDisplay.ContentSelectable = contentSelectable;
				slotDisplay.transform.SetParent(entriesParent, worldPositionStays: false);
				slotDisplay.Movable = Movable;
				slots.Add(slotDisplay);
			}
		}
	}

	private void OnSlotDisplayDoubleClicked(SlotDisplay display)
	{
		this.onElementDoubleClicked?.Invoke(this, display);
	}

	private void Clear()
	{
		foreach (SlotDisplay slot in slots)
		{
			slot.onSlotDisplayClicked -= OnSlotDisplayClicked;
			SlotDisplay.Release(slot);
		}
		slots.Clear();
		entriesParent.DestroyAllChildren();
	}

	private void OnSlotDisplayClicked(SlotDisplay display)
	{
		this.onElementClicked?.Invoke(this, display);
		if (!editable && notifyNotEditable)
		{
			ShowNotEditableIndicator().Forget();
		}
	}

	private async UniTask ShowNotEditableIndicator()
	{
		int token = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		currentToken = token;
		notEditableIndicator.DOKill();
		await notEditableIndicator.DOFade(1f, fadeDuration);
		if (!TokenChanged())
		{
			await UniTask.WaitForSeconds(sustainDuration, ignoreTimeScale: true);
			if (!TokenChanged())
			{
				await notEditableIndicator.DOFade(0f, fadeDuration);
			}
		}
		bool TokenChanged()
		{
			return token != currentToken;
		}
	}
}
