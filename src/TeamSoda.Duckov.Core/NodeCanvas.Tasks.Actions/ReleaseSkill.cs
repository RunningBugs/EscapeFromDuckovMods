using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

public class ReleaseSkill : ActionTask<AICharacterController>
{
	private float readyTime;

	private float tryReleaseSkillTimeMarker = -1f;

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		base.agent.CharacterMainControl.SetSkill(SkillTypes.characterSkill, base.agent.skillInstance, base.agent.skillInstance.gameObject);
		if (!base.agent.CharacterMainControl.StartSkillAim(SkillTypes.characterSkill))
		{
			EndAction(success: false);
		}
		else
		{
			readyTime = base.agent.skillInstance.SkillContext.skillReadyTime;
		}
	}

	protected override void OnUpdate()
	{
		if ((bool)base.agent.searchedEnemy)
		{
			base.agent.CharacterMainControl.SetAimPoint(base.agent.searchedEnemy.transform.position);
		}
		if (base.elapsedTime > readyTime + 0.1f)
		{
			if (Random.Range(0f, 1f) < base.agent.skillSuccessChance)
			{
				base.agent.CharacterMainControl.ReleaseSkill(SkillTypes.characterSkill);
				EndAction(success: true);
			}
			else
			{
				base.agent.CharacterMainControl.CancleSkill();
				EndAction(success: false);
			}
		}
	}

	protected override void OnStop()
	{
		base.agent.CharacterMainControl.CancleSkill();
		base.agent.CharacterMainControl.SwitchToFirstAvailableWeapon();
	}
}
