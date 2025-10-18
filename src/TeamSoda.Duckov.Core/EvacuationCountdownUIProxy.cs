using UnityEngine;

public class EvacuationCountdownUIProxy : MonoBehaviour
{
	public void Request(CountDownArea target)
	{
		EvacuationCountdownUI.Request(target);
	}

	public void Release(CountDownArea target)
	{
		EvacuationCountdownUI.Release(target);
	}
}
