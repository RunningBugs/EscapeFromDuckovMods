using Duckov.Achievements;
using UnityEngine;

namespace Duckov.PerkTrees.Behaviours;

public class UnlockAchievement : PerkBehaviour
{
	[SerializeField]
	private string achievementKey;

	protected override void OnUnlocked()
	{
		if (!(AchievementManager.Instance == null))
		{
			AchievementManager.Instance.Unlock(achievementKey.Trim());
		}
	}
}
