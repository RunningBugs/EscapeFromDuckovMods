using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Conditions;

public class CheckReloading : ConditionTask<AICharacterController>
{
	protected override bool OnCheck()
	{
		if (base.agent == null)
		{
			return false;
		}
		if (base.agent.CharacterMainControl == null)
		{
			return false;
		}
		return base.agent.CharacterMainControl.reloadAction.Running;
	}
}
