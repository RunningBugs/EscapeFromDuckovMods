using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using ItemStatsSystem;
using Saves;
using UnityEngine;

namespace Duckov.Aquariums;

public class Aquarium : MonoBehaviour
{
	private class ItemGraphicPair
	{
		public Item item;

		public ItemGraphicInfo graphic;
	}

	[SerializeField]
	private string id = "Default";

	[SerializeField]
	private Transform graphicsParent;

	[ItemTypeID]
	private int aquariumItemTypeID = 1158;

	private Item aquariumItem;

	private List<ItemGraphicPair> graphicRecords = new List<ItemGraphicPair>();

	private bool loading;

	private bool loaded;

	private int loadToken;

	private bool dirty = true;

	private string ItemSaveKey => "Aquarium/Item/" + id;

	private void Awake()
	{
		SavesSystem.OnCollectSaveData += Save;
	}

	private void Start()
	{
		Load().Forget();
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= Save;
	}

	private async UniTask Load()
	{
		if (aquariumItem != null)
		{
			aquariumItem.DestroyTree();
		}
		int token = ++loadToken;
		loading = true;
		Item item = await ItemSavesUtilities.LoadItem(ItemSaveKey);
		if (token != loadToken)
		{
			return;
		}
		if (item == null)
		{
			item = await ItemAssetsCollection.InstantiateAsync(aquariumItemTypeID);
			if (token != loadToken)
			{
				return;
			}
		}
		aquariumItem = item;
		aquariumItem.transform.SetParent(base.transform);
		aquariumItem.onChildChanged += OnChildChanged;
		loading = false;
		loaded = true;
	}

	private void OnChildChanged(Item item)
	{
		dirty = true;
	}

	private void FixedUpdate()
	{
		if (!loading && dirty)
		{
			Refresh();
			dirty = false;
		}
	}

	private void Refresh()
	{
		if (aquariumItem == null)
		{
			return;
		}
		foreach (Item allChild in aquariumItem.GetAllChildren(includingGrandChildren: false, excludeSelf: true))
		{
			if (!(allChild == null) && allChild.Tags.Contains("Fish"))
			{
				_ = GetOrCreateGraphic(allChild) == null;
			}
		}
		graphicRecords.RemoveAll((ItemGraphicPair e) => e == null || e.graphic == null);
		for (int num = 0; num < graphicRecords.Count; num++)
		{
			ItemGraphicPair itemGraphicPair = graphicRecords[num];
			if (itemGraphicPair.item == null || itemGraphicPair.item.ParentItem != aquariumItem)
			{
				if (itemGraphicPair.graphic != null)
				{
					Object.Destroy(itemGraphicPair.graphic);
				}
				graphicRecords.RemoveAt(num);
				num--;
			}
		}
	}

	private ItemGraphicInfo GetOrCreateGraphic(Item item)
	{
		if (item == null)
		{
			return null;
		}
		ItemGraphicPair itemGraphicPair = graphicRecords.Find((ItemGraphicPair e) => e != null && e.item == item);
		if (itemGraphicPair != null && itemGraphicPair.graphic != null)
		{
			return itemGraphicPair.graphic;
		}
		ItemGraphicInfo itemGraphicInfo = ItemGraphicInfo.CreateAGraphic(item, graphicsParent);
		if (itemGraphicPair != null)
		{
			graphicRecords.Remove(itemGraphicPair);
		}
		if (itemGraphicInfo == null)
		{
			return null;
		}
		itemGraphicInfo.GetComponent<IAquariumContent>()?.Setup(this);
		graphicRecords.Add(new ItemGraphicPair
		{
			item = item,
			graphic = itemGraphicInfo
		});
		return itemGraphicInfo;
	}

	public void Loot()
	{
		LootView.LootItem(aquariumItem);
	}

	private void Save()
	{
		if (!loading && loaded)
		{
			aquariumItem.Save(ItemSaveKey);
		}
	}
}
