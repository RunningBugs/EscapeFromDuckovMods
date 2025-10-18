using System;
using System.Collections.Generic;
using Duckov.Utilities;
using UnityEngine;

namespace ItemStatsSystem;

[Serializable]
public class ItemAgentUtilities
{
	[Serializable]
	public class AgentKeyPair
	{
		public string key;

		public ItemAgent agentPrefab;

		private StringList avaliableKeys => StringLists.ItemAgentKeys;
	}

	private Item master;

	private ItemAgent activeAgent;

	[SerializeField]
	private List<AgentKeyPair> agents;

	private Dictionary<int, AgentKeyPair> hashedAgentsCache;

	public Item Master => master;

	public ItemAgent ActiveAgent => activeAgent;

	private Dictionary<int, AgentKeyPair> HashedAgents
	{
		get
		{
			if (hashedAgentsCache == null)
			{
				hashedAgentsCache = new Dictionary<int, AgentKeyPair>();
				foreach (AgentKeyPair agent in agents)
				{
					hashedAgentsCache.Add(agent.key.GetHashCode(), agent);
				}
			}
			return hashedAgentsCache;
		}
	}

	public ItemAgent this[int hash] => GetPrefab(hash);

	public ItemAgent this[string key] => GetPrefab(key);

	public event Action<Item, ItemAgent> onCreateAgent;

	public ItemAgent GetPrefab(int hash)
	{
		if (HashedAgents.TryGetValue(hash, out var value))
		{
			return value.agentPrefab;
		}
		return null;
	}

	public ItemAgent GetPrefab(string key)
	{
		return GetPrefab(key.GetHashCode());
	}

	public void Initialize(Item master)
	{
		this.master = master;
	}

	public ItemAgent CreateAgent(int hash, ItemAgent.AgentTypes agentType)
	{
		ItemAgent prefab = GetPrefab(hash);
		return CreateAgent(prefab, agentType);
	}

	public ItemAgent CreateAgent(ItemAgent prefab, ItemAgent.AgentTypes agentType)
	{
		if (prefab == null)
		{
			return null;
		}
		if (Master == null)
		{
			Debug.Log("Create agent:" + prefab.name + " failed,master is null");
			return null;
		}
		if (ActiveAgent != null)
		{
			ReleaseActiveAgent();
			Debug.Log("Creating agent:" + prefab.name + ",destrory another agent");
		}
		ItemAgent itemAgent = (activeAgent = UnityEngine.Object.Instantiate(prefab));
		itemAgent.Initialize(Master, agentType);
		this.onCreateAgent?.Invoke(Master, itemAgent);
		return itemAgent;
	}

	public void ReleaseActiveAgent()
	{
		if (!(ActiveAgent == null))
		{
			UnityEngine.Object.Destroy(ActiveAgent.gameObject);
			activeAgent = null;
		}
	}
}
