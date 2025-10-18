using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterSpawnerGroup : CharacterSpawnerComponentBase
{
	public CharacterSpawnerRoot spawnerRoot;

	public bool hasLeader;

	[Range(0f, 1f)]
	public float hasLeaderChance = 1f;

	public List<RandomCharacterSpawner> spawners;

	private List<AICharacterController> characters;

	private AICharacterController leaderAI;

	public AICharacterController LeaderAI => leaderAI;

	public void Collect()
	{
		spawners = GetComponentsInChildren<RandomCharacterSpawner>().ToList();
	}

	public override void Init(CharacterSpawnerRoot root)
	{
		foreach (RandomCharacterSpawner spawner in spawners)
		{
			if (spawner == null)
			{
				Debug.LogError("生成器引用为空：" + base.gameObject.name);
			}
			else
			{
				spawner.Init(root);
			}
		}
		spawnerRoot = root;
	}

	public void Awake()
	{
		characters = new List<AICharacterController>();
		if (hasLeader && Random.Range(0f, 1f) > hasLeaderChance)
		{
			hasLeader = false;
		}
	}

	private void Update()
	{
		if (!hasLeader || !(leaderAI == null) || characters.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < characters.Count; i++)
		{
			if (characters[i] == null)
			{
				characters.RemoveAt(i);
				i--;
			}
			else
			{
				leaderAI = characters[i];
			}
		}
	}

	public void AddCharacterSpawned(AICharacterController _character, bool isLeader)
	{
		_character.group = this;
		if (isLeader)
		{
			leaderAI = _character;
		}
		else if (hasLeader && !leaderAI)
		{
			leaderAI = _character;
		}
		characters.Add(_character);
	}

	public override void StartSpawn()
	{
		bool flag = true;
		foreach (RandomCharacterSpawner spawner in spawners)
		{
			if (!(spawner == null))
			{
				spawner.masterGroup = this;
				if (flag && hasLeader)
				{
					spawner.firstIsLeader = true;
				}
				flag = false;
				spawner.StartSpawn();
			}
		}
	}
}
