using UnityEngine;

namespace ECM2.Examples;

public sealed class SimpleCameraController : MonoBehaviour
{
	[SerializeField]
	private Transform _target;

	[SerializeField]
	private float _distanceToTarget = 10f;

	[SerializeField]
	private float _smoothTime = 0.1f;

	private Vector3 _followVelocity;

	public Transform target
	{
		get
		{
			return _target;
		}
		set
		{
			_target = value;
		}
	}

	public float distanceToTarget
	{
		get
		{
			return _distanceToTarget;
		}
		set
		{
			_distanceToTarget = Mathf.Max(0f, value);
		}
	}

	public void OnValidate()
	{
		distanceToTarget = _distanceToTarget;
	}

	public void Start()
	{
		if (!(_target == null))
		{
			base.transform.position = target.position - base.transform.forward * distanceToTarget;
		}
	}

	public void LateUpdate()
	{
		if (!(_target == null))
		{
			Vector3 vector = target.position - base.transform.forward * distanceToTarget;
			base.transform.position = Vector3.SmoothDamp(base.transform.position, vector, ref _followVelocity, _smoothTime);
		}
	}
}
