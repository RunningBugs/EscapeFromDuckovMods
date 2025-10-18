using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Actions;

public class SetRun : ActionTask<AICharacterController>
{
	public BBParameter<bool> run;

	protected override string info => $"SetRun:{run.value}";

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		base.agent.CharacterMainControl.SetRunInput(run.value);
		EndAction(success: true);
	}

	protected override void OnStop()
	{
	}

	protected override void OnPause()
	{
	}
}
