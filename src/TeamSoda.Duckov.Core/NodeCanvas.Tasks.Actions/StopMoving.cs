using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Actions;

public class StopMoving : ActionTask<AICharacterController>
{
	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		base.agent.StopMove();
		EndAction(success: true);
	}
}
