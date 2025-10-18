using System;
using System.Collections.Generic;
using Duckov;
using UnityEngine;
using UnityEngine.Events;

public class HitMarker : MonoBehaviour
{
	public UnityEvent hitEvent;

	public UnityEvent killEvent;

	public Animator animator;

	private readonly int hitHash1 = Animator.StringToHash("HitMarkerHit1");

	private readonly int hitHash2 = Animator.StringToHash("HitMarkerHit2");

	private readonly int critHash1 = Animator.StringToHash("HitMarkerCrit1");

	private readonly int critHash2 = Animator.StringToHash("HitMarkerCrit2");

	private bool hitMarkerIndex;

	private readonly int killHash = Animator.StringToHash("HitMarkerKill");

	private readonly int killCritHash = Animator.StringToHash("HitMarkerKillCrit");

	public List<RectTransform> hitMarkerImages;

	private float scatterOnHit;

	private Camera _cam;

	private Camera MainCam
	{
		get
		{
			if (!_cam)
			{
				if (LevelManager.Instance == null)
				{
					return null;
				}
				if (LevelManager.Instance.GameCamera == null)
				{
					return null;
				}
				_cam = LevelManager.Instance.GameCamera.renderCamera;
			}
			return _cam;
		}
	}

	public static event Action OnHitMarker;

	public static event Action OnKillMarker;

	private void Awake()
	{
		Health.OnHurt += OnHealthHitEvent;
		Health.OnDead += OnHealthKillEvent;
		HealthSimpleBase.OnSimpleHealthHit += OnSimpleHealthHit;
		HealthSimpleBase.OnSimpleHealthDead += OnSimpleHealthKill;
	}

	private void OnDestroy()
	{
		Health.OnHurt -= OnHealthHitEvent;
		Health.OnDead -= OnHealthKillEvent;
		HealthSimpleBase.OnSimpleHealthHit -= OnSimpleHealthHit;
		HealthSimpleBase.OnSimpleHealthDead -= OnSimpleHealthKill;
	}

	private void OnHealthHitEvent(Health _health, DamageInfo dmgInfo)
	{
		if (!dmgInfo.isFromBuffOrEffect && !(dmgInfo.damageValue <= 1.01f))
		{
			OnHit(dmgInfo);
		}
	}

	private void OnHit(DamageInfo dmgInfo)
	{
		if ((bool)dmgInfo.fromCharacter && dmgInfo.fromCharacter.IsMainCharacter && (!dmgInfo.toDamageReceiver || !dmgInfo.toDamageReceiver.IsMainCharacter))
		{
			bool flag = (float)dmgInfo.crit > 0f;
			Vector3 vector = MainCam.WorldToScreenPoint(dmgInfo.damagePoint);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(base.transform.parent as RectTransform, vector, null, out var localPoint);
			base.transform.localPosition = Vector3.ClampMagnitude(localPoint, 10f);
			ItemAgent_Gun gun = CharacterMainControl.Main.GetGun();
			if (gun != null)
			{
				scatterOnHit = gun.CurrentScatter;
			}
			int stateHashName = ((!flag) ? (hitMarkerIndex ? hitHash1 : hitHash2) : (hitMarkerIndex ? critHash1 : critHash2));
			int shortNameHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
			if (shortNameHash != killHash && shortNameHash != killCritHash)
			{
				hitMarkerIndex = !hitMarkerIndex;
				animator.CrossFade(stateHashName, 0.02f);
			}
			HitMarker.OnHitMarker?.Invoke();
			if (!dmgInfo.toDamageReceiver || !dmgInfo.toDamageReceiver.useSimpleHealth)
			{
				AudioManager.PostHitMarker(flag);
			}
			hitEvent?.Invoke();
		}
	}

	private void OnHealthKillEvent(Health _health, DamageInfo dmgInfo)
	{
		OnKill(dmgInfo);
	}

	private void OnKill(DamageInfo dmgInfo)
	{
		if ((bool)dmgInfo.fromCharacter && dmgInfo.fromCharacter.IsMainCharacter && (!dmgInfo.toDamageReceiver || !dmgInfo.toDamageReceiver.IsMainCharacter))
		{
			bool flag = (float)dmgInfo.crit > 0f;
			int stateHashName = (flag ? killCritHash : killHash);
			animator.CrossFade(stateHashName, 0.02f);
			if (!dmgInfo.toDamageReceiver || !dmgInfo.toDamageReceiver.useSimpleHealth)
			{
				AudioManager.PostKillMarker(flag);
			}
			HitMarker.OnKillMarker?.Invoke();
			killEvent?.Invoke();
		}
	}

	private void OnSimpleHealthHit(HealthSimpleBase health, DamageInfo dmgInfo)
	{
		if (!(dmgInfo.damageValue <= 1.01f))
		{
			OnHit(dmgInfo);
		}
	}

	private void OnSimpleHealthKill(HealthSimpleBase health, DamageInfo dmgInfo)
	{
		OnKill(dmgInfo);
	}

	private void LateUpdate()
	{
		foreach (RectTransform hitMarkerImage in hitMarkerImages)
		{
			hitMarkerImage.anchoredPosition += hitMarkerImage.anchoredPosition.normalized * scatterOnHit * 3f;
		}
	}
}
