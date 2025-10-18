using System.Collections.Generic;
using UnityEngine;

public class ToggleHUD : MonoBehaviour
{
	public List<GameObject> toggleTargets;

	private void Awake()
	{
		foreach (GameObject toggleTarget in toggleTargets)
		{
			if (toggleTarget != null && !toggleTarget.activeInHierarchy)
			{
				toggleTarget.SetActive(value: true);
			}
		}
	}
}
