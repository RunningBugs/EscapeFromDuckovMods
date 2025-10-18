using UnityEngine;

public class LogOnEnableAndDisable : MonoBehaviour
{
	private void OnEnable()
	{
		Debug.Log("OnEnable");
	}

	private void OnDisable()
	{
		Debug.Log("OnDisable");
	}
}
