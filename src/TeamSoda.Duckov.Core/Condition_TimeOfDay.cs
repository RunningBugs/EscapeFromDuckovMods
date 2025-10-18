using Duckov.Quests;
using UnityEngine;

public class Condition_TimeOfDay : Condition
{
	[Range(0f, 24f)]
	public float from;

	[Range(0f, 24f)]
	public float to;

	public override bool Evaluate()
	{
		float num = (float)GameClock.TimeOfDay.TotalHours % 24f;
		if (!(num >= from) || !(num <= to))
		{
			if (to < from)
			{
				if (!(num >= from))
				{
					return num <= to;
				}
				return true;
			}
			return false;
		}
		return true;
	}
}
