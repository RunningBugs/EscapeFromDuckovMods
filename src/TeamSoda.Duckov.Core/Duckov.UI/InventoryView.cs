using Duckov.UI.Animations;
using ItemStatsSystem;
using UnityEngine;

namespace Duckov.UI;

public class InventoryView : View
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private ItemSlotCollectionDisplay slotDisplay;

	[SerializeField]
	private InventoryDisplay inventoryDisplay;

	[SerializeField]
	private ItemDetailsDisplay detailsDisplay;

	[SerializeField]
	private FadeGroup itemDetailsFadeGroup;

	private static InventoryView Instance => View.GetViewInstance<InventoryView>();

	private Item CharacterItem => LevelManager.Instance?.MainCharacter?.CharacterItem;

	protected override void Awake()
	{
		base.Awake();
	}

	private void Update()
	{
		bool editable = true;
		inventoryDisplay.Editable = editable;
		slotDisplay.Editable = editable;
	}

	protected override void OnOpen()
	{
		UnregisterEvents();
		base.OnOpen();
		Item characterItem = CharacterItem;
		if (characterItem == null)
		{
			Debug.LogError("物品栏开启失败，角色物体不存在");
			Close();
			return;
		}
		base.gameObject.SetActive(value: true);
		slotDisplay.Setup(characterItem);
		inventoryDisplay.Setup(characterItem.Inventory);
		RegisterEvents();
		fadeGroup.Show();
	}

	protected override void OnClose()
	{
		UnregisterEvents();
		base.OnClose();
		fadeGroup.Hide();
		itemDetailsFadeGroup.Hide();
		if ((bool)SplitDialogue.Instance && SplitDialogue.Instance.isActiveAndEnabled)
		{
			SplitDialogue.Instance.Cancel();
		}
	}

	private void RegisterEvents()
	{
		ItemUIUtilities.OnSelectionChanged += OnItemSelectionChanged;
	}

	private void OnItemSelectionChanged()
	{
		if (ItemUIUtilities.SelectedItem != null)
		{
			detailsDisplay.Setup(ItemUIUtilities.SelectedItem);
			itemDetailsFadeGroup.Show();
		}
		else
		{
			itemDetailsFadeGroup.Hide();
		}
	}

	private void UnregisterEvents()
	{
		ItemUIUtilities.OnSelectionChanged -= OnItemSelectionChanged;
	}

	public static void Show()
	{
		if (LevelManager.LevelInited)
		{
			LootView.Instance?.Show();
			if (LootView.Instance == null)
			{
				Debug.Log("LOOTVIEW INSTANCE IS NULL");
			}
		}
	}

	public static void Hide()
	{
		LootView.Instance?.Close();
	}
}
