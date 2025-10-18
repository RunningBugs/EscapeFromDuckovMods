using System;
using UnityEngine;

[Serializable]
public class CharacterSkillKeeper
{
	private SkillBase skill;

	private GameObject skillBindingObject;

	public Action OnSkillChanged;

	public SkillBase Skill => skill;

	public void SetSkill(SkillBase _skill, GameObject _bindingObject)
	{
		skill = null;
		skillBindingObject = null;
		if (_skill != null && _bindingObject != null)
		{
			skill = _skill;
			skillBindingObject = _bindingObject;
		}
		OnSkillChanged?.Invoke();
	}

	public bool CheckSkillAndBinding()
	{
		if (skill != null && skillBindingObject != null)
		{
			return true;
		}
		skill = null;
		skillBindingObject = null;
		return false;
	}
}
