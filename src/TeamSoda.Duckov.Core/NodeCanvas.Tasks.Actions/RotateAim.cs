using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

public class RotateAim : ActionTask<AICharacterController>
{
	private Vector3 startDir;

	public float angle;

	private float currentAngle;

	public BBParameter<Vector2> timeRange;

	private float time;

	public bool shoot;

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		time = Random.Range(timeRange.value.x, timeRange.value.y);
		startDir = base.agent.CharacterMainControl.CurrentAimDirection;
		base.agent.SetTarget(null);
		if (shoot)
		{
			base.agent.CharacterMainControl.Trigger(trigger: true, triggerThisFrame: true, releaseThisFrame: false);
		}
	}

	protected override void OnUpdate()
	{
		currentAngle = angle * base.elapsedTime / time;
		Vector3 vector = Quaternion.Euler(0f, currentAngle, 0f) * startDir;
		base.agent.CharacterMainControl.SetAimPoint(base.agent.CharacterMainControl.transform.position + vector * 100f);
		if (shoot)
		{
			base.agent.CharacterMainControl.Trigger(trigger: true, triggerThisFrame: true, releaseThisFrame: false);
		}
		if (base.elapsedTime > time)
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
