using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using UnityEngine;

public class DynamicItemDebugger : MonoBehaviour
{
	[SerializeField]
	private List<Item> prefabs;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		Add();
	}

	private void Add()
	{
		foreach (Item prefab in prefabs)
		{
			ItemAssetsCollection.AddDynamicEntry(prefab);
		}
	}

	private void CreateCorresponding()
	{
		CreateTask().Forget();
	}

	private async UniTask CreateTask()
	{
		foreach (Item prefab in prefabs)
		{
			Item item = await ItemAssetsCollection.InstantiateAsync(prefab.TypeID);
			item.transform.SetParent(base.transform);
			if ((bool)CharacterMainControl.Main)
			{
				ItemUtilities.SendToPlayer(item);
			}
		}
	}
}
