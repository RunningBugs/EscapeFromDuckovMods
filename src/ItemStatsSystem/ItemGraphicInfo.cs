using System;
using System.Collections.Generic;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;

public class ItemGraphicInfo : MonoBehaviour
{
	[Serializable]
	public struct ItemGraphicSocket
	{
		public Transform socketPoint;

		public GameObject showIfPluged;

		public GameObject hideIfPluged;
	}

	[SerializeField]
	private List<ItemGraphicSocket> sockets;

	public Dictionary<string, ItemGraphicSocket> socketsDictionary;

	private Item itemRefrence;

	private List<ItemGraphicInfo> subGraphics;

	public Item ItemRefrence => itemRefrence;

	public static ItemGraphicInfo CreateAGraphic(Item item, Transform parent)
	{
		if (item == null || item.ItemGraphic == null)
		{
			return null;
		}
		ItemGraphicInfo itemGraphicInfo = UnityEngine.Object.Instantiate(item.ItemGraphic);
		if (parent != null)
		{
			itemGraphicInfo.transform.SetParent(parent);
		}
		itemGraphicInfo.transform.localPosition = Vector3.zero;
		itemGraphicInfo.transform.localRotation = Quaternion.identity;
		itemGraphicInfo.transform.localScale = Vector3.one;
		itemGraphicInfo.Setup(item);
		return itemGraphicInfo;
	}

	public void Setup(Item item)
	{
		itemRefrence = item;
		subGraphics = new List<ItemGraphicInfo>();
		socketsDictionary = new Dictionary<string, ItemGraphicSocket>();
		foreach (ItemGraphicSocket socket in sockets)
		{
			socketsDictionary.Add(socket.socketPoint.name, socket);
		}
		if (!(item.Slots != null) || item.Slots.Count <= 0)
		{
			return;
		}
		foreach (Slot slot in item.Slots)
		{
			Item content = slot.Content;
			if (content == null)
			{
				continue;
			}
			string key = slot.Key;
			if (!socketsDictionary.TryGetValue(key, out var value))
			{
				continue;
			}
			ItemGraphicInfo itemGraphicInfo = CreateAGraphic(content, value.socketPoint);
			if ((bool)itemGraphicInfo)
			{
				if ((bool)value.showIfPluged)
				{
					value.showIfPluged.SetActive(value: true);
				}
				if ((bool)value.hideIfPluged)
				{
					value.hideIfPluged.SetActive(value: false);
				}
				subGraphics.Add(itemGraphicInfo);
				itemGraphicInfo.Setup(content);
			}
		}
	}
}
