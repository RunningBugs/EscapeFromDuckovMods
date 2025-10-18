using UnityEngine;

public class TimeScaleManager : MonoBehaviour
{
	private void Awake()
	{
	}

	private void Update()
	{
		float timeScale = 1f;
		if (GameManager.Paused)
		{
			timeScale = 0f;
		}
		if (CameraMode.Active)
		{
			timeScale = 0f;
		}
		Time.timeScale = timeScale;
		Time.fixedDeltaTime = Mathf.Max(0.0005f, Time.timeScale * 0.02f);
	}
}
