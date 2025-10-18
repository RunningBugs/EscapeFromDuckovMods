using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

public class CheckCanReleaseSkill : ConditionTask<AICharacterController>
{
	protected override bool OnCheck()
	{
		if (base.agent == null)
		{
			return false;
		}
		if (!base.agent.hasSkill)
		{
			return false;
		}
		if (!base.agent.skillInstance)
		{
			return false;
		}
		if (Time.time < base.agent.nextReleaseSkillTimeMarker)
		{
			return false;
		}
		if (!base.agent.CharacterMainControl.skillAction.IsSkillHasEnoughStaminaAndCD(base.agent.skillInstance))
		{
			return false;
		}
		if ((bool)base.agent.CharacterMainControl.CurrentAction && base.agent.CharacterMainControl.CurrentAction.Running)
		{
			return false;
		}
		base.agent.nextReleaseSkillTimeMarker = Time.time + Random.Range(base.agent.skillCoolTimeRange.x, base.agent.skillCoolTimeRange.y);
		if (Random.Range(0f, 1f) > base.agent.skillSuccessChance)
		{
			return false;
		}
		return true;
	}
}
