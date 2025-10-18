using Duckov.UI;

namespace Duckov.PerkTrees.Interactable;

public class PerkTreeUIInvoker : InteractableBase
{
	public string perkTreeID;

	protected override bool ShowUnityEvents => false;

	protected override void OnInteractStart(CharacterMainControl interactCharacter)
	{
		PerkTreeView.Show(PerkTreeManager.GetPerkTree(perkTreeID));
		StopInteract();
	}
}
