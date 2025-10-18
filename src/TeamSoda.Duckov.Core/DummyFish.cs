using Duckov.Aquariums;
using UnityEngine;

public class DummyFish : MonoBehaviour, IAquariumContent
{
	[SerializeField]
	private Rigidbody rigidbody;

	[SerializeField]
	private float rotateForce = 10f;

	[SerializeField]
	private float swimForce = 10f;

	[SerializeField]
	private float deadZone = 2f;

	[SerializeField]
	private float rotationDamping = 0.1f;

	[Header("Control")]
	[SerializeField]
	private Transform target;

	[Range(0f, 1f)]
	[SerializeField]
	private float swim;

	private float rotVelocityX;

	private float rotVelocityY;

	private Aquarium master;

	private Vector3 _debug_idealRotForward;

	private Vector3 _debug_projectedForward;

	private Vector3 TargetPosition => target.position;

	private void Awake()
	{
		rigidbody.useGravity = false;
	}

	public void Setup(Aquarium master)
	{
		this.master = master;
	}

	private void FixedUpdate()
	{
		Vector3 up = Vector3.up;
		Vector3 forward = base.transform.forward;
		Vector3 right = base.transform.right;
		Vector3 vector = TargetPosition - rigidbody.position;
		Vector3 normalized = vector.normalized;
		Vector3 vector2 = Vector3.Cross(up, normalized);
		float b = Vector3.Dot(normalized, forward);
		float num = Mathf.Max(0f, b);
		swim = ((vector.magnitude > deadZone) ? 1f : (vector.magnitude / deadZone)) * num;
		Vector3 vector3 = -(Vector3.Dot(vector2, rigidbody.velocity) * vector2);
		rigidbody.velocity += forward * swimForce * swim * Time.deltaTime + vector3 * 0.5f;
		rigidbody.angularVelocity = Vector3.zero;
		Vector3 vector4 = vector;
		vector4.y = 0f;
		float num2 = Mathf.Clamp01(vector4.magnitude / deadZone - 0.5f);
		Vector3 to = (_debug_idealRotForward = Vector3.Lerp(_debug_projectedForward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized, normalized, num2));
		float num3 = Vector3.SignedAngle(forward, to, right);
		float num4 = Vector3.SignedAngle(forward, to, Vector3.up);
		float num5 = rotateForce * num3;
		float num6 = rotateForce * num4;
		rotVelocityX += num5 * Time.fixedDeltaTime;
		rotVelocityY += num6 * Time.fixedDeltaTime * num2;
		rotVelocityX *= 1f - rotationDamping;
		rotVelocityY *= 1f - rotationDamping;
		Vector3 eulerAngles = rigidbody.rotation.eulerAngles;
		eulerAngles.y += rotVelocityY * Time.deltaTime;
		eulerAngles.x += rotVelocityX * Time.deltaTime;
		if (eulerAngles.x < -179f)
		{
			eulerAngles.x += 360f;
		}
		if (eulerAngles.x > 179f)
		{
			eulerAngles.x -= 360f;
		}
		eulerAngles.x = Mathf.Clamp(eulerAngles.x, -45f, 45f);
		eulerAngles.z = 0f;
		Quaternion rot = Quaternion.Euler(eulerAngles);
		rigidbody.MoveRotation(rot);
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawLine(base.transform.position, base.transform.position + _debug_idealRotForward);
		Gizmos.color = Color.red;
		Gizmos.DrawLine(base.transform.position, base.transform.position + _debug_projectedForward);
	}
}
