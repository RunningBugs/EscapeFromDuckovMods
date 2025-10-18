using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Actions;

public class Attack : ActionTask<AICharacterController>
{
	protected override string info => $"Attack";

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		base.agent.CharacterMainControl.Attack();
		EndAction(success: true);
	}

	protected override void OnStop()
	{
	}

	protected override void OnPause()
	{
	}
}
