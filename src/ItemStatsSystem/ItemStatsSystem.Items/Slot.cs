using System;
using System.Collections.Generic;
using Duckov.Utilities;
using UnityEngine;

namespace ItemStatsSystem.Items;

[Serializable]
public class Slot
{
	[NonSerialized]
	private SlotCollection collection;

	[SerializeField]
	private string key;

	[SerializeField]
	private Sprite slotIcon;

	private Item content;

	public List<Tag> requireTags = new List<Tag>();

	public List<Tag> excludeTags = new List<Tag>();

	[SerializeField]
	private bool forbidItemsWithSameID;

	public Item Master => collection?.Master;

	public Item Content => content;

	private StringList referenceKeys => StringLists.SlotNames;

	public Sprite SlotIcon
	{
		get
		{
			return slotIcon;
		}
		set
		{
			slotIcon = value;
		}
	}

	public bool ForbidItemsWithSameID => forbidItemsWithSameID;

	public string Key => key;

	public string DisplayName
	{
		get
		{
			if (requireTags == null || requireTags.Count < 1)
			{
				return "?";
			}
			Tag tag = requireTags[0];
			if (tag == null)
			{
				return "?";
			}
			return tag.DisplayName;
		}
	}

	public event Action<Slot> onSlotContentChanged;

	public void Initialize(SlotCollection collection)
	{
		this.collection = collection;
	}

	public void ForceInvokeSlotContentChangedEvent()
	{
		this.onSlotContentChanged?.Invoke(this);
	}

	public bool Plug(Item otherItem, out Item unpluggedItem)
	{
		unpluggedItem = null;
		if (!CheckAbleToPlug(otherItem))
		{
			Debug.Log("Unable to Plug");
			return false;
		}
		if (content != null)
		{
			if (content.Stackable && content.TypeID == otherItem.TypeID)
			{
				content.Combine(otherItem);
				Master.NotifySlotPlugged(this);
				this.onSlotContentChanged?.Invoke(this);
				content.InitiateNotifyItemTreeChanged();
				if (otherItem.StackCount <= 0)
				{
					return true;
				}
				return false;
			}
			unpluggedItem = Unplug();
		}
		if (otherItem.PluggedIntoSlot != null)
		{
			otherItem.Detach();
		}
		if (otherItem.InInventory != null)
		{
			otherItem.Detach();
		}
		content = otherItem;
		otherItem.transform.SetParent(collection.transform);
		otherItem.NotifyPluggedTo(this);
		Master.NotifySlotPlugged(this);
		otherItem.InitiateNotifyItemTreeChanged();
		this.onSlotContentChanged?.Invoke(this);
		return true;
	}

	private bool CheckAbleToPlug(Item otherItem)
	{
		if (otherItem == null)
		{
			return false;
		}
		if (otherItem == content)
		{
			return false;
		}
		if (forbidItemsWithSameID && collection != null)
		{
			foreach (Slot item2 in collection)
			{
				if (item2 != null && item2 != this && item2.ForbidItemsWithSameID)
				{
					Item item = item2.Content;
					if (!(item == null) && !(item == otherItem) && item.TypeID == otherItem.TypeID)
					{
						return false;
					}
				}
			}
		}
		if (Master.GetAllParents().Contains(otherItem))
		{
			return false;
		}
		if (!otherItem.Tags.Check(requireTags, excludeTags))
		{
			return false;
		}
		return true;
	}

	public Item Unplug()
	{
		Item item = content;
		content = null;
		if (item != null)
		{
			if (!item.IsBeingDestroyed)
			{
				item.transform.SetParent(null);
			}
			item.NotifyUnpluggedFrom(this);
			Master.NotifySlotUnplugged(this);
			item.InitiateNotifyItemTreeChanged();
			Master.InitiateNotifyItemTreeChanged();
			this.onSlotContentChanged?.Invoke(this);
		}
		return item;
	}

	public bool CanPlug(Item item)
	{
		if (item == null)
		{
			return false;
		}
		return CheckAbleToPlug(item);
	}

	public Slot()
	{
	}

	public Slot(string key)
	{
		this.key = key;
	}
}
