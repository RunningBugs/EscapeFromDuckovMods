using UnityEngine;

public class CharacterTouchInputControl : MonoBehaviour
{
	public InputManager characterInputManager;

	public void SetMoveInput(Vector2 axisInput, bool holding)
	{
		characterInputManager.SetMoveInput(axisInput);
	}

	public void SetRunInput(bool holding)
	{
		characterInputManager.SetRunInput(holding);
	}

	public void SetAdsInput(bool holding)
	{
		characterInputManager.SetAdsInput(holding);
	}

	public void SetGunAimInput(Vector2 axisInput, bool holding)
	{
		characterInputManager.SetAimInputUsingJoystick(axisInput);
		characterInputManager.SetAimType(AimTypes.normalAim);
	}

	public void SetCharacterSkillAimInput(Vector2 axisInput, bool holding)
	{
		characterInputManager.SetAimInputUsingJoystick(axisInput);
		characterInputManager.SetAimType(AimTypes.characterSkill);
	}

	public void StartCharacterSkillAim()
	{
		characterInputManager.StartCharacterSkillAim();
	}

	public void CharacterSkillRelease(bool trigger)
	{
		if (!trigger)
		{
			characterInputManager.CancleSkill();
		}
		else
		{
			characterInputManager.ReleaseCharacterSkill();
		}
	}

	public void SetItemSkillAimInput(Vector2 axisInput, bool holding)
	{
		characterInputManager.SetAimInputUsingJoystick(axisInput);
		characterInputManager.SetAimType(AimTypes.handheldSkill);
	}

	public void StartItemSkillAim()
	{
		characterInputManager.StartItemSkillAim();
	}

	public void ItemSkillRelease(bool trigger)
	{
		if (!trigger)
		{
			characterInputManager.CancleSkill();
		}
		else
		{
			characterInputManager.ReleaseItemSkill();
		}
	}
}
