using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;

public class FishingRod : MonoBehaviour
{
	[SerializeField]
	private ItemAgent _selfAgent;

	private Slot baitSlot;

	public Transform lineStart;

	private ItemAgent selfAgent
	{
		get
		{
			if (_selfAgent == null)
			{
				_selfAgent = GetComponent<ItemAgent>();
			}
			return _selfAgent;
		}
	}

	public Item Bait
	{
		get
		{
			if (baitSlot == null)
			{
				baitSlot = selfAgent.Item.Slots.GetSlot("Bait");
			}
			if (baitSlot != null)
			{
				return baitSlot.Content;
			}
			return null;
		}
	}

	public bool UseBait()
	{
		Item bait = Bait;
		if (bait == null)
		{
			return false;
		}
		if (bait.Stackable)
		{
			bait.StackCount--;
		}
		else
		{
			bait.DestroyTree();
		}
		return true;
	}
}
