using UnityEngine;

namespace ECM2.Examples.Teleport;

public class Teleporter : MonoBehaviour
{
	[Tooltip("The destination teleporter.")]
	public Teleporter destination;

	[Tooltip("If true, the character will orient towards the destination Teleporter forward (yaw only)")]
	public bool OrientWithDestination;

	public bool isTeleporterEnabled { get; set; } = true;

	private void OnTriggerEnter(Collider other)
	{
		if (!(destination == null) && isTeleporterEnabled && other.TryGetComponent<Character>(out var component))
		{
			component.TeleportPosition(destination.transform.position);
			destination.isTeleporterEnabled = false;
			if (OrientWithDestination)
			{
				Vector3 upVector = component.GetUpVector();
				Vector3 vector = destination.transform.forward.projectedOnPlane(upVector);
				Quaternion newRotation = Quaternion.LookRotation(vector, component.GetUpVector());
				component.TeleportRotation(newRotation);
				component.LaunchCharacter(vector * component.GetSpeed(), overrideVerticalVelocity: false, overrideLateralVelocity: true);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		isTeleporterEnabled = true;
	}
}
