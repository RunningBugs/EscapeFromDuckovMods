using Duckov.MiniGames;
using Duckov.UI;
using Duckov.UI.Animations;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;

public class GamingConsoleView : View
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private InventoryDisplay characterInventory;

	[SerializeField]
	private InventoryDisplay petInventory;

	[SerializeField]
	private InventoryDisplay storageInventory;

	[SerializeField]
	private SlotDisplay monitorSlotDisplay;

	[SerializeField]
	private SlotDisplay consoleSlotDisplay;

	[SerializeField]
	private ItemSlotCollectionDisplay consoleSlotCollectionDisplay;

	private GamingConsole target;

	private bool isBeingDestroyed;

	public static GamingConsoleView Instance => View.GetViewInstance<GamingConsoleView>();

	protected override void OnOpen()
	{
		base.OnOpen();
		fadeGroup.Show();
		Setup(target);
		if ((bool)CharacterMainControl.Main)
		{
			characterInventory.Setup(CharacterMainControl.Main.CharacterItem.Inventory);
		}
		if ((bool)PetProxy.PetInventory)
		{
			petInventory.Setup(PetProxy.PetInventory);
		}
		if ((bool)PlayerStorage.Inventory)
		{
			storageInventory.Setup(PlayerStorage.Inventory);
		}
		RefreshConsole();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	private void SetTarget(GamingConsole target)
	{
		if (this.target != null)
		{
			this.target.onContentChanged -= OnTargetContentChanged;
		}
		if (target != null)
		{
			this.target = target;
		}
		else
		{
			this.target = Object.FindObjectOfType<GamingConsole>();
		}
	}

	private void Setup(GamingConsole target)
	{
		SetTarget(target);
		if (!(this.target == null))
		{
			this.target.onContentChanged += OnTargetContentChanged;
			consoleSlotDisplay.Setup(this.target.ConsoleSlot);
			monitorSlotDisplay.Setup(this.target.MonitorSlot);
			RefreshConsole();
		}
	}

	private void OnTargetContentChanged(GamingConsole console)
	{
		RefreshConsole();
	}

	private void RefreshConsole()
	{
		if (isBeingDestroyed)
		{
			return;
		}
		Slot consoleSlot = target.ConsoleSlot;
		if (consoleSlot != null)
		{
			Item content = consoleSlot.Content;
			consoleSlotCollectionDisplay.gameObject.SetActive(content);
			if ((bool)content)
			{
				consoleSlotCollectionDisplay.Setup(content);
			}
		}
	}

	internal static void Show(GamingConsole console)
	{
		Instance.target = console;
		Instance.Open();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		isBeingDestroyed = true;
	}
}
