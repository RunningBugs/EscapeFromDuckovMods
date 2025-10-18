using Duckov.UI;
using ItemStatsSystem;
using UnityEngine;

public class DebugUISetup : MonoBehaviour
{
	[SerializeField]
	private ItemSlotCollectionDisplay slotCollectionDisplay;

	[SerializeField]
	private InventoryDisplay inventoryDisplay;

	private CharacterMainControl Character => LevelManager.Instance.MainCharacter;

	private Item CharacterItem => Character.CharacterItem;

	public void Setup()
	{
		slotCollectionDisplay.Setup(CharacterItem);
		inventoryDisplay.Setup(CharacterItem.Inventory);
	}
}
