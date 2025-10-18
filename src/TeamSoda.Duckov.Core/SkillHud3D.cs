using UnityEngine;

public class SkillHud3D : MonoBehaviour
{
	private CharacterMainControl character;

	private bool aiming;

	public SkillRangeHUD skillRangeHUD;

	public SkillProjectileLineHUD projectileLine;

	private SkillBase currentSkill;

	private void Awake()
	{
		HideAll();
	}

	private void HideAll()
	{
		skillRangeHUD.gameObject.SetActive(value: false);
		projectileLine.gameObject.SetActive(value: false);
	}

	private void LateUpdate()
	{
		if (!character)
		{
			character = LevelManager.Instance.MainCharacter;
			return;
		}
		currentSkill = null;
		currentSkill = character.skillAction.CurrentRunningSkill;
		if (aiming != (currentSkill != null))
		{
			aiming = !aiming;
			if (currentSkill != null)
			{
				currentSkill = character.skillAction.CurrentRunningSkill;
				skillRangeHUD.gameObject.SetActive(value: true);
				float range = 1f;
				if (currentSkill.SkillContext.effectRange > 1f)
				{
					range = currentSkill.SkillContext.effectRange;
				}
				skillRangeHUD.SetRange(range);
				if (currentSkill.SkillContext.isGrenade)
				{
					projectileLine.gameObject.SetActive(value: true);
				}
			}
			else
			{
				HideAll();
			}
		}
		Vector3 currentSkillAimPoint = character.GetCurrentSkillAimPoint();
		bool flag = false;
		Vector3 hitPoint = Vector3.one;
		if (projectileLine.gameObject.activeSelf)
		{
			flag = projectileLine.UpdateLine(character.CurrentUsingAimSocket.position, currentSkillAimPoint, currentSkill.SkillContext.grenageVerticleSpeed, ref hitPoint);
		}
		skillRangeHUD.transform.position = currentSkillAimPoint;
		skillRangeHUD.SetProgress(character.skillAction.GetProgress().progress);
	}
}
