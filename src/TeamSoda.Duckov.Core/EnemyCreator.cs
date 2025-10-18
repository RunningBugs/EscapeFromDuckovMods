using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.Scenes;
using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;

public class EnemyCreator : MonoBehaviour
{
	private CharacterMainControl character;

	[SerializeField]
	private List<RandomItemGenerateDescription> itemsToGenerate;

	[SerializeField]
	private ItemFilter bulletFilter;

	[SerializeField]
	private AudioManager.VoiceType voiceType;

	[SerializeField]
	private CharacterModel characterModel;

	[SerializeField]
	private AICharacterController aiController;

	private int characterItemTypeID => GameplayDataSettings.ItemAssets.DefaultCharacterItemTypeID;

	private void Start()
	{
		Debug.LogError("This scripts shouldn't exist!", this);
		if (LevelManager.LevelInited)
		{
			StartCreate();
		}
		else
		{
			LevelManager.OnLevelInitialized += StartCreate;
		}
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelInitialized -= StartCreate;
	}

	private void StartCreate()
	{
		int creatorID = GetCreatorID();
		if (MultiSceneCore.Instance != null)
		{
			if (MultiSceneCore.Instance.usedCreatorIds.Contains(creatorID))
			{
				return;
			}
			MultiSceneCore.Instance.usedCreatorIds.Add(creatorID);
		}
		CreateCharacterAsync();
	}

	private async UniTaskVoid CreateCharacterAsync()
	{
		Item characterItemInstance = await LoadOrCreateCharacterItemInstance();
		List<Item> initialItems = await GenerateItems();
		character = await LevelManager.Instance.CharacterCreator.CreateCharacter(characterItemInstance, characterModel, base.transform.position, base.transform.rotation);
		character.SetTeam(Teams.scav);
		if ((bool)aiController)
		{
			Object.Instantiate(aiController).Init(character, base.transform.position, voiceType);
		}
		await UniTask.NextFrame();
		if (initialItems != null)
		{
			foreach (Item item in initialItems)
			{
				if (!(item == null) && !characterItemInstance.TryPlug(item) && !characterItemInstance.Inventory.AddAndMerge(item))
				{
					item.DestroyTree();
				}
			}
		}
		await AddBullet();
		PlugAccessories();
		if (MultiSceneCore.MainScene.HasValue && MultiSceneCore.MainScene.Value != base.gameObject.scene)
		{
			MultiSceneCore.MoveToActiveWithScene(base.gameObject);
		}
	}

	private void PlugAccessories()
	{
		Item item = character.PrimWeaponSlot()?.Content;
		if (item == null)
		{
			return;
		}
		Inventory inventory = character?.CharacterItem?.Inventory;
		if (inventory == null)
		{
			return;
		}
		foreach (Item item2 in inventory)
		{
			if (!(item2 == null))
			{
				item.TryPlug(item2, emptyOnly: true);
			}
		}
	}

	private async UniTask AddBullet()
	{
		Item item = character.PrimWeaponSlot()?.Content;
		if (!(item != null))
		{
			return;
		}
		string text = item.Constants.GetString("Caliber");
		if (!string.IsNullOrEmpty(text))
		{
			bulletFilter.caliber = text;
			int[] array = ItemAssetsCollection.Search(bulletFilter);
			if (array.Length >= 1)
			{
				Item item2 = await ItemAssetsCollection.InstantiateAsync(array.GetRandom());
				character?.CharacterItem?.Inventory?.AddItem(item2);
			}
		}
	}

	private async UniTask<List<Item>> GenerateItems()
	{
		List<Item> items = new List<Item>();
		foreach (RandomItemGenerateDescription item in itemsToGenerate)
		{
			items.AddRange(await item.Generate());
		}
		return items;
	}

	private async UniTask<Item> LoadOrCreateCharacterItemInstance()
	{
		return await ItemAssetsCollection.InstantiateAsync(characterItemTypeID);
	}

	private int GetCreatorID()
	{
		Transform parent = base.transform.parent;
		string arg = base.transform.GetSiblingIndex().ToString();
		while (parent != null)
		{
			arg = $"{parent.GetSiblingIndex()}/{arg}";
			parent = parent.parent;
		}
		arg = $"{base.gameObject.scene.buildIndex}/{arg}";
		return arg.GetHashCode();
	}
}
