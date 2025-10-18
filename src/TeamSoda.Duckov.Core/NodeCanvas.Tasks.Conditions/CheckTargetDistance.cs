using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

public class CheckTargetDistance : ConditionTask<AICharacterController>
{
	public bool useTransform;

	[ShowIf("useTransform", 1)]
	public BBParameter<Transform> targetTransform;

	[ShowIf("useTransform", 0)]
	public BBParameter<Vector3> targetPoint;

	public bool useShootRange;

	[ShowIf("useShootRange", 1)]
	public BBParameter<float> shootRangeMultiplier = 1f;

	[ShowIf("useShootRange", 0)]
	public BBParameter<float> distance;

	protected override string info => "is target in range";

	protected override bool OnCheck()
	{
		if (useTransform && targetTransform.value == null)
		{
			return false;
		}
		Vector3 b = (useTransform ? targetTransform.value.position : targetPoint.value);
		float num = 0f;
		num = ((!useShootRange) ? distance.value : (base.agent.CharacterMainControl.GetAimRange() * shootRangeMultiplier.value));
		return Vector3.Distance(base.agent.transform.position, b) <= num;
	}
}
