using Duckov;
using UnityEngine;

public class CA_Interact : CharacterActionBase, IProgress
{
	private InteractableBase masterInteractableAround;

	private int interactIndexInGroup;

	private InteractableBase interactingTarget;

	private LayerMask interactLayers;

	private InteractableBase minDistanceInteractable;

	private Collider[] colliders;

	private Progress progress;

	public InteractableBase MasterInteractableAround => masterInteractableAround;

	public InteractableBase InteractTarget
	{
		get
		{
			if ((bool)masterInteractableAround)
			{
				return masterInteractableAround.GetInteractableInGroup(interactIndexInGroup);
			}
			return null;
		}
	}

	public int InteractIndexInGroup => interactIndexInGroup;

	public InteractableBase InteractingTarget => interactingTarget;

	private void Awake()
	{
		interactLayers = 1 << LayerMask.NameToLayer("Interactable");
		colliders = new Collider[5];
	}

	public void SearchInteractableAround()
	{
		if (!characterController.IsMainCharacter)
		{
			return;
		}
		InteractableBase interactableBase = masterInteractableAround;
		int num = Physics.OverlapSphereNonAlloc(base.transform.position + Vector3.up * 0.5f + characterController.CurrentAimDirection * 0.2f, 0.3f, colliders, interactLayers);
		if (num <= 0)
		{
			masterInteractableAround = null;
			return;
		}
		float num2 = 999f;
		float num3 = 0f;
		minDistanceInteractable = null;
		for (int i = 0; i < num; i++)
		{
			Collider collider = colliders[i];
			num3 = Vector3.Distance(base.transform.position, collider.transform.position);
			if (num3 < num2)
			{
				InteractableBase interactableBase2 = null;
				if (masterInteractableAround == null || masterInteractableAround.gameObject != collider.gameObject)
				{
					interactableBase2 = collider.GetComponent<InteractableBase>();
				}
				else if (masterInteractableAround != null && masterInteractableAround.gameObject == collider.gameObject)
				{
					interactableBase2 = masterInteractableAround;
				}
				if (!(interactableBase2 == null) && interactableBase2.CheckInteractable())
				{
					minDistanceInteractable = interactableBase2;
					num2 = num3;
				}
			}
		}
		masterInteractableAround = minDistanceInteractable;
		if (interactableBase != masterInteractableAround || interactableBase == null)
		{
			interactIndexInGroup = 0;
		}
	}

	public void SwitchInteractable(int dir)
	{
		if (MasterInteractableAround == null || !MasterInteractableAround.interactableGroup)
		{
			interactIndexInGroup = 0;
			return;
		}
		interactIndexInGroup += dir;
		int num = 1;
		if (MasterInteractableAround.interactableGroup)
		{
			num = MasterInteractableAround.GetInteractableList().Count;
		}
		if (interactIndexInGroup >= num)
		{
			interactIndexInGroup = 0;
		}
		else if (interactIndexInGroup < 0)
		{
			interactIndexInGroup = num - 1;
		}
	}

	public void SetInteractableTarget(InteractableBase interactable)
	{
		masterInteractableAround = interactable;
		interactIndexInGroup = 0;
	}

	protected override bool OnStart()
	{
		InteractableBase interactTarget = InteractTarget;
		if (!interactTarget)
		{
			return false;
		}
		if (interactTarget.StartInteract(characterController))
		{
			interactingTarget = interactTarget;
			return true;
		}
		return false;
	}

	protected override void OnUpdateAction(float deltaTime)
	{
		if (interactingTarget == null)
		{
			StopAction();
		}
		else if (!interactingTarget.Interacting)
		{
			StopAction();
		}
		else
		{
			interactingTarget.UpdateInteract(characterController, deltaTime);
		}
	}

	public override ActionPriorities ActionPriority()
	{
		return ActionPriorities.Interact;
	}

	public override bool CanMove()
	{
		return false;
	}

	protected override void OnStop()
	{
		if ((bool)interactingTarget && interactingTarget.Interacting)
		{
			interactingTarget.InternalStopInteract();
		}
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
		return false;
	}

	public override bool IsReady()
	{
		if (!base.Running)
		{
			return InteractTarget != null;
		}
		return false;
	}

	public Progress GetProgress()
	{
		if (interactingTarget != null)
		{
			progress = interactingTarget.GetProgress();
		}
		else
		{
			progress.inProgress = false;
		}
		return progress;
	}
}
