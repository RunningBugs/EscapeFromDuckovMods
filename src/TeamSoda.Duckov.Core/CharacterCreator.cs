using Cysharp.Threading.Tasks;
using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;

public class CharacterCreator : MonoBehaviour
{
	public CharacterMainControl characterPfb => GameplayDataSettings.Prefabs.CharacterPrefab;

	public async UniTask<CharacterMainControl> CreateCharacter(Item itemInstance, CharacterModel modelPrefab, Vector3 pos, Quaternion rotation)
	{
		CharacterMainControl characterMainControl = Object.Instantiate(characterPfb, pos, rotation);
		CharacterModel characterModel = Object.Instantiate(modelPrefab);
		characterMainControl.SetCharacterModel(characterModel);
		if (itemInstance == null)
		{
			if ((bool)characterMainControl)
			{
				Object.Destroy(characterMainControl.gameObject);
			}
			return null;
		}
		characterMainControl.SetItem(itemInstance);
		if (!LevelManager.Instance.IsRaidMap)
		{
			characterMainControl.AddBuff(GameplayDataSettings.Buffs.BaseBuff);
		}
		return characterMainControl;
	}

	public async UniTask<Item> LoadOrCreateCharacterItemInstance(int itemTypeID)
	{
		return await ItemAssetsCollection.InstantiateAsync(itemTypeID);
	}
}
