using UnityEngine;

namespace ItemStatsSystem;

[AddComponentMenu("ItemAgent(不要用，用DuckovItemAgent)")]
public class ItemAgent : MonoBehaviour
{
	public enum AgentTypes
	{
		normal,
		pickUp,
		handheld,
		equipment
	}

	private Item item;

	protected AgentTypes agentType;

	public AgentTypes AgentType => agentType;

	public Item Item => item;

	public void Initialize(Item item, AgentTypes _agentType)
	{
		this.item = item;
		agentType = _agentType;
		item.onUnpluggedFromSlot += OnUnplugedFromSlot;
		OnInitialize();
	}

	protected virtual void OnDestroy()
	{
		if (item != null)
		{
			item.onUnpluggedFromSlot -= OnUnplugedFromSlot;
		}
	}

	private void OnUnplugedFromSlot(Item item)
	{
		if (!(item == null) && item.AgentUtilities != null && !(item.AgentUtilities.ActiveAgent == null))
		{
			if (item.AgentUtilities.ActiveAgent != this)
			{
				Debug.LogError("release的对象是" + item.AgentUtilities.ActiveAgent.gameObject.name + ",而调用者是" + base.gameObject.name);
			}
			item.AgentUtilities.ReleaseActiveAgent();
		}
	}

	protected virtual void OnInitialize()
	{
	}
}
