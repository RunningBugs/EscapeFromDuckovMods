using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

public class Dash : ActionTask<AICharacterController>
{
	public enum DashDirectionModes
	{
		random,
		targetTransform
	}

	public DashDirectionModes directionMode;

	[ShowIf("directionMode", 1)]
	public BBParameter<Transform> targetTransform;

	[ShowIf("directionMode", 1)]
	public bool verticle;

	public BBParameter<Vector2> dashTimeSpaceRange;

	private float dashTimeSpace;

	private float lastDashTime = -999f;

	protected override string info => $"Dash";

	protected override string OnInit()
	{
		dashTimeSpace = Random.Range(dashTimeSpaceRange.value.x, dashTimeSpaceRange.value.y);
		return null;
	}

	protected override void OnExecute()
	{
		if (Time.time - lastDashTime < dashTimeSpace)
		{
			EndAction();
			return;
		}
		lastDashTime = Time.time;
		dashTimeSpace = Random.Range(dashTimeSpaceRange.value.x, dashTimeSpaceRange.value.y);
		Vector3 vector = Vector3.forward;
		switch (directionMode)
		{
		case DashDirectionModes.random:
			vector = Random.insideUnitCircle;
			vector.z = vector.y;
			vector.y = 0f;
			vector.Normalize();
			break;
		case DashDirectionModes.targetTransform:
			if (targetTransform.value == null)
			{
				EndAction();
				return;
			}
			vector = targetTransform.value.position - base.agent.transform.position;
			vector.y = 0f;
			vector.Normalize();
			if (verticle)
			{
				vector = Vector3.Cross(vector, Vector3.up) * ((Random.Range(0f, 1f) > 0.5f) ? 1f : (-1f));
			}
			break;
		}
		base.agent.CharacterMainControl.SetMoveInput(vector);
		base.agent.CharacterMainControl.Dash();
		EndAction(success: true);
	}

	protected override void OnStop()
	{
	}

	protected override void OnPause()
	{
	}
}
