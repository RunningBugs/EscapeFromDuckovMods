using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

public class Shoot : ActionTask<AICharacterController>
{
	public BBParameter<Vector2> shootTimeRange;

	private float shootTime;

	public float semiTimeSpace = 0.35f;

	private float semiTimer;

	protected override string info => $"Shoot {shootTimeRange.value.x}to{shootTimeRange.value.y} sec.";

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		semiTimer = semiTimeSpace;
		base.agent.CharacterMainControl.Trigger(trigger: true, triggerThisFrame: true, releaseThisFrame: false);
		if (!base.agent.shootCanMove)
		{
			base.agent.StopMove();
		}
		shootTime = Random.Range(shootTimeRange.value.x, shootTimeRange.value.y);
		if (shootTime <= 0f)
		{
			EndAction(success: true);
		}
	}

	protected override void OnUpdate()
	{
		bool triggerThisFrame = false;
		semiTimer += Time.deltaTime;
		if (!base.agent.shootCanMove)
		{
			base.agent.StopMove();
		}
		if (semiTimer >= semiTimeSpace)
		{
			semiTimer = 0f;
			triggerThisFrame = true;
		}
		base.agent.CharacterMainControl.Trigger(trigger: true, triggerThisFrame, releaseThisFrame: false);
		if (base.elapsedTime >= shootTime)
		{
			EndAction(success: true);
		}
	}

	protected override void OnStop()
	{
		base.agent.CharacterMainControl.Trigger(trigger: false, triggerThisFrame: false, releaseThisFrame: false);
	}

	protected override void OnPause()
	{
		base.agent.CharacterMainControl.Trigger(trigger: false, triggerThisFrame: false, releaseThisFrame: false);
	}
}
