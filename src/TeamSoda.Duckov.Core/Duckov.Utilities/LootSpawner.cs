using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.Scenes;
using ItemStatsSystem;
using UnityEngine;

namespace Duckov.Utilities;

[RequireComponent(typeof(Points))]
public class LootSpawner : MonoBehaviour
{
	[Serializable]
	private struct Entry
	{
		[ItemTypeID]
		[SerializeField]
		public int itemTypeID;
	}

	[Range(0f, 1f)]
	public float spawnChance = 1f;

	public bool randomGenrate = true;

	public bool randomFromPool;

	[SerializeField]
	private Vector2Int randomCount = new Vector2Int(1, 1);

	[SerializeField]
	private RandomContainer<Tag> tags;

	[SerializeField]
	private List<Tag> excludeTags;

	[SerializeField]
	private RandomContainer<int> qualities;

	[SerializeField]
	private RandomContainer<Entry> randomPool;

	[ItemTypeID]
	[SerializeField]
	private List<int> fixedItems;

	[SerializeField]
	private Points points;

	private bool loading;

	[SerializeField]
	[ItemTypeID]
	private List<int> typeIds;

	public bool RandomFromPool
	{
		get
		{
			if (randomGenrate)
			{
				return randomFromPool;
			}
			return false;
		}
	}

	public bool RandomButNotFromPool
	{
		get
		{
			if (randomGenrate)
			{
				return !randomFromPool;
			}
			return false;
		}
	}

	public void CalculateChances()
	{
		tags.RefreshPercent();
		qualities.RefreshPercent();
		randomPool.RefreshPercent();
	}

	private void Start()
	{
		if (points == null)
		{
			points = GetComponent<Points>();
		}
		bool flag = false;
		int key = GetKey();
		if (MultiSceneCore.Instance.inLevelData.TryGetValue(key, out var value) && value is bool flag2)
		{
			flag = flag2;
		}
		if (!flag)
		{
			if (UnityEngine.Random.Range(0f, 1f) <= spawnChance)
			{
				Setup().Forget();
			}
			flag = true;
			MultiSceneCore.Instance.inLevelData.Add(key, true);
		}
	}

	private int GetKey()
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

	public async UniTask Setup()
	{
		typeIds.Clear();
		if (randomGenrate && !randomFromPool)
		{
			int num = UnityEngine.Random.Range(randomCount.x, randomCount.y);
			if (!excludeTags.Contains(GameplayDataSettings.Tags.Special))
			{
				excludeTags.Add(GameplayDataSettings.Tags.Special);
			}
			if (!LevelManager.Rule.AdvancedDebuffMode && !excludeTags.Contains(GameplayDataSettings.Tags.AdvancedDebuffMode))
			{
				excludeTags.Add(GameplayDataSettings.Tags.AdvancedDebuffMode);
			}
			for (int i = 0; i < num; i++)
			{
				Tag random = tags.GetRandom();
				int random2 = qualities.GetRandom();
				int[] array = Search(new ItemFilter
				{
					requireTags = new Tag[1] { random },
					excludeTags = excludeTags.ToArray(),
					minQuality = random2,
					maxQuality = random2
				});
				if (array.Length >= 1)
				{
					int random3 = array.GetRandom();
					typeIds.Add(random3);
				}
			}
		}
		else if (randomGenrate && randomFromPool)
		{
			int num2 = UnityEngine.Random.Range(randomCount.x, randomCount.y);
			for (int j = 0; j < num2; j++)
			{
				Entry random4 = randomPool.GetRandom();
				typeIds.Add(random4.itemTypeID);
			}
		}
		else
		{
			typeIds.AddRange(fixedItems);
		}
		loading = true;
		int idCount = typeIds.Count;
		List<Vector3> spawnPoints = points.GetRandomPoints(idCount);
		for (int k = 0; k < idCount; k++)
		{
			(await ItemAssetsCollection.InstantiateAsync(typeIds[k])).Drop(spawnPoints[k] + Vector3.up * 0.5f, createRigidbody: false, Vector3.up, 360f);
		}
		loading = false;
	}

	public static int[] Search(ItemFilter filter)
	{
		return ItemAssetsCollection.Search(filter);
	}

	private void OnValidate()
	{
		if (points == null)
		{
			points = GetComponent<Points>();
		}
	}
}
