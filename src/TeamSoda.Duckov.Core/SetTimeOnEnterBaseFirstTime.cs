using System;
using Saves;
using UnityEngine;

public class SetTimeOnEnterBaseFirstTime : MonoBehaviour
{
	public int setTimeTo;

	private void Start()
	{
		if (!SavesSystem.Load<bool>("FirstTimeToBaseTimeSetted"))
		{
			SavesSystem.Save("FirstTimeToBaseTimeSetted", value: true);
			TimeSpan time = new TimeSpan(setTimeTo, 0, 0);
			GameClock.Instance.StepTimeTil(time);
		}
	}
}
