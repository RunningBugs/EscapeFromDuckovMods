using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterSpawnerGroupSelector : CharacterSpawnerComponentBase
{
	public CharacterSpawnerRoot spawnerRoot;

	public List<CharacterSpawnerGroup> groups;

	public Vector2Int spawnGroupCountRange = new Vector2Int(1, 1);

	private int finalCount;

	public void Collect()
	{
		groups = GetComponentsInChildren<CharacterSpawnerGroup>().ToList();
		foreach (CharacterSpawnerGroup group in groups)
		{
			group.Collect();
		}
	}

	public override void Init(CharacterSpawnerRoot root)
	{
		foreach (CharacterSpawnerGroup group in groups)
		{
			if (group == null)
			{
				Debug.LogError("生成器引用为空");
			}
			else
			{
				group.Init(root);
			}
		}
		spawnerRoot = root;
	}

	public override void StartSpawn()
	{
		if (spawnGroupCountRange.y > groups.Count)
		{
			spawnGroupCountRange.y = groups.Count;
		}
		if (spawnGroupCountRange.x > groups.Count)
		{
			spawnGroupCountRange.x = groups.Count;
		}
		RandomSpawn(finalCount = Random.Range(spawnGroupCountRange.x, spawnGroupCountRange.y));
	}

	private void OnValidate()
	{
		if (groups.Count >= 0 && spawnGroupCountRange.x > spawnGroupCountRange.y)
		{
			spawnGroupCountRange.y = spawnGroupCountRange.x;
		}
	}

	public void RandomSpawn(int count)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < groups.Count; i++)
		{
			list.Add(i);
		}
		for (int j = 0; j < count; j++)
		{
			int index = Random.Range(0, list.Count);
			int index2 = list[index];
			list.RemoveAt(index);
			CharacterSpawnerGroup characterSpawnerGroup = groups[index2];
			if ((bool)characterSpawnerGroup)
			{
				characterSpawnerGroup.StartSpawn();
			}
		}
	}
}
