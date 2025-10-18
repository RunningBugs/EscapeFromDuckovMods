using UnityEngine;

public class TimeOfDayAlertTriggerProxy : MonoBehaviour
{
	public void OnEnter()
	{
		TimeOfDayAlert.EnterAlertTrigger();
	}

	public void OnLeave()
	{
		TimeOfDayAlert.LeaveAlertTrigger();
	}
}
