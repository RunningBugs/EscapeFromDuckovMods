using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Conditions;

public class CheckHoldGun : ConditionTask<AICharacterController>
{
	protected override string OnInit()
	{
		return null;
	}

	protected override void OnEnable()
	{
	}

	protected override void OnDisable()
	{
	}

	protected override bool OnCheck()
	{
		return base.agent.CharacterMainControl.GetGun() != null;
	}
}
