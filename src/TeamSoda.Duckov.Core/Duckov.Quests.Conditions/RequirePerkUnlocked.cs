using Duckov.PerkTrees;
using UnityEngine;

namespace Duckov.Quests.Conditions;

public class RequirePerkUnlocked : Condition
{
	[SerializeField]
	private string perkTreeID;

	[SerializeField]
	private string perkObjectName;

	private Perk perk;

	public override bool Evaluate()
	{
		return GetUnlocked();
	}

	private bool GetUnlocked()
	{
		if ((bool)perk)
		{
			return perk.Unlocked;
		}
		PerkTree perkTree = PerkTreeManager.GetPerkTree(perkTreeID);
		if ((bool)perkTree)
		{
			foreach (Perk perk in perkTree.perks)
			{
				if (perk.gameObject.name == perkObjectName)
				{
					this.perk = perk;
					return this.perk.Unlocked;
				}
			}
		}
		return false;
	}
}
