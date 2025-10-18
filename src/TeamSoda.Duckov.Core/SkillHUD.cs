using System;
using UnityEngine;
using UnityEngine.UI;

public class SkillHUD : MonoBehaviour
{
	private CharacterMainControl characterMainControl;

	public CharacterTouchInputControl touchInputController;

	public Image skillIcon;

	private bool skillHudActive;

	public Soda_Joysticks skillJoystick;

	public GameObject skillButton;

	public GameObject activeParent;

	[SerializeField]
	private SkillTypes skillType;

	private CharacterSkillKeeper skillKeeper;

	private float rangeCache = -1f;

	private void Awake()
	{
		SyncHud();
	}

	private void SyncHud()
	{
		if (rangeCache < 0f)
		{
			rangeCache = skillJoystick.joystickRangePercent;
		}
		activeParent.SetActive(skillHudActive);
		if (skillHudActive)
		{
			skillIcon.sprite = skillKeeper.Skill.icon;
			if (skillKeeper.Skill.SkillContext.castRange > 0f)
			{
				skillJoystick.canCancle = true;
				skillJoystick.joystickRangePercent = rangeCache;
			}
			else
			{
				skillJoystick.canCancle = false;
				skillJoystick.joystickRangePercent = 0f;
			}
		}
	}

	private void Update()
	{
		if (!characterMainControl)
		{
			characterMainControl = LevelManager.Instance.MainCharacter;
			if (!characterMainControl)
			{
				return;
			}
			OnInit();
		}
		if (skillHudActive && (skillKeeper == null || !skillKeeper.CheckSkillAndBinding()))
		{
			skillHudActive = false;
			SyncHud();
		}
	}

	private void OnInit()
	{
		switch (skillType)
		{
		case SkillTypes.itemSkill:
			skillKeeper = characterMainControl.skillAction.holdItemSkillKeeper;
			skillJoystick.UpdateValueEvent.AddListener(touchInputController.SetItemSkillAimInput);
			skillJoystick.OnTouchEvent.AddListener(touchInputController.StartItemSkillAim);
			skillJoystick.OnUpEvent.AddListener(touchInputController.ItemSkillRelease);
			break;
		case SkillTypes.characterSkill:
			skillKeeper = characterMainControl.skillAction.characterSkillKeeper;
			skillJoystick.UpdateValueEvent.AddListener(touchInputController.SetCharacterSkillAimInput);
			skillJoystick.OnTouchEvent.AddListener(touchInputController.StartCharacterSkillAim);
			skillJoystick.OnUpEvent.AddListener(touchInputController.CharacterSkillRelease);
			break;
		}
		CharacterSkillKeeper characterSkillKeeper = skillKeeper;
		characterSkillKeeper.OnSkillChanged = (Action)Delegate.Combine(characterSkillKeeper.OnSkillChanged, new Action(OnSkillChanged));
		if (skillKeeper.CheckSkillAndBinding())
		{
			OnSkillChanged();
		}
	}

	private void OnSkillChanged()
	{
		skillHudActive = skillKeeper.CheckSkillAndBinding();
		if (skillJoystick.Holding)
		{
			skillJoystick.CancleTouch();
		}
		SyncHud();
	}

	private void OnDestroy()
	{
		if (skillKeeper != null)
		{
			CharacterSkillKeeper characterSkillKeeper = skillKeeper;
			characterSkillKeeper.OnSkillChanged = (Action)Delegate.Remove(characterSkillKeeper.OnSkillChanged, new Action(OnSkillChanged));
		}
	}
}
