using System;
using ItemStatsSystem;

public class ItemSetting_Skill : ItemSettingBase
{
	public enum OnReleaseAction
	{
		none,
		reduceCount
	}

	public OnReleaseAction onRelease;

	public SkillBase Skill;

	public override void OnInit()
	{
		if ((bool)Skill)
		{
			SkillBase skill = Skill;
			skill.OnSkillReleasedEvent = (Action)Delegate.Combine(skill.OnSkillReleasedEvent, new Action(OnSkillReleased));
			Skill.fromItem = base.Item;
		}
	}

	private void OnSkillReleased()
	{
		OnReleaseAction onReleaseAction = onRelease;
		if (onReleaseAction != OnReleaseAction.none && onReleaseAction == OnReleaseAction.reduceCount && (!LevelManager.Instance || !LevelManager.Instance.IsBaseLevel))
		{
			if (base.Item.Stackable)
			{
				base.Item.StackCount--;
				return;
			}
			base.Item.Detach();
			base.Item.DestroyTree();
		}
	}

	private void OnDestroy()
	{
		if ((bool)Skill)
		{
			SkillBase skill = Skill;
			skill.OnSkillReleasedEvent = (Action)Delegate.Remove(skill.OnSkillReleasedEvent, new Action(OnSkillReleased));
		}
	}

	public override void SetMarkerParam(Item selfItem)
	{
		selfItem.SetBool("IsSkill", value: true);
	}
}
