using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;

public class ItemSetting_Accessory : ItemSettingBase
{
	[SerializeField]
	private AccessoryBase accessoryPfb;

	public ADSAimMarker overrideAdsAimMarker;

	private AccessoryBase accessoryInstance;

	private bool created;

	private Item masterItem;

	public override void SetMarkerParam(Item selfItem)
	{
		selfItem.SetBool("IsBullet", value: true);
	}

	public override void OnInit()
	{
		base.Item.onPluggedIntoSlot += OnPluggedIntoSlot;
		base.Item.onUnpluggedFromSlot += OnUnpluggedIntoSlot;
	}

	private void OnPluggedIntoSlot(Item selfItem)
	{
		Slot pluggedIntoSlot = selfItem.PluggedIntoSlot;
		if (pluggedIntoSlot != null)
		{
			masterItem = pluggedIntoSlot.Master;
			if ((bool)masterItem)
			{
				masterItem.AgentUtilities.onCreateAgent += OnMasterCreateAgent;
				CreateAccessory(masterItem.AgentUtilities.ActiveAgent as DuckovItemAgent);
			}
		}
	}

	private void OnUnpluggedIntoSlot(Item selfItem)
	{
		if ((bool)masterItem)
		{
			masterItem.AgentUtilities.onCreateAgent -= OnMasterCreateAgent;
		}
		DestroyAccessory();
	}

	private void OnDestroy()
	{
		if ((bool)masterItem)
		{
			masterItem.AgentUtilities.onCreateAgent -= OnMasterCreateAgent;
		}
		DestroyAccessory();
	}

	private void OnMasterCreateAgent(Item _masterItem, ItemAgent newAgnet)
	{
		if (masterItem != _masterItem)
		{
			Debug.LogError("缓存了错误的Item");
		}
		if (newAgnet.AgentType == ItemAgent.AgentTypes.handheld)
		{
			CreateAccessory(newAgnet as DuckovItemAgent);
		}
	}

	private void CreateAccessory(DuckovItemAgent parentAgent)
	{
		DestroyAccessory();
		if (!(accessoryPfb == null) && !(parentAgent == null) && parentAgent.AgentType == ItemAgent.AgentTypes.handheld)
		{
			accessoryInstance = Object.Instantiate(accessoryPfb);
			accessoryInstance.Init(parentAgent, base.Item);
		}
	}

	private void DestroyAccessory()
	{
		if ((bool)accessoryInstance)
		{
			Object.Destroy(accessoryInstance.gameObject);
		}
	}
}
