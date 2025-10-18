using UnityEngine;

namespace Duckov.Quests.Conditions;

public class RequireGameobjectsActived : Condition
{
	[SerializeField]
	private GameObject[] targets;

	public override bool Evaluate()
	{
		GameObject[] array = targets;
		foreach (GameObject gameObject in array)
		{
			if (gameObject == null || !gameObject.activeInHierarchy)
			{
				return false;
			}
		}
		return true;
	}
}
