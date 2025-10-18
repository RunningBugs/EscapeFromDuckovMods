using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Actions;

public class FindMainCharacter : ActionTask<AICharacterController>
{
	public BBParameter<CharacterMainControl> mainCharacter;

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		if (!(LevelManager.Instance == null))
		{
			mainCharacter.value = LevelManager.Instance.MainCharacter;
			if (mainCharacter.value != null)
			{
				EndAction(success: true);
			}
		}
	}

	protected override void OnUpdate()
	{
		if (!(LevelManager.Instance == null))
		{
			mainCharacter.value = LevelManager.Instance.MainCharacter;
			if (mainCharacter.value != null)
			{
				EndAction(success: true);
			}
		}
	}

	protected override void OnStop()
	{
	}

	protected override void OnPause()
	{
	}
}
