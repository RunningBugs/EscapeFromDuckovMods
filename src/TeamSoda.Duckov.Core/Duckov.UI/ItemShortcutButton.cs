using System;
using DG.Tweening;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI;

public class ItemShortcutButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private ItemDisplay itemDisplay;

	[SerializeField]
	private GameObject usingIndicator;

	[SerializeField]
	private GameObject notInteractableIndicator;

	[SerializeField]
	private Image denialIndicator;

	[SerializeField]
	private Color denialColor;

	private bool isBeingDestroyed;

	private bool requireRefresh;

	private bool lastFrameUsing;

	public int Index { get; private set; }

	public ItemShortcutPanel Master { get; private set; }

	public Inventory Inventory { get; private set; }

	public CharacterMainControl Character { get; private set; }

	public Item TargetItem { get; private set; }

	private bool Interactable
	{
		get
		{
			if ((bool)TargetItem?.UsageUtilities)
			{
				return true;
			}
			if ((bool)TargetItem && TargetItem.HasHandHeldAgent)
			{
				return true;
			}
			if ((bool)TargetItem && TargetItem.GetBool("IsSkill"))
			{
				return true;
			}
			return false;
		}
	}

	private static event Action<int> OnRequireAnimateDenial;

	private Item GetTargetItem()
	{
		return ItemShortcut.Get(Index);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!Interactable)
		{
			denialIndicator.color = denialColor;
			denialIndicator.DOColor(Color.clear, 0.1f);
		}
		else if ((bool)Character && (bool)TargetItem && (bool)TargetItem.UsageUtilities && TargetItem.UsageUtilities.IsUsable(TargetItem, Character))
		{
			Character.UseItem(TargetItem);
		}
		else if ((bool)Character && (bool)TargetItem && TargetItem.GetBool("IsSkill"))
		{
			Character.ChangeHoldItem(TargetItem);
		}
		else if ((bool)Character && (bool)TargetItem && TargetItem.HasHandHeldAgent)
		{
			Character.ChangeHoldItem(TargetItem);
		}
		else
		{
			AnimateDenial();
		}
	}

	public void AnimateDenial()
	{
		denialIndicator.DOKill();
		denialIndicator.color = denialColor;
		denialIndicator.DOColor(Color.clear, 0.1f);
	}

	private void Awake()
	{
		OnRequireAnimateDenial += OnStaticAnimateDenial;
	}

	private void OnDestroy()
	{
		OnRequireAnimateDenial -= OnStaticAnimateDenial;
		isBeingDestroyed = true;
		UnregisterEvents();
	}

	private void OnStaticAnimateDenial(int index)
	{
		if (base.isActiveAndEnabled && index == Index)
		{
			AnimateDenial();
		}
	}

	public static void AnimateDenial(int index)
	{
		ItemShortcutButton.OnRequireAnimateDenial?.Invoke(index);
	}

	internal void Initialize(ItemShortcutPanel itemShortcutPanel, int index)
	{
		UnregisterEvents();
		Master = itemShortcutPanel;
		Inventory = Master.Target;
		Index = index;
		Character = Master.Character;
		Refresh();
		RegisterEvents();
	}

	private void Refresh()
	{
		if (!isBeingDestroyed)
		{
			UnregisterEvents();
			TargetItem = GetTargetItem();
			if (TargetItem == null)
			{
				SetupEmpty();
			}
			else
			{
				SetupItem(TargetItem);
			}
			RegisterEvents();
			requireRefresh = false;
		}
	}

	private void SetupItem(Item targetItem)
	{
		if ((bool)notInteractableIndicator)
		{
			notInteractableIndicator.gameObject.SetActive(value: false);
		}
		itemDisplay.Setup(targetItem);
		itemDisplay.gameObject.SetActive(value: true);
		notInteractableIndicator.gameObject.SetActive(!Interactable);
	}

	private void SetupEmpty()
	{
		itemDisplay.gameObject.SetActive(value: false);
	}

	private void RegisterEvents()
	{
		ItemShortcut.OnSetItem += OnItemShortcutSetItem;
		if (Inventory != null)
		{
			Inventory.onContentChanged += OnContentChanged;
		}
		if (TargetItem != null)
		{
			TargetItem.onSetStackCount += OnItemStackCountChanged;
		}
	}

	private void UnregisterEvents()
	{
		ItemShortcut.OnSetItem -= OnItemShortcutSetItem;
		if (Inventory != null)
		{
			Inventory.onContentChanged -= OnContentChanged;
		}
		if (TargetItem != null)
		{
			TargetItem.onSetStackCount -= OnItemStackCountChanged;
		}
	}

	private void OnItemShortcutSetItem(int obj)
	{
		Refresh();
	}

	private void OnItemStackCountChanged(Item item)
	{
		if (!(item != TargetItem))
		{
			requireRefresh = true;
		}
	}

	private void OnContentChanged(Inventory inventory, int index)
	{
		requireRefresh = true;
	}

	private void Update()
	{
		if (requireRefresh)
		{
			Refresh();
		}
		bool flag = TargetItem != null && Character.CurrentHoldItemAgent != null && TargetItem == Character.CurrentHoldItemAgent.Item;
		if (flag && !lastFrameUsing)
		{
			OnStartedUsing();
		}
		else if (!flag && lastFrameUsing)
		{
			OnStoppedUsing();
		}
		usingIndicator.gameObject.SetActive(flag);
	}

	private void OnStartedUsing()
	{
	}

	private void OnStoppedUsing()
	{
	}
}
