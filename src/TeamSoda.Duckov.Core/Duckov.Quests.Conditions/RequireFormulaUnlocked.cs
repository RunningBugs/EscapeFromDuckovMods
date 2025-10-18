using ItemStatsSystem;
using UnityEngine;

namespace Duckov.Quests.Conditions;

public class RequireFormulaUnlocked : Condition
{
	[ItemTypeID]
	[SerializeField]
	private int itemID;

	[SerializeField]
	private string formulaID;

	public Item setFromItem;

	public override bool Evaluate()
	{
		return CraftingManager.IsFormulaUnlocked(formulaID);
	}
}
