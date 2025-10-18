using System.Collections.Generic;
using Duckov.Bitcoins;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;

public class MiningMachineVisual : MonoBehaviour
{
	public List<MiningMachineCardDisplay> cardsDisplay;

	private bool inited;

	private SlotCollection slots;

	private Item minnerItem;

	private void Update()
	{
		if (!inited && (bool)BitcoinMiner.Instance && BitcoinMiner.Instance.Item != null)
		{
			inited = true;
			minnerItem = BitcoinMiner.Instance.Item;
			minnerItem.onSlotContentChanged += OnSlotContentChanged;
			slots = minnerItem.Slots;
			OnSlotContentChanged(minnerItem, null);
		}
	}

	private void OnDestroy()
	{
		if ((bool)minnerItem)
		{
			minnerItem.onSlotContentChanged -= OnSlotContentChanged;
		}
	}

	private void OnSlotContentChanged(Item minnerItem, Slot changedSlot)
	{
		for (int i = 0; i < slots.Count; i++)
		{
			if (cardsDisplay[i] == null)
			{
				continue;
			}
			Item content = slots[i].Content;
			MiningMachineCardDisplay.CardTypes cardType = MiningMachineCardDisplay.CardTypes.normal;
			if (content != null)
			{
				ItemSetting_GPU component = content.GetComponent<ItemSetting_GPU>();
				if ((bool)component)
				{
					cardType = component.cardType;
				}
			}
			cardsDisplay[i].SetVisualActive(content != null, cardType);
		}
	}
}
