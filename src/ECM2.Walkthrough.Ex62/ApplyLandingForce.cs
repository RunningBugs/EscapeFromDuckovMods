using UnityEngine;

namespace ECM2.Walkthrough.Ex62;

public class ApplyLandingForce : MonoBehaviour
{
	public float landingForceScale = 1f;

	private Character _character;

	private void Awake()
	{
		_character = GetComponent<Character>();
	}

	private void OnEnable()
	{
		_character.Landed += OnLanded;
	}

	private void OnDisable()
	{
		_character.Landed -= OnLanded;
	}

	private void OnLanded(Vector3 landingVelocity)
	{
		Rigidbody groundRigidbody = _character.characterMovement.groundRigidbody;
		if ((bool)groundRigidbody)
		{
			Vector3 force = _character.GetGravityVector() * (_character.mass * landingVelocity.magnitude * landingForceScale);
			groundRigidbody.AddForceAtPosition(force, _character.position);
		}
	}
}
