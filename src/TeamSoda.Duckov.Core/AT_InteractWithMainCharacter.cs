using NodeCanvas.Framework;

public class AT_InteractWithMainCharacter : ActionTask
{
	public InteractableBase interactable;

	protected override void OnExecute()
	{
		base.OnExecute();
		interactable.InteractWithMainCharacter();
		EndAction();
	}
}
