using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Actions;

public class SetNoticedToTarget : ActionTask<AICharacterController>
{
	public BBParameter<DamageReceiver> target;

	protected override string info => "set noticed to";

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		base.agent.SetNoticedToTarget(target.value);
		EndAction(success: true);
	}

	protected override void OnStop()
	{
	}

	protected override void OnPause()
	{
	}
}
