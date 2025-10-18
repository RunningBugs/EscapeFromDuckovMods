using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

public class MoveToRandomPos : ActionTask<AICharacterController>
{
	public bool useTransform;

	public bool setAimToPos;

	[ShowIf("setAimToPos", 1)]
	public BBParameter<Vector3> aimPos;

	[ShowIf("useTransform", 0)]
	public BBParameter<Vector3> centerPos;

	[ShowIf("useTransform", 1)]
	public BBParameter<Transform> centerTransform;

	public BBParameter<float> radius;

	public BBParameter<float> avoidRadius;

	public float randomAngle = 360f;

	public BBParameter<float> overTime = 8f;

	public bool overTimeReturnSuccess = true;

	private Vector3 targetPoint;

	public bool failIfNoPath;

	[ShowIf("failIfNoPath", 0)]
	public bool retryIfNotFound;

	public bool syncDirectionIfNoAimTarget = true;

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		if (base.agent == null)
		{
			EndAction(success: false);
			return;
		}
		targetPoint = RandomPoint();
		base.agent.MoveToPos(targetPoint);
	}

	protected override void OnUpdate()
	{
		if (base.agent == null)
		{
			EndAction(success: false);
			return;
		}
		if (base.elapsedTime > overTime.value)
		{
			EndAction(overTimeReturnSuccess);
			return;
		}
		if (useTransform && centerTransform.value == null)
		{
			EndAction(success: false);
			return;
		}
		if (syncDirectionIfNoAimTarget && base.agent.aimTarget == null)
		{
			if (setAimToPos && aimPos.isDefined)
			{
				base.agent.CharacterMainControl.SetAimPoint(aimPos.value);
			}
			else
			{
				Vector3 currentMoveDirection = base.agent.CharacterMainControl.CurrentMoveDirection;
				if (currentMoveDirection.magnitude > 0f)
				{
					base.agent.CharacterMainControl.SetAimPoint(base.agent.CharacterMainControl.transform.position + currentMoveDirection * 1000f);
				}
			}
		}
		if (base.agent.WaitingForPathResult())
		{
			return;
		}
		if (base.agent.ReachedEndOfPath() || !base.agent.IsMoving())
		{
			EndAction(success: true);
		}
		else if (!base.agent.HasPath())
		{
			if (!failIfNoPath && retryIfNotFound)
			{
				targetPoint = RandomPoint();
				base.agent.MoveToPos(targetPoint);
			}
			else
			{
				EndAction(!failIfNoPath);
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

	private Vector3 RandomPoint()
	{
		Vector3 vector = base.agent.CharacterMainControl.transform.position;
		if (useTransform)
		{
			if (centerTransform.isDefined)
			{
				vector = centerTransform.value.position;
			}
		}
		else
		{
			vector = centerPos.value;
		}
		Vector3 vector2 = vector - base.agent.transform.position;
		vector2.y = 0f;
		if (vector2.magnitude < 0.1f)
		{
			vector2 = Random.insideUnitSphere;
			vector2.y = 0f;
		}
		vector2 = vector2.normalized;
		float y = Random.Range(-0.5f * randomAngle, 0.5f * randomAngle);
		float num = Random.Range(avoidRadius.value, radius.value);
		vector2 = Quaternion.Euler(0f, y, 0f) * -vector2;
		return vector + vector2 * num;
	}
}
