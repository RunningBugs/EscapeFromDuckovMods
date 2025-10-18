using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Actions;

public class AimToPlayer : ActionTask<AICharacterController>
{
	private CharacterMainControl target;

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
	}

	protected override void OnUpdate()
	{
		if (!target)
		{
			target = CharacterMainControl.Main;
		}
		base.agent.CharacterMainControl.SetAimPoint(target.transform.position);
	}
}
