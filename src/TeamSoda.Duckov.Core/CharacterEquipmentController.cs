using System;
using System.Linq;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;

public class CharacterEquipmentController : MonoBehaviour
{
	[SerializeField]
	private CharacterMainControl characterMainControl;

	private Item characterItem;

	public static int equipmentModelHash = "EquipmentModel".GetHashCode();

	public static int armorHash = "Armor".GetHashCode();

	public static int helmatHash = "Helmat".GetHashCode();

	public static int faceMaskHash = "FaceMask".GetHashCode();

	public static int backpackHash = "Backpack".GetHashCode();

	public static int headsetHash = "Headset".GetHashCode();

	private Slot armorSlot;

	private Slot helmatSlot;

	private Slot backpackSlot;

	private Slot faceMaskSlot;

	private Slot headsetSlot;

	public event Action<Slot> OnHelmatSlotContentChanged;

	public event Action<Slot> OnFaceMaskSlotContentChanged;

	public void SetItem(Item _item)
	{
		characterItem = _item;
		armorSlot = characterItem.Slots.GetSlot(armorHash);
		helmatSlot = characterItem.Slots.GetSlot(helmatHash);
		faceMaskSlot = characterItem.Slots.GetSlot(faceMaskHash);
		backpackSlot = characterItem.Slots.GetSlot(backpackHash);
		headsetSlot = characterItem.Slots.GetSlot(headsetHash);
		armorSlot.onSlotContentChanged += ChangeArmorModel;
		helmatSlot.onSlotContentChanged += ChangeHelmatModel;
		faceMaskSlot.onSlotContentChanged += ChangeFaceMaskModel;
		backpackSlot.onSlotContentChanged += ChangeBackpackModel;
		headsetSlot.onSlotContentChanged += ChangeHeadsetModel;
		if (armorSlot?.Content != null)
		{
			ChangeArmorModel(armorSlot);
		}
		if (helmatSlot?.Content != null)
		{
			ChangeHelmatModel(helmatSlot);
		}
		if (faceMaskSlot?.Content != null)
		{
			ChangeFaceMaskModel(faceMaskSlot);
		}
		if (backpackSlot?.Content != null)
		{
			ChangeBackpackModel(backpackSlot);
		}
		if (headsetSlot?.Content != null)
		{
			ChangeHeadsetModel(headsetSlot);
		}
	}

	private void OnDestroy()
	{
		if (armorSlot != null)
		{
			armorSlot.onSlotContentChanged -= ChangeArmorModel;
		}
		if (helmatSlot != null)
		{
			helmatSlot.onSlotContentChanged -= ChangeHelmatModel;
		}
		if (backpackSlot != null)
		{
			backpackSlot.onSlotContentChanged -= ChangeBackpackModel;
		}
		if (faceMaskSlot != null)
		{
			faceMaskSlot.onSlotContentChanged -= ChangeFaceMaskModel;
		}
	}

	private void ChangeArmorModel(Slot slot)
	{
		if (!(characterMainControl.characterModel == null))
		{
			Transform armorSocket = characterMainControl.characterModel.ArmorSocket;
			ChangeEquipmentModel(slot, armorSocket);
		}
	}

	private void ChangeHelmatModel(Slot slot)
	{
		this.OnHelmatSlotContentChanged?.Invoke(slot);
		if (!(characterMainControl.characterModel == null))
		{
			Transform helmatSocket = characterMainControl.characterModel.HelmatSocket;
			ChangeEquipmentModel(slot, helmatSocket);
		}
	}

	private void ChangeHeadsetModel(Slot slot)
	{
		if (!(characterMainControl.characterModel == null))
		{
			Transform helmatSocket = characterMainControl.characterModel.HelmatSocket;
			ChangeEquipmentModel(slot, helmatSocket);
		}
	}

	private void ChangeBackpackModel(Slot slot)
	{
		if (!(characterMainControl.characterModel == null))
		{
			Transform backpackSocket = characterMainControl.characterModel.BackpackSocket;
			ChangeEquipmentModel(slot, backpackSocket);
		}
	}

	private void ChangeFaceMaskModel(Slot slot)
	{
		this.OnFaceMaskSlotContentChanged?.Invoke(slot);
		if (!(characterMainControl.characterModel == null))
		{
			Transform faceMaskSocket = characterMainControl.characterModel.FaceMaskSocket;
			ChangeEquipmentModel(slot, faceMaskSocket);
		}
	}

	private void ChangeEquipmentModel(Slot slot, Transform socket)
	{
		if (slot != null && !(slot.Content == null))
		{
			ItemAgent itemAgent = slot.Content.AgentUtilities.CreateAgent(equipmentModelHash, ItemAgent.AgentTypes.equipment);
			if (itemAgent == null)
			{
				Debug.LogError("生成的装备Item没有装备agent，Item名称：" + slot.Content.gameObject.name);
			}
			if (itemAgent != null)
			{
				itemAgent.transform.SetParent(socket, worldPositionStays: false);
				itemAgent.transform.localRotation = Quaternion.identity;
				itemAgent.transform.localPosition = Vector3.zero;
			}
		}
	}

	private bool IsSlotRequireTag(Slot slot, Tag tag)
	{
		return slot.requireTags.Any((Tag e) => e.Hash == tag.Hash);
	}
}
