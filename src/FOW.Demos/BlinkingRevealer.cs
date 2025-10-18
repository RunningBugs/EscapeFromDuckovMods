using UnityEngine;

namespace FOW.Demos;

public class BlinkingRevealer : MonoBehaviour
{
	public float BlinkCycleTime = 5f;

	public bool RandomOffset = true;

	private void Awake()
	{
		if (RandomOffset)
		{
			BlinkCycleTime += Random.Range(0f, BlinkCycleTime * 0.5f);
		}
	}

	private void Update()
	{
		if (Time.time % BlinkCycleTime < BlinkCycleTime / 2f)
		{
			if (!base.transform.GetChild(0).gameObject.activeInHierarchy)
			{
				base.transform.GetChild(0).gameObject.SetActive(value: true);
			}
		}
		else if (base.transform.GetChild(0).gameObject.activeInHierarchy)
		{
			base.transform.GetChild(0).gameObject.SetActive(value: false);
		}
	}
}
