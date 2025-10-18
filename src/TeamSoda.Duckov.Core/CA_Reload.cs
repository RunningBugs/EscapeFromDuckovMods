using Duckov;
using ItemStatsSystem;

public class CA_Reload : CharacterActionBase, IProgress
{
	public ItemAgent_Gun currentGun;

	public Item preferedBulletToReload;

	public override ActionPriorities ActionPriority()
	{
		return ActionPriorities.Reload;
	}

	public override bool CanMove()
	{
		return true;
	}

	public override bool CanRun()
	{
		return true;
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
		currentGun = characterController.agentHolder.CurrentHoldGun;
		if (!currentGun)
		{
			return false;
		}
		if (currentGun.IsReloading())
		{
			return false;
		}
		return true;
	}

	protected override bool OnStart()
	{
		currentGun = null;
		if (!characterController || !characterController.CurrentHoldItemAgent)
		{
			return false;
		}
		currentGun = characterController.agentHolder.CurrentHoldGun;
		currentGun.GunItemSetting.PreferdBulletsToLoad = preferedBulletToReload;
		preferedBulletToReload = null;
		if (currentGun != null && currentGun.BeginReload())
		{
			return true;
		}
		return false;
	}

	protected override void OnStop()
	{
		if (currentGun != null)
		{
			currentGun.CancleReload();
		}
	}

	public bool GetGunReloadable()
	{
		if (currentGun == null)
		{
			currentGun = characterController.agentHolder.CurrentHoldGun;
			return false;
		}
		if (base.Running)
		{
			return false;
		}
		if (currentGun.IsFull())
		{
			return false;
		}
		return true;
	}

	public override bool CanEditInventory()
	{
		return true;
	}

	protected override void OnUpdateAction(float deltaTime)
	{
		if (currentGun == null)
		{
			StopAction();
		}
		else if (!currentGun.IsReloading())
		{
			StopAction();
		}
	}

	public Progress GetProgress()
	{
		if (currentGun != null)
		{
			return currentGun.GetReloadProgress();
		}
		return new Progress
		{
			inProgress = false
		};
	}
}
