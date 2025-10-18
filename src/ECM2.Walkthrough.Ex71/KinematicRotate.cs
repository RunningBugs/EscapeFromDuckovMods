using UnityEngine;

namespace ECM2.Walkthrough.Ex71;

[RequireComponent(typeof(Rigidbody))]
public class KinematicRotate : MonoBehaviour
{
	[SerializeField]
	private float _rotationSpeed = 30f;

	public Vector3 rotationAxis = Vector3.up;

	private Rigidbody _rigidbody;

	private float _angle;

	public float rotationSpeed
	{
		get
		{
			return _rotationSpeed;
		}
		set
		{
			_rotationSpeed = value;
		}
	}

	public float angle
	{
		get
		{
			return _angle;
		}
		set
		{
			_angle = MathLib.ClampAngle(value, 0f, 360f);
		}
	}

	public void OnValidate()
	{
		rotationSpeed = _rotationSpeed;
		rotationAxis = rotationAxis.normalized;
	}

	public void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_rigidbody.isKinematic = true;
	}

	public void FixedUpdate()
	{
		angle += rotationSpeed * Time.deltaTime;
		Quaternion quaternion = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, rotationAxis.normalized);
		_rigidbody.MoveRotation(_rigidbody.rotation * quaternion);
	}
}
