using System;
using DG.Tweening;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class Hook : MiniGameBehaviour
{
	public enum HookStatus
	{
		Idle,
		Swinging,
		Launching,
		Attaching,
		Retrieving,
		Retrieved
	}

	public float emptySpeed = 1000f;

	public float strength;

	public float swingFreqFactor = 1f;

	[SerializeField]
	private Transform hookAxis;

	[SerializeField]
	private HookHead hookHead;

	[SerializeField]
	private Transform claw;

	[SerializeField]
	private float clawOffset = 4f;

	[SerializeField]
	private Animator clawAnimator;

	[SerializeField]
	private LineRenderer ropeLineRenderer;

	[SerializeField]
	private Bounds bounds;

	[SerializeField]
	private float grabAnimationTime = 0.5f;

	[SerializeField]
	private Ease grabAnimationEase = Ease.OutBounce;

	[SerializeField]
	private float maxAngle;

	[SerializeField]
	private float minDist;

	[SerializeField]
	private float maxDist;

	[Range(0f, 1f)]
	private float ropeControl;

	[Range(-1f, 1f)]
	private float axisControl;

	private HookStatus status;

	private float t;

	private GoldMinerEntity _grabbingTarget;

	private Vector2 relativePos;

	private Quaternion targetRelativeRotation;

	private float targetDist;

	private float retrieveETA;

	public float forceModification;

	private float maxDeltaWatch;

	public Transform Axis => hookAxis;

	public HookStatus Status => status;

	private float RopeDistance => Mathf.Lerp(minDist, maxDist, ropeControl);

	private float AxisAngle => Mathf.Lerp(0f - maxAngle, maxAngle, (axisControl + 1f) / 2f);

	private bool RopeOutOfBound
	{
		get
		{
			Vector3 point = Quaternion.Euler(0f, 0f, AxisAngle) * Vector2.down * RopeDistance;
			return !bounds.Contains(point);
		}
	}

	public GoldMinerEntity GrabbingTarget
	{
		get
		{
			return _grabbingTarget;
		}
		private set
		{
			_grabbingTarget = value;
		}
	}

	internal Vector3 Direction => -hookAxis.transform.up;

	public event Action<Hook, GoldMinerEntity> OnResolveTarget;

	public event Action<Hook> OnLaunch;

	public event Action<Hook> OnBeginRetrieve;

	public event Action<Hook, GoldMinerEntity> OnAttach;

	public event Action<Hook> OnEndRetrieve;

	public void SetParameters(float swingFreqFactor, float emptySpeed, float strength)
	{
		this.swingFreqFactor = swingFreqFactor;
		this.emptySpeed = emptySpeed;
		this.strength = strength;
	}

	public void Tick(float deltaTime)
	{
		UpdateStatus(deltaTime);
		UpdateHookHeadPosition();
		UpdateAxis();
		ropeLineRenderer.SetPositions(new Vector3[2]
		{
			hookAxis.transform.position,
			hookHead.transform.position
		});
	}

	private void UpdateHookHeadPosition()
	{
		hookHead.transform.localPosition = GetHookHeadPosition(RopeDistance);
	}

	private Vector3 GetHookHeadPosition(float ropeDistance)
	{
		return -Vector3.up * RopeDistance;
	}

	private void UpdateAxis()
	{
		hookAxis.transform.localRotation = Quaternion.Euler(0f, 0f, AxisAngle);
	}

	private void OnValidate()
	{
		UpdateHookHeadPosition();
		UpdateAxis();
	}

	private void UpdateStatus(float deltaTime)
	{
		switch (status)
		{
		case HookStatus.Swinging:
			UpdateSwinging(deltaTime);
			UpdateClaw();
			break;
		case HookStatus.Launching:
			UpdateClaw();
			UpdateLaunching(deltaTime);
			break;
		case HookStatus.Attaching:
			UpdateAttaching(deltaTime);
			break;
		case HookStatus.Retrieving:
			UpdateRetreving(deltaTime);
			UpdateClaw();
			break;
		case HookStatus.Retrieved:
			UpdateRetrieved();
			break;
		case HookStatus.Idle:
			break;
		}
	}

	public void Launch()
	{
		if (status == HookStatus.Swinging)
		{
			EnterStatus(HookStatus.Launching);
			this.OnLaunch?.Invoke(this);
		}
	}

	public void Reset()
	{
		ropeControl = 0f;
	}

	private void UpdateClaw()
	{
		clawAnimator.SetBool("Grabbing", GrabbingTarget);
		if (!GrabbingTarget)
		{
			claw.localRotation = Quaternion.Euler(0f, 0f, -180f);
			claw.localPosition = Vector3.zero;
		}
		else
		{
			Vector2 to = GrabbingTarget.transform.position - hookHead.transform.position;
			claw.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, to));
			claw.position = hookHead.transform.position + (Vector3)to.normalized * clawOffset;
		}
	}

	private void UpdateSwinging(float deltaTime)
	{
		t += deltaTime * 90f * swingFreqFactor * (MathF.PI / 180f);
		axisControl = Mathf.Sin(t);
	}

	private void UpdateLaunching(float deltaTime)
	{
		float speed = emptySpeed;
		if (GrabbingTarget != null)
		{
			speed = GrabbingTarget.Speed;
		}
		float num = (100f + strength) / 100f;
		speed *= num;
		float maxDelta = speed * deltaTime / (maxDist - minDist);
		Vector3 hookHeadPosition = GetHookHeadPosition(RopeDistance);
		ropeControl = Mathf.MoveTowards(ropeControl, 1f, maxDelta);
		GetHookHeadPosition(RopeDistance);
		Vector3 oldWorldPos = hookAxis.localToWorldMatrix.MultiplyPoint(hookHeadPosition);
		Vector3 newWorldPos = hookAxis.localToWorldMatrix.MultiplyPoint(hookHeadPosition);
		if (RopeOutOfBound || ropeControl >= 1f)
		{
			EnterStatus(HookStatus.Retrieving);
		}
		CheckGrab(oldWorldPos, newWorldPos);
	}

	private void CheckGrab(Vector3 oldWorldPos, Vector3 newWorldPos)
	{
		if ((bool)GrabbingTarget)
		{
			return;
		}
		Vector3 vector = newWorldPos - oldWorldPos;
		RaycastHit2D[] array = Physics2D.CircleCastAll(oldWorldPos, 8f, vector.normalized, vector.magnitude);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit2D raycastHit2D = array[i];
			if (!(raycastHit2D.collider == null))
			{
				GoldMinerEntity component = raycastHit2D.collider.gameObject.GetComponent<GoldMinerEntity>();
				if (!(component == null))
				{
					Grab(component);
					break;
				}
			}
		}
	}

	private void Grab(GoldMinerEntity target)
	{
		GrabbingTarget = target;
		EnterStatus(HookStatus.Attaching);
		relativePos = target.transform.position - hookHead.transform.position;
		targetDist = relativePos.magnitude;
		targetRelativeRotation = Quaternion.FromToRotation(relativePos, GrabbingTarget.transform.up);
		retrieveETA = grabAnimationTime;
		Vector2 to = GrabbingTarget.transform.position - hookHead.transform.position;
		Vector3 endValue = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.up, to));
		Vector3 endValue2 = hookHead.transform.position + (Vector3)to.normalized * clawOffset;
		claw.DORotate(endValue, retrieveETA).SetEase(grabAnimationEase);
		claw.DOMove(endValue2, retrieveETA).SetEase(grabAnimationEase);
		clawAnimator.SetBool("Grabbing", GrabbingTarget);
		GrabbingTarget.NotifyAttached(this);
		this.OnAttach?.Invoke(this, target);
	}

	private void UpdateAttaching(float deltaTime)
	{
		if (GrabbingTarget == null)
		{
			EnterStatus(HookStatus.Retrieving);
			return;
		}
		retrieveETA -= deltaTime;
		if (retrieveETA <= 0f)
		{
			EnterStatus(HookStatus.Retrieving);
		}
	}

	private void UpdateRetreving(float deltaTime)
	{
		float speed = emptySpeed;
		if (GrabbingTarget != null)
		{
			speed = GrabbingTarget.Speed;
		}
		float num = (100f + strength) / 100f;
		speed *= num;
		float maxDelta = (maxDeltaWatch = speed * deltaTime / (maxDist - minDist));
		Vector3 hookHeadPosition = GetHookHeadPosition(RopeDistance);
		ropeControl = Mathf.MoveTowards(ropeControl, 0f, maxDelta);
		GetHookHeadPosition(RopeDistance);
		Vector3 oldWorldPos = hookAxis.localToWorldMatrix.MultiplyPoint(hookHeadPosition);
		Vector3 newWorldPos = hookAxis.localToWorldMatrix.MultiplyPoint(hookHeadPosition);
		if (ropeControl <= 0f)
		{
			ropeControl = 0f;
			EnterStatus(HookStatus.Retrieved);
		}
		if ((bool)GrabbingTarget)
		{
			Vector3 vector = GrabbingTarget.transform.position - hookHead.transform.position;
			if (vector.magnitude > targetDist)
			{
				GrabbingTarget.transform.position = hookHead.transform.position + vector.normalized * targetDist;
				Vector3 toDirection = targetRelativeRotation * vector;
				GrabbingTarget.transform.rotation = Quaternion.FromToRotation(Vector3.up, toDirection);
			}
		}
		else
		{
			CheckGrab(oldWorldPos, newWorldPos);
		}
	}

	private void UpdateRetrieved()
	{
		if ((bool)GrabbingTarget)
		{
			ResolveRetrievedObject(GrabbingTarget);
			GrabbingTarget = null;
		}
		EnterStatus(HookStatus.Swinging);
	}

	private void ResolveRetrievedObject(GoldMinerEntity grabingTarget)
	{
		this.OnResolveTarget?.Invoke(this, grabingTarget);
	}

	private void OnExitStatus(HookStatus status)
	{
		switch (status)
		{
		case HookStatus.Retrieving:
			this.OnEndRetrieve?.Invoke(this);
			break;
		case HookStatus.Idle:
		case HookStatus.Swinging:
		case HookStatus.Launching:
		case HookStatus.Attaching:
		case HookStatus.Retrieved:
			break;
		}
	}

	private void EnterStatus(HookStatus status)
	{
		OnExitStatus(this.status);
		this.status = status;
		OnEnterStatus(this.status);
	}

	private void OnEnterStatus(HookStatus status)
	{
		switch (status)
		{
		case HookStatus.Swinging:
			ropeControl = 0f;
			break;
		case HookStatus.Retrieving:
			if ((bool)GrabbingTarget)
			{
				GrabbingTarget.NotifyBeginRetrieving();
			}
			this.OnBeginRetrieve?.Invoke(this);
			break;
		case HookStatus.Idle:
		case HookStatus.Launching:
		case HookStatus.Attaching:
		case HookStatus.Retrieved:
			break;
		}
	}

	internal void ReleaseClaw()
	{
		GrabbingTarget = null;
	}

	internal void BeginSwing()
	{
		EnterStatus(HookStatus.Swinging);
	}
}
