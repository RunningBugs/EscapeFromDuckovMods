public class InteractableCarriable : InteractableBase
{
	public Carriable carryTarget;

	protected override void Start()
	{
		base.Start();
		finishWhenTimeOut = true;
	}

	protected override bool IsInteractable()
	{
		return true;
	}

	protected override void OnInteractStart(CharacterMainControl character)
	{
	}

	protected override void OnInteractFinished()
	{
		if ((bool)interactCharacter)
		{
			CharacterMainControl characterMainControl = interactCharacter;
			StopInteract();
			characterMainControl.Carry(carryTarget);
		}
	}
}
