using UnityEngine;

public class SetActiveOnAwake : MonoBehaviour
{
	public GameObject target;

	private void Awake()
	{
		target.SetActive(value: true);
	}
}
