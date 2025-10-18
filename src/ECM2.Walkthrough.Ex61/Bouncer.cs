using UnityEngine;

namespace ECM2.Walkthrough.Ex61;

public class Bouncer : MonoBehaviour
{
	public float launchImpulse = 15f;

	public bool overrideVerticalVelocity;

	public bool overrideLateralVelocity;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && other.TryGetComponent<Character>(out var component))
		{
			component.PauseGroundConstraint();
			component.LaunchCharacter(base.transform.up * launchImpulse, overrideVerticalVelocity, overrideLateralVelocity);
		}
	}
}
