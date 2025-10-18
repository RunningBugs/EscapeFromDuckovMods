using System;
using DG.Tweening;
using Duckov.UI;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.Economy.UI;

public class StockShopItemEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private string moneyFormat = "n0";

	[SerializeField]
	private ItemDisplay itemDisplay;

	[SerializeField]
	private TextMeshProUGUI priceText;

	[SerializeField]
	private GameObject selectionIndicator;

	[SerializeField]
	private GameObject lockedIndicator;

	[SerializeField]
	private GameObject waitingForUnlockIndicator;

	[SerializeField]
	private GameObject outOfStockIndicator;

	[SerializeField]
	[Range(0f, 1f)]
	private float punchDuration = 0.2f;

	[SerializeField]
	[Range(-1f, 1f)]
	private float selectionRingPunchScale = 0.1f;

	[SerializeField]
	[Range(-1f, 1f)]
	private float iconPunchScale = 0.1f;

	private StockShopView master;

	private StockShop.Entry target;

	private StockShop stockShop => master?.Target;

	public StockShop.Entry Target => target;

	private void Awake()
	{
		itemDisplay.onPointerClick += OnItemDisplayPointerClick;
	}

	private void OnItemDisplayPointerClick(ItemDisplay display, PointerEventData data)
	{
		OnPointerClick(data);
	}

	public Item GetItem()
	{
		return stockShop.GetItemInstanceDirect(target.ItemTypeID);
	}

	internal void Setup(StockShopView master, StockShop.Entry entry)
	{
		UnregisterEvents();
		this.master = master;
		target = entry;
		Item itemInstanceDirect = stockShop.GetItemInstanceDirect(target.ItemTypeID);
		itemDisplay.Setup(itemInstanceDirect);
		itemDisplay.ShowOperationButtons = false;
		itemDisplay.IsStockshopSample = true;
		_ = itemInstanceDirect.StackCount;
		int num = stockShop.ConvertPrice(itemInstanceDirect);
		priceText.text = num.ToString(moneyFormat);
		Refresh();
		RegisterEvents();
	}

	private void RegisterEvents()
	{
		if (master != null)
		{
			StockShopView stockShopView = master;
			stockShopView.onSelectionChanged = (Action)Delegate.Combine(stockShopView.onSelectionChanged, new Action(OnMasterSelectionChanged));
		}
		if (target != null)
		{
			target.onStockChanged += OnTargetStockChanged;
		}
	}

	private void UnregisterEvents()
	{
		if (master != null)
		{
			StockShopView stockShopView = master;
			stockShopView.onSelectionChanged = (Action)Delegate.Remove(stockShopView.onSelectionChanged, new Action(OnMasterSelectionChanged));
		}
		if (target != null)
		{
			target.onStockChanged -= OnTargetStockChanged;
		}
	}

	private void OnMasterSelectionChanged()
	{
		Refresh();
	}

	private void OnTargetStockChanged(StockShop.Entry entry)
	{
		Refresh();
	}

	public bool IsUnlocked()
	{
		if (target == null)
		{
			return false;
		}
		if (target.ForceUnlock)
		{
			return true;
		}
		return EconomyManager.IsUnlocked(target.ItemTypeID);
	}

	private void Refresh()
	{
		if (base.gameObject.activeSelf)
		{
			bool active = master.GetSelection() == this;
			selectionIndicator.SetActive(active);
			bool flag = EconomyManager.IsUnlocked(target.ItemTypeID);
			bool flag2 = EconomyManager.IsWaitingForUnlockConfirm(target.ItemTypeID);
			if (target.ForceUnlock)
			{
				flag = true;
				flag2 = false;
			}
			lockedIndicator.SetActive(!flag && !flag2);
			waitingForUnlockIndicator.SetActive(!flag && flag2);
			base.gameObject.SetActive(flag || flag2);
			outOfStockIndicator.SetActive(Target.CurrentStock <= 0);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Punch();
		if (!(master == null))
		{
			eventData.Use();
			if (EconomyManager.IsWaitingForUnlockConfirm(target.ItemTypeID))
			{
				EconomyManager.ConfirmUnlock(target.ItemTypeID);
			}
			if (master.GetSelection() == this)
			{
				master.SetSelection(null);
			}
			else
			{
				master.SetSelection(this);
			}
		}
	}

	public void Punch()
	{
		selectionIndicator.transform.DOKill();
		selectionIndicator.transform.localScale = Vector3.one;
		selectionIndicator.transform.DOPunchScale(Vector3.one * selectionRingPunchScale, punchDuration);
	}

	private void OnEnable()
	{
		EconomyManager.OnItemUnlockStateChanged += OnItemUnlockStateChanged;
	}

	private void OnDisable()
	{
		EconomyManager.OnItemUnlockStateChanged -= OnItemUnlockStateChanged;
	}

	private void OnItemUnlockStateChanged(int itemTypeID)
	{
		if (target != null && itemTypeID == target.ItemTypeID)
		{
			Refresh();
		}
	}
}
