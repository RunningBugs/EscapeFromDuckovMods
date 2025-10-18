using NodeCanvas.Framework;
using SodaCraft.Localizations;

namespace NodeCanvas.Tasks.Actions;

public class PopText : ActionTask<AICharacterController>
{
	public BBParameter<string> content;

	public bool checkHide;

	private string Key => content.value;

	private string DisplayText => Key.ToPlainText();

	protected override string info => $"Pop:'{DisplayText}'";

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		if (checkHide && base.agent.CharacterMainControl.Hidden)
		{
			EndAction(success: true);
			return;
		}
		if (!base.agent.canTalk)
		{
			EndAction(success: true);
			return;
		}
		base.agent.CharacterMainControl.PopText(DisplayText);
		EndAction(success: true);
	}

	protected override void OnStop()
	{
	}

	protected override void OnPause()
	{
	}
}
