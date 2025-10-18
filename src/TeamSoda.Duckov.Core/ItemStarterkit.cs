using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using ItemStatsSystem;
using Saves;
using SodaCraft.Localizations;
using UnityEngine;

public class ItemStarterkit : InteractableBase
{
	[ItemTypeID]
	[SerializeField]
	private List<int> items;

	[SerializeField]
	private GameObject notPickedItem;

	[SerializeField]
	private GameObject pickedItem;

	[SerializeField]
	private GameObject pickFX;

	private List<Item> itemsCache;

	[SerializeField]
	private string notificationTextKey;

	private bool caching;

	private bool cached;

	private bool picked;

	private string saveKey = "StarterKit_Picked";

	protected override bool IsInteractable()
	{
		if (picked)
		{
			return false;
		}
		if (!cached)
		{
			return false;
		}
		return true;
	}

	private async UniTask CacheItems()
	{
		if (caching || cached)
		{
			return;
		}
		caching = true;
		cached = false;
		itemsCache = new List<Item>();
		foreach (int item2 in items)
		{
			Item item = await ItemAssetsCollection.InstantiateAsync(item2);
			if (!(item == null))
			{
				item.transform.SetParent(base.transform);
				itemsCache.Add(item);
			}
		}
		caching = false;
		cached = true;
	}

	protected override void Awake()
	{
		base.Awake();
		SavesSystem.OnCollectSaveData += Save;
		SceneLoader.onStartedLoadingScene += OnStartedLoadingScene;
	}

	protected override void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= Save;
		SceneLoader.onStartedLoadingScene -= OnStartedLoadingScene;
		base.OnDestroy();
	}

	private void OnStartedLoadingScene(SceneLoadingContext context)
	{
		picked = false;
		Save();
	}

	private void Save()
	{
		SavesSystem.Save(saveKey, picked);
	}

	private void Load()
	{
		picked = SavesSystem.Load<bool>(saveKey);
		base.MarkerActive = !picked;
		if ((bool)notPickedItem)
		{
			notPickedItem?.SetActive(!picked);
		}
		if ((bool)pickedItem)
		{
			pickedItem.SetActive(picked);
		}
	}

	protected override void Start()
	{
		base.Start();
		Load();
		if (!picked)
		{
			CacheItems().Forget();
		}
	}

	protected override void OnInteractFinished()
	{
		foreach (Item item in itemsCache)
		{
			ItemUtilities.SendToPlayerCharacter(item);
		}
		picked = true;
		base.MarkerActive = !picked;
		itemsCache.Clear();
		OnPicked();
	}

	private void OnPicked()
	{
		if ((bool)notPickedItem)
		{
			notPickedItem.SetActive(value: false);
		}
		if ((bool)pickedItem)
		{
			pickedItem.SetActive(value: true);
		}
		if ((bool)pickFX)
		{
			pickFX.SetActive(value: true);
		}
		NotificationText.Push(notificationTextKey.ToPlainText());
	}
}
