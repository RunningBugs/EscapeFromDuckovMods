using UnityEngine;

namespace ECM2.Examples.SlopeSpeedModifier;

public class MyCharacter : Character
{
	public override float GetMaxSpeed()
	{
		float maxSpeed = base.GetMaxSpeed();
		float signedSlopeAngle = GetSignedSlopeAngle();
		float num = ((signedSlopeAngle > 0f) ? (1f - Mathf.InverseLerp(0f, 90f, signedSlopeAngle)) : (1f + Mathf.InverseLerp(0f, 90f, 0f - signedSlopeAngle)));
		return maxSpeed * num;
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(10f, 10f, 400f, 20f), $"Slope angle: {GetSignedSlopeAngle():F2} maxSpeed: {GetMaxSpeed():F2} ");
	}
}
