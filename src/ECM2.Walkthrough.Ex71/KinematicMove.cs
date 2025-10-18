using System;
using UnityEngine;

namespace ECM2.Walkthrough.Ex71;

[RequireComponent(typeof(Rigidbody))]
public class KinematicMove : MonoBehaviour
{
	[SerializeField]
	public float _moveTime = 3f;

	[SerializeField]
	private Vector3 _offset;

	private Rigidbody _rigidbody;

	private Vector3 _startPosition;

	private Vector3 _targetPosition;

	public float moveTime
	{
		get
		{
			return _moveTime;
		}
		set
		{
			_moveTime = Mathf.Max(0.0001f, value);
		}
	}

	public Vector3 offset
	{
		get
		{
			return _offset;
		}
		set
		{
			_offset = value;
		}
	}

	public static float EaseInOut(float time, float duration)
	{
		return -0.5f * (Mathf.Cos(MathF.PI * time / duration) - 1f);
	}

	public void OnValidate()
	{
		moveTime = _moveTime;
	}

	public void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_rigidbody.isKinematic = true;
		_startPosition = base.transform.position;
		_targetPosition = _startPosition + offset;
	}

	public void FixedUpdate()
	{
		float t = EaseInOut(Mathf.PingPong(Time.time, _moveTime), _moveTime);
		Vector3 position = Vector3.Lerp(_startPosition, _targetPosition, t);
		_rigidbody.MovePosition(position);
	}
}
