using Duckov;
using Duckov.Quests;
using UnityEngine;

public class SetEndingMissleParameter : MonoBehaviour
{
	[SerializeField]
	private Condition launcherClosedCondition;

	private void Start()
	{
		bool flag = launcherClosedCondition.Evaluate();
		AudioManager.SetRTPC("Ending_Missile", flag ? 1 : 0);
	}
}
