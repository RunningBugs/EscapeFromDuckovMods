using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using UnityEngine;

public class AISpecialAttachment_SpawnItemOnCritKill : AISpecialAttachmentBase
{
	[ItemTypeID]
	public int itemToSpawn;

	private Item itemInstance;

	private bool hasDead;

	public bool inverse;

	protected override void OnInited()
	{
		character.BeforeCharacterSpawnLootOnDead += BeforeCharacterSpawnLootOnDead;
		SpawnItem().Forget();
	}

	private async UniTaskVoid SpawnItem()
	{
		itemInstance = await ItemAssetsCollection.InstantiateAsync(itemToSpawn);
		itemInstance.transform.SetParent(base.transform, worldPositionStays: false);
		if (hasDead)
		{
			Object.Destroy(itemInstance.gameObject);
		}
	}

	private void OnDestroy()
	{
		if ((bool)character)
		{
			character.BeforeCharacterSpawnLootOnDead -= BeforeCharacterSpawnLootOnDead;
		}
	}

	private void BeforeCharacterSpawnLootOnDead(DamageInfo dmgInfo)
	{
		hasDead = true;
		Debug.Log($"Die crit:{dmgInfo.crit}");
		bool flag = dmgInfo.crit > 0;
		if (inverse == flag || character == null)
		{
			if (itemInstance != null)
			{
				Object.Destroy(itemInstance.gameObject);
			}
			return;
		}
		Debug.Log("pick up on crit");
		if (itemInstance != null)
		{
			character.CharacterItem.Inventory.AddAndMerge(itemInstance);
		}
	}
}
