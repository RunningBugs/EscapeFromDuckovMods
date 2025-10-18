using UnityEngine;

namespace Duckov.Quests.Conditions;

public class RequireQuestsActive : Condition
{
	[SerializeField]
	private int[] requiredQuestIDs;

	public int[] RequiredQuestIDs => requiredQuestIDs;

	public override bool Evaluate()
	{
		return QuestManager.AreQuestsActive(requiredQuestIDs);
	}
}
