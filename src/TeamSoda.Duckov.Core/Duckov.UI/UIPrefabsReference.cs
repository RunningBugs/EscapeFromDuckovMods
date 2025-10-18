using UnityEngine;

namespace Duckov.UI;

[CreateAssetMenu]
public class UIPrefabsReference : ScriptableObject
{
	[SerializeField]
	private ItemDisplay itemDisplay;

	[SerializeField]
	private SlotIndicator slotIndicator;

	[SerializeField]
	private SlotDisplay slotDisplay;

	[SerializeField]
	private InventoryEntry inventoryEntry;

	public ItemDisplay ItemDisplay => itemDisplay;

	public SlotIndicator SlotIndicator => slotIndicator;

	public SlotDisplay SlotDisplay => slotDisplay;

	public InventoryEntry InventoryEntry => inventoryEntry;
}
