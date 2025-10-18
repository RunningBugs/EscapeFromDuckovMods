using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

public class ReleaseItemSkillIfHas : ActionTask<AICharacterController>
{
	public bool random = true;

	private float checkTimeMarker = -1f;

	private float readyTime;

	private ItemSetting_Skill skillRefrence;

	private float chance
	{
		get
		{
			if (!base.agent)
			{
				return 0f;
			}
			return base.agent.itemSkillChance;
		}
	}

	public float checkTimeSpace
	{
		get
		{
			if (!base.agent)
			{
				return 999f;
			}
			return base.agent.itemSkillCoolTime;
		}
	}

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		skillRefrence = null;
		if (Time.time - checkTimeMarker < checkTimeSpace)
		{
			EndAction(success: false);
			return;
		}
		checkTimeMarker = Time.time;
		if (Random.Range(0f, 1f) > chance)
		{
			EndAction(success: false);
			return;
		}
		ItemSetting_Skill itemSkill = base.agent.GetItemSkill(random);
		if (!itemSkill)
		{
			EndAction(success: false);
			return;
		}
		if ((bool)base.agent.CharacterMainControl.CurrentAction && base.agent.CharacterMainControl.CurrentAction.Running)
		{
			EndAction(success: false);
			return;
		}
		skillRefrence = itemSkill;
		base.agent.CharacterMainControl.ChangeHoldItem(itemSkill.Item);
		base.agent.CharacterMainControl.SetSkill(SkillTypes.itemSkill, itemSkill.Skill, itemSkill.gameObject);
		if (!base.agent.CharacterMainControl.StartSkillAim(SkillTypes.itemSkill))
		{
			EndAction(success: false);
		}
		else
		{
			readyTime = itemSkill.Skill.SkillContext.skillReadyTime;
		}
	}

	protected override void OnUpdate()
	{
		if (!skillRefrence)
		{
			EndAction(success: false);
			return;
		}
		if ((bool)base.agent.searchedEnemy)
		{
			base.agent.CharacterMainControl.SetAimPoint(base.agent.searchedEnemy.transform.position);
		}
		if (base.elapsedTime > readyTime + 0.1f)
		{
			base.agent.CharacterMainControl.ReleaseSkill(SkillTypes.itemSkill);
			EndAction(success: true);
		}
	}

	protected override void OnStop()
	{
		base.agent.CharacterMainControl.CancleSkill();
		base.agent.CharacterMainControl.SwitchToFirstAvailableWeapon();
	}
}
