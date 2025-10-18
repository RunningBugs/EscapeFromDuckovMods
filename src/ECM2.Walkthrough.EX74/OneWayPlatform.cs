using UnityEngine;

namespace ECM2.Walkthrough.EX74;

public class OneWayPlatform : MonoBehaviour
{
	public Collider platformCollider;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			Character component = other.GetComponent<Character>();
			if ((bool)component)
			{
				component.IgnoreCollision(platformCollider);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			Character component = other.GetComponent<Character>();
			if ((bool)component)
			{
				component.IgnoreCollision(platformCollider, ignore: false);
			}
		}
	}
}
