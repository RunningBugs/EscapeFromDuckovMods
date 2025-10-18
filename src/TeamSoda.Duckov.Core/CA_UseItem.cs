using Duckov;
using FMOD.Studio;
using ItemStatsSystem;
using UnityEngine;

public class CA_UseItem : CharacterActionBase, IProgress
{
	private Item item;

	public IAgentUsable agentUsable;

	public bool hasSound;

	public string actionSound;

	public string useSound;

	private EventInstance? soundInstance;

	public override ActionPriorities ActionPriority()
	{
		return ActionPriorities.usingItem;
	}

	public override bool CanMove()
	{
		return true;
	}

	public override bool CanRun()
	{
		return false;
	}

	public override bool CanUseHand()
	{
		return false;
	}

	public override bool CanControlAim()
	{
		return true;
	}

	public override bool IsReady()
	{
		return true;
	}

	protected override bool OnStart()
	{
		agentUsable = null;
		bool flag = false;
		if (item.AgentUtilities.ActiveAgent == null)
		{
			if (characterController.ChangeHoldItem(item) && characterController.CurrentHoldItemAgent != null)
			{
				agentUsable = characterController.CurrentHoldItemAgent as IAgentUsable;
				flag = true;
			}
		}
		else if (item.AgentUtilities.ActiveAgent == characterController.CurrentHoldItemAgent)
		{
			flag = true;
		}
		if (flag)
		{
			PostActionSound();
		}
		return flag;
	}

	protected override void OnStop()
	{
		StopSound();
		characterController.SwitchToWeaponBeforeUse();
		if (item != null && !item.IsBeingDestroyed && item.GetRoot() != characterController.CharacterItem && !characterController.PickupItem(item))
		{
			item.Drop(characterController, createRigidbody: true);
		}
	}

	public void SetUseItem(Item _item)
	{
		item = _item;
		hasSound = false;
		UsageUtilities component = item.GetComponent<UsageUtilities>();
		if ((bool)component)
		{
			hasSound = component.hasSound;
			actionSound = component.actionSound;
			useSound = component.useSound;
		}
	}

	protected override void OnUpdateAction(float deltaTime)
	{
		if (item == null)
		{
			StopAction();
		}
		else if (characterController.CurrentHoldItemAgent == null || characterController.CurrentHoldItemAgent.Item == null || characterController.CurrentHoldItemAgent.Item != item)
		{
			Debug.Log("拿的不统一");
			StopAction();
		}
		else if (base.ActionTimer > characterController.CurrentHoldItemAgent.Item.UseTime)
		{
			OnFinish();
			Debug.Log("Use Finished");
			StopAction();
		}
	}

	private void OnFinish()
	{
		item.Use(characterController);
		PostUseSound();
		if (item.Stackable)
		{
			item.StackCount -= 1;
		}
		else if (item.UseDurability)
		{
			if (item.Durability <= 0f && !item.IsBeingDestroyed)
			{
				item.DestroyTree();
			}
		}
		else
		{
			item.DestroyTree();
		}
	}

	public Progress GetProgress()
	{
		Progress result = default(Progress);
		if (item != null && base.Running)
		{
			result.inProgress = true;
			result.total = item.UseTime;
			result.current = actionTimer;
			return result;
		}
		result.inProgress = false;
		return result;
	}

	private void OnDestroy()
	{
		StopSound();
	}

	private void OnDisable()
	{
		StopSound();
	}

	private void PostActionSound()
	{
		if (hasSound)
		{
			soundInstance = AudioManager.Post(actionSound, base.gameObject);
		}
	}

	private void PostUseSound()
	{
		if (hasSound)
		{
			AudioManager.Post(useSound, base.gameObject);
		}
	}

	private void StopSound()
	{
		if (soundInstance.HasValue)
		{
			soundInstance.Value.stop(STOP_MODE.ALLOWFADEOUT);
		}
	}
}
