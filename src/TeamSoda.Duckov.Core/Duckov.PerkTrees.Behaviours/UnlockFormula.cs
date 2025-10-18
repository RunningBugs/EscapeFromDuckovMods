using System.Collections.Generic;

namespace Duckov.PerkTrees.Behaviours;

public class UnlockFormula : PerkBehaviour
{
	private IEnumerable<string> FormulasToUnlock
	{
		get
		{
			if (!CraftingFormulaCollection.Instance)
			{
				yield break;
			}
			string matchKey = base.Master.Master.ID + "/" + base.Master.name;
			foreach (CraftingFormula entry in CraftingFormulaCollection.Instance.Entries)
			{
				if (entry.requirePerk == matchKey)
				{
					yield return entry.id;
				}
			}
		}
	}

	protected override void OnUnlocked()
	{
		foreach (string item in FormulasToUnlock)
		{
			CraftingManager.UnlockFormula(item);
		}
	}
}
