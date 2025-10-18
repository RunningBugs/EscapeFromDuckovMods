using Duckov.UI;

public class Workbench : InteractableBase
{
	protected override void OnInteractFinished()
	{
		ItemCustomizeSelectionView.Show();
	}
}
