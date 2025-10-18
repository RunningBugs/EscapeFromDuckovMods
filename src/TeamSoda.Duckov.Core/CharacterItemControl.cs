using ItemStatsSystem;
using UnityEngine;

public class CharacterItemControl : MonoBehaviour
{
	public CharacterMainControl characterMainControl;

	private Inventory inventory => characterMainControl.CharacterItem.Inventory;

	public bool PickupItem(Item item)
	{
		if (item == null)
		{
			return false;
		}
		if (inventory != null)
		{
			item.AgentUtilities.ReleaseActiveAgent();
			item.Detach();
			bool flag = false;
			bool? flag2 = characterMainControl.CharacterItem.TryPlug(item, emptyOnly: true);
			if ((flag2.HasValue && flag2.Value) || ((!characterMainControl.IsMainCharacter) ? characterMainControl.CharacterItem.Inventory.AddAndMerge(item) : ItemUtilities.SendToPlayerCharacterInventory(item)))
			{
				return true;
			}
		}
		item.Drop(base.transform.position, createRigidbody: true, Vector3.forward, 360f);
		return false;
	}
}
