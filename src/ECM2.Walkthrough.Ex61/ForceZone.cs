using UnityEngine;

namespace ECM2.Walkthrough.Ex61;

public class ForceZone : MonoBehaviour
{
	public Vector3 windDirection = Vector3.up;

	public float windStrength = 20f;

	private void OnTriggerStay(Collider other)
	{
		if (other.CompareTag("Player") && other.TryGetComponent<Character>(out var component))
		{
			Vector3 vector = windDirection.normalized * windStrength;
			Vector3 rhs = -component.GetGravityDirection();
			float num = Vector3.Dot(vector, rhs);
			if (num > 0f && component.IsWalking() && num - component.GetGravityMagnitude() > 0f)
			{
				component.PauseGroundConstraint();
			}
			component.AddForce(vector, ForceMode.Acceleration);
		}
	}
}
