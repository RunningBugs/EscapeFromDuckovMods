using Duckov.Achievements;
using Duckov.Rules.UI;
using Saves;
using UnityEngine;

public class EndingControl : MonoBehaviour
{
	public int endingIndex;

	public string MissleLuncherClosedKey = "MissleLuncherClosed";

	public void SetEndingIndex()
	{
		Ending.endingIndex = endingIndex;
		AchievementManager instance = AchievementManager.Instance;
		bool flag = SavesSystem.Load<bool>(MissleLuncherClosedKey);
		DifficultySelection.UnlockRage();
		if (!instance)
		{
			return;
		}
		if (endingIndex == 0)
		{
			if (!flag)
			{
				instance.Unlock("Ending_0");
			}
			else
			{
				instance.Unlock("Ending_3");
			}
		}
		else if (!flag)
		{
			instance.Unlock("Ending_1");
		}
		else
		{
			instance.Unlock("Ending_2");
		}
	}
}
