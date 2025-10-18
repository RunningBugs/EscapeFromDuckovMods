using System.Collections.Generic;

namespace Duckov.Quests;

public static class ConditionExtensions
{
	public static bool Satisfied(this IEnumerable<Condition> conditions)
	{
		foreach (Condition condition in conditions)
		{
			if (!(condition == null) && !condition.Evaluate())
			{
				return false;
			}
		}
		return true;
	}
}
