using Duckov;
using UnityEngine;

public class CA_Carry : CharacterActionBase, IProgress
{
	[HideInInspector]
	public Carriable carryTarget;

	private Carriable carringTarget;

	public Vector3 carryPoint = new Vector3(0f, 1f, 0.8f);

	public override ActionPriorities ActionPriority()
	{
		return ActionPriorities.Whatever;
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
		return carryTarget != null;
	}

	public float GetWeight()
	{
		if (!base.Running)
		{
			return 0f;
		}
		if (!carringTarget)
		{
			return 0f;
		}
		return carringTarget.GetWeight();
	}

	public Progress GetProgress()
	{
		return new Progress
		{
			inProgress = false,
			total = 1f,
			current = 1f
		};
	}

	protected override bool OnStart()
	{
		characterController.ChangeHoldItem(null);
		carryTarget.Take(this);
		carringTarget = carryTarget;
		return true;
	}

	protected override void OnUpdateAction(float deltaTime)
	{
		if (characterController.CurrentHoldItemAgent != null)
		{
			StopAction();
		}
		if ((bool)carryTarget)
		{
			carryTarget.OnCarriableUpdate(deltaTime);
		}
	}

	protected override void OnStop()
	{
		carryTarget.Drop();
		carringTarget = null;
	}
}
