using Duckov.Quests;
using UnityEngine;

public class RequireEnemyKilled : Condition
{
	[SerializeField]
	private CharacterRandomPreset enemyPreset;

	[SerializeField]
	private int threshold = 1;

	public override bool Evaluate()
	{
		if (enemyPreset == null)
		{
			return false;
		}
		return SavesCounter.GetKillCount(enemyPreset.nameKey) >= threshold;
	}
}
