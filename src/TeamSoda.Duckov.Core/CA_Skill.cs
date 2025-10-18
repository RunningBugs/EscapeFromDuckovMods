using Duckov;
using UnityEngine;

public class CA_Skill : CharacterActionBase, IProgress
{
	[SerializeField]
	public CharacterSkillKeeper holdItemSkillKeeper;

	[SerializeField]
	public CharacterSkillKeeper characterSkillKeeper;

	private SkillTypes skillTypeToRelease;

	private CharacterSkillKeeper currentRunningSkillKeeper;

	public SkillBase CurrentRunningSkill
	{
		get
		{
			if (!base.Running || currentRunningSkillKeeper == null)
			{
				return null;
			}
			return currentRunningSkillKeeper.Skill;
		}
	}

	public CharacterSkillKeeper GetSkillKeeper(SkillTypes skillType)
	{
		return skillType switch
		{
			SkillTypes.itemSkill => holdItemSkillKeeper, 
			SkillTypes.characterSkill => characterSkillKeeper, 
			_ => null, 
		};
	}

	public override ActionPriorities ActionPriority()
	{
		return ActionPriorities.Skills;
	}

	public override bool CanControlAim()
	{
		return true;
	}

	public override bool CanEditInventory()
	{
		return false;
	}

	public override bool CanMove()
	{
		if (CurrentRunningSkill != null)
		{
			return CurrentRunningSkill.SkillContext.movableWhileAim;
		}
		return true;
	}

	public override bool CanRun()
	{
		return false;
	}

	public override bool CanUseHand()
	{
		return false;
	}

	public override bool IsReady()
	{
		if (base.Running)
		{
			return false;
		}
		return true;
	}

	public bool IsSkillHasEnoughStaminaAndCD(SkillBase skill)
	{
		if (characterController.CurrentStamina < skill.staminaCost)
		{
			return false;
		}
		if (Time.time - skill.LastReleaseTime < skill.coolDownTime)
		{
			return false;
		}
		return true;
	}

	protected override bool OnStart()
	{
		CharacterSkillKeeper skillKeeper = GetSkillKeeper(skillTypeToRelease);
		if (skillKeeper != null && skillKeeper.CheckSkillAndBinding())
		{
			if (skillKeeper.Skill != null)
			{
				if (!IsSkillHasEnoughStaminaAndCD(skillKeeper.Skill))
				{
					return false;
				}
				_ = skillKeeper.Skill.SkillContext;
			}
			currentRunningSkillKeeper = skillKeeper;
			Debug.Log($"skillType is {skillTypeToRelease}");
			return true;
		}
		return false;
	}

	public void SetNextSkillType(SkillTypes skillType)
	{
		if (!base.Running)
		{
			skillTypeToRelease = skillType;
		}
	}

	public bool SetSkillOfType(SkillTypes skillType, SkillBase _skill, GameObject _bindingObject)
	{
		CharacterSkillKeeper skillKeeper = GetSkillKeeper(skillType);
		if (skillKeeper == null)
		{
			return false;
		}
		if (base.Running && skillKeeper == currentRunningSkillKeeper)
		{
			StopAction();
		}
		skillKeeper.SetSkill(_skill, _bindingObject);
		return true;
	}

	public bool ReleaseSkill(SkillTypes skillType)
	{
		if (!base.Running)
		{
			return false;
		}
		if (CurrentRunningSkill == null)
		{
			StopAction();
			return false;
		}
		if (skillType != skillTypeToRelease)
		{
			StopAction();
			return false;
		}
		if (!IsSkillHasEnoughStaminaAndCD(CurrentRunningSkill))
		{
			return false;
		}
		if (actionTimer < CurrentRunningSkill.SkillContext.skillReadyTime)
		{
			StopAction();
			return false;
		}
		Vector3 currentSkillAimPoint = characterController.GetCurrentSkillAimPoint();
		SkillReleaseContext releaseContext = new SkillReleaseContext
		{
			releasePoint = currentSkillAimPoint
		};
		CurrentRunningSkill.ReleaseSkill(releaseContext, characterController);
		currentRunningSkillKeeper = null;
		StopAction();
		return true;
	}

	protected override void OnStop()
	{
		currentRunningSkillKeeper = null;
	}

	protected override void OnUpdateAction(float deltaTime)
	{
		if (currentRunningSkillKeeper == null || !currentRunningSkillKeeper.CheckSkillAndBinding())
		{
			StopAction();
		}
	}

	public Progress GetProgress()
	{
		Progress result = default(Progress);
		SkillBase currentRunningSkill = CurrentRunningSkill;
		if (currentRunningSkill != null)
		{
			result.total = currentRunningSkill.SkillContext.skillReadyTime;
			result.current = actionTimer;
			result.inProgress = result.progress < 1f;
		}
		else
		{
			result.inProgress = false;
		}
		return result;
	}
}
