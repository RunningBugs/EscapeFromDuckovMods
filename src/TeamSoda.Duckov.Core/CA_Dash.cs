using Duckov;
using UnityEngine;

public class CA_Dash : CharacterActionBase, IProgress
{
	private float dashSpeed;

	private bool dashCanControl;

	public AnimationCurve speedCurve;

	public float dashTime;

	public float coolTime = 0.5f;

	private Vector3 dashDirection;

	public float staminaCost = 10f;

	private float lastEndTime = -999f;

	[SerializeField]
	private string overrideSFX;

	private string sfx
	{
		get
		{
			if (string.IsNullOrWhiteSpace(overrideSFX))
			{
				return "Char/Footstep/dash";
			}
			return overrideSFX;
		}
	}

	public override ActionPriorities ActionPriority()
	{
		return ActionPriorities.Dash;
	}

	public override bool CanMove()
	{
		return false;
	}

	public override bool CanRun()
	{
		return false;
	}

	public override bool CanUseHand()
	{
		return dashCanControl;
	}

	public override bool CanControlAim()
	{
		return dashCanControl;
	}

	public Progress GetProgress()
	{
		Progress result = default(Progress);
		if (base.Running)
		{
			result.inProgress = true;
			result.total = dashTime;
			result.current = actionTimer;
		}
		else
		{
			result.inProgress = false;
		}
		return result;
	}

	public override bool IsReady()
	{
		if (Time.time - lastEndTime < coolTime)
		{
			return false;
		}
		return !base.Running;
	}

	protected override bool OnStart()
	{
		if (characterController.CurrentStamina < staminaCost)
		{
			return false;
		}
		characterController.UseStamina(staminaCost);
		dashSpeed = characterController.DashSpeed;
		dashCanControl = characterController.DashCanControl;
		if (characterController.MoveInput.magnitude > 0f)
		{
			dashDirection = characterController.MoveInput.normalized;
		}
		else
		{
			dashDirection = characterController.CurrentAimDirection;
		}
		characterController.SetForceMoveVelocity(dashSpeed * speedCurve.Evaluate(0f) * dashDirection);
		if (!dashCanControl)
		{
			characterController.movementControl.ForceTurnTo(dashDirection);
		}
		AudioManager.Post(sfx, base.gameObject);
		return true;
	}

	protected override void OnStop()
	{
		characterController.SetForceMoveVelocity(characterController.CharacterRunSpeed * dashDirection);
		lastEndTime = Time.time;
	}

	protected override void OnUpdateAction(float deltaTime)
	{
		if ((!(actionTimer > dashTime) && base.Running) || !StopAction())
		{
			characterController.SetForceMoveVelocity(dashSpeed * speedCurve.Evaluate(Mathf.Clamp01(actionTimer / dashTime)) * dashDirection);
		}
	}
}
