using UnityEngine;

namespace Duckov.Quests.Conditions;

public class RequireDemo : Condition
{
	[SerializeField]
	private bool inverse;

	public override bool Evaluate()
	{
		if (inverse)
		{
			return !GameMetaData.Instance.IsDemo;
		}
		return GameMetaData.Instance.IsDemo;
	}
}
