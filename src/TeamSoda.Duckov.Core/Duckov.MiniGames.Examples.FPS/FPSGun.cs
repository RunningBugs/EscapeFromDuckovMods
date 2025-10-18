using System;
using DG.Tweening;
using UnityEngine;

namespace Duckov.MiniGames.Examples.FPS;

public class FPSGun : MiniGameBehaviour
{
	[Serializable]
	public struct Pose
	{
		[SerializeField]
		private Vector3 localPosition;

		[SerializeField]
		private Quaternion localRotation;

		public static Pose Extraterpolate(Pose poseA, Pose poseB, float t)
		{
			return new Pose
			{
				localPosition = Vector3.LerpUnclamped(poseA.localPosition, poseB.localPosition, t),
				localRotation = Quaternion.LerpUnclamped(poseA.localRotation, poseB.localRotation, t)
			};
		}

		public Pose(Transform fromTransform)
		{
			localPosition = fromTransform.localPosition;
			localRotation = fromTransform.localRotation;
		}
	}

	[SerializeField]
	private float fireRate = 1f;

	[SerializeField]
	private bool auto;

	[SerializeField]
	private Transform muzzle;

	[SerializeField]
	private ParticleSystem muzzleFlash;

	[SerializeField]
	private ParticleSystem bulletTracer;

	[SerializeField]
	private LayerMask castLayers = -1;

	[SerializeField]
	private ParticleSystem normalHitFXPrefab;

	[SerializeField]
	private float minScatterAngle;

	[SerializeField]
	private float maxScatterAngle;

	[SerializeField]
	private float scatterIncrementPerShot;

	[SerializeField]
	private float scatterDecayRate;

	[SerializeField]
	private Transform graphicsTransform;

	[SerializeField]
	private Pose idlePose;

	[SerializeField]
	private Pose recoilPose;

	private float scatterStatus;

	private float coolDown;

	private Camera mainCamera;

	private bool trigger;

	private bool justPressedTrigger;

	public float ScatterAngle => Mathf.Lerp(minScatterAngle, maxScatterAngle, scatterStatus);

	private void Fire()
	{
		coolDown = 1f / fireRate;
		DoCast();
		muzzleFlash.Play();
		DoFireAnimation();
		scatterStatus = Mathf.MoveTowards(scatterStatus, 1f, scatterIncrementPerShot);
	}

	private void DoFireAnimation()
	{
		graphicsTransform.DOKill(complete: true);
		graphicsTransform.localPosition = Vector3.zero;
		graphicsTransform.localRotation = Quaternion.identity;
		graphicsTransform.DOPunchPosition(Vector3.back * 0.2f, 0.2f);
		graphicsTransform.DOShakeRotation(0.5f, -Vector3.right * 10f);
	}

	private void DoCast()
	{
		Ray ray = mainCamera.ViewportPointToRay(Vector3.one * 0.5f);
		Vector2 vector = UnityEngine.Random.insideUnitCircle * ScatterAngle / 2f;
		Vector3 vector2 = Quaternion.Euler(vector.y, vector.x, 0f) * Vector3.forward;
		Vector3 direction = mainCamera.transform.localToWorldMatrix.MultiplyVector(vector2);
		ray.direction = direction;
		Physics.Raycast(ray, out var hitInfo, 100f, castLayers);
		HandleBulletTracer(hitInfo);
		if (!(hitInfo.collider == null))
		{
			FPSDamageInfo fPSDamageInfo = new FPSDamageInfo
			{
				source = this,
				amount = 1f,
				point = hitInfo.point,
				normal = hitInfo.normal
			};
			FPSDamageReceiver component = hitInfo.collider.GetComponent<FPSDamageReceiver>();
			if ((bool)component)
			{
				component.CastDamage(fPSDamageInfo);
			}
			else
			{
				HandleNormalHit(fPSDamageInfo);
			}
		}
	}

	private void HandleBulletTracer(RaycastHit castInfo)
	{
		if (bulletTracer == null)
		{
			return;
		}
		Vector3 position = muzzle.transform.position;
		Vector3 forward = muzzle.transform.forward;
		if (castInfo.collider != null)
		{
			forward = castInfo.point - position;
			if ((castInfo.point - position).magnitude < 5f)
			{
				bulletTracer.transform.rotation = Quaternion.FromToRotation(Vector3.forward, -forward);
				bulletTracer.transform.position = castInfo.point;
			}
			else
			{
				bulletTracer.transform.rotation = Quaternion.FromToRotation(Vector3.forward, forward);
				bulletTracer.transform.position = muzzle.position;
			}
		}
		else
		{
			bulletTracer.transform.rotation = Quaternion.FromToRotation(Vector3.forward, forward);
			bulletTracer.transform.position = muzzle.position;
		}
		bulletTracer.Emit(1);
	}

	private void HandleNormalHit(FPSDamageInfo info)
	{
		FXPool.Play(normalHitFXPrefab, info.point, Quaternion.FromToRotation(Vector3.forward, info.normal));
	}

	internal void SetTrigger(bool value)
	{
		trigger = value;
		if (value)
		{
			justPressedTrigger = true;
		}
	}

	internal void Setup(Camera mainCamera, Transform gunParent)
	{
		base.transform.SetParent(gunParent, worldPositionStays: false);
		base.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		this.mainCamera = mainCamera;
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (coolDown > 0f)
		{
			coolDown -= deltaTime;
			coolDown = Mathf.Max(0f, coolDown);
		}
		if (coolDown <= 0f && trigger && (auto || justPressedTrigger))
		{
			Fire();
		}
		justPressedTrigger = false;
		scatterStatus = Mathf.MoveTowards(scatterStatus, 0f, scatterDecayRate * deltaTime);
		UpdateGunPhysicsStatus(deltaTime);
	}

	private void UpdateGunPhysicsStatus(float deltaTime)
	{
	}
}
