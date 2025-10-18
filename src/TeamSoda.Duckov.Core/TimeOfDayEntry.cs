using System.Collections.Generic;
using UnityEngine;

public class TimeOfDayEntry : MonoBehaviour
{
	[SerializeField]
	private List<TimeOfDayPhase> phases;

	private void Start()
	{
		if (phases.Count > 0)
		{
			TimeOfDayPhase value = phases[0];
			phases[0] = value;
		}
	}

	public TimeOfDayPhase GetPhase(TimePhaseTags timePhaseTags)
	{
		for (int i = 0; i < phases.Count; i++)
		{
			TimeOfDayPhase result = phases[i];
			if (result.timePhaseTag == timePhaseTags)
			{
				return result;
			}
		}
		if (timePhaseTags == TimePhaseTags.dawn)
		{
			return GetPhase(TimePhaseTags.day);
		}
		return phases[0];
	}
}
