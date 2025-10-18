using System.Linq;
using Sirenix.Utilities;

public class InteractCrafter : InteractableBase
{
	public string requireTag;

	protected override void Awake()
	{
		base.Awake();
		finishWhenTimeOut = true;
	}

	protected override void OnInteractFinished()
	{
		base.OnInteractFinished();
		CraftView.SetupAndOpenView(FilterCraft);
	}

	private bool FilterCraft(CraftingFormula formula)
	{
		if (requireTag.IsNullOrWhitespace())
		{
			return true;
		}
		if (formula.tags.Contains(requireTag))
		{
			return true;
		}
		return false;
	}
}
