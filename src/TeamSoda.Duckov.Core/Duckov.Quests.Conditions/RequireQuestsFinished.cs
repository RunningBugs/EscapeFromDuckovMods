using UnityEngine;

namespace Duckov.Quests.Conditions;

public class RequireQuestsFinished : Condition
{
	[SerializeField]
	private int[] requiredQuestIDs;

	public int[] RequiredQuestIDs => requiredQuestIDs;

	public override bool Evaluate()
	{
		return QuestManager.AreQuestFinished(requiredQuestIDs);
	}
}
