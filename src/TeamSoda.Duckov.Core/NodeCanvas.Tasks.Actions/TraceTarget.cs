using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

public class TraceTarget : ActionTask<AICharacterController>
{
	public bool traceTargetTransform = true;

	[ShowIf("traceTargetTransform", 0)]
	public BBParameter<Vector3> centerPosition;

	[ShowIf("traceTargetTransform", 1)]
	public BBParameter<Transform> centerTransform;

	public BBParameter<float> stopDistance;

	public BBParameter<float> overTime = 8f;

	public bool overTimeReturnSuccess = true;

	private Vector3 targetPoint;

	public bool failIfNoPath;

	[ShowIf("failIfNoPath", 0)]
	public bool retryIfNotFound;

	private float recalculatePathTimeSpace = 0.15f;

	private float recalculatePathTimer = 0.15f;

	public bool syncDirectionIfNoAimTarget = true;

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		if (base.agent == null || (traceTargetTransform && centerTransform.value == null))
		{
			EndAction(success: false);
			return;
		}
		Vector3 pos = (traceTargetTransform ? centerTransform.value.position : centerPosition.value);
		base.agent.MoveToPos(pos);
	}

	protected override void OnUpdate()
	{
		if (base.agent == null)
		{
			EndAction(success: false);
			return;
		}
		Vector3 vector = ((traceTargetTransform && centerTransform.value != null) ? centerTransform.value.position : centerPosition.value);
		if (base.elapsedTime > overTime.value)
		{
			EndAction(overTimeReturnSuccess);
			return;
		}
		if (Vector3.Distance(vector, base.agent.transform.position) < stopDistance.value)
		{
			EndAction(success: true);
			return;
		}
		recalculatePathTimer -= Time.deltaTime;
		if (recalculatePathTimer <= 0f)
		{
			recalculatePathTimer = recalculatePathTimeSpace;
			base.agent.MoveToPos(vector);
		}
		else if (!base.agent.WaitingForPathResult())
		{
			if (!base.agent.IsMoving() || base.agent.ReachedEndOfPath())
			{
				EndAction(success: true);
				return;
			}
			if (!base.agent.HasPath())
			{
				if (!failIfNoPath && retryIfNotFound)
				{
					base.agent.MoveToPos(vector);
				}
				else
				{
					EndAction(!failIfNoPath);
				}
				return;
			}
		}
		if (syncDirectionIfNoAimTarget && base.agent.aimTarget == null)
		{
			Vector3 currentMoveDirection = base.agent.CharacterMainControl.CurrentMoveDirection;
			if (currentMoveDirection.magnitude > 0f)
			{
				base.agent.CharacterMainControl.SetAimPoint(base.agent.CharacterMainControl.transform.position + currentMoveDirection * 1000f);
			}
		}
	}

	protected override void OnStop()
	{
		base.agent.StopMove();
	}

	protected override void OnPause()
	{
	}
}
