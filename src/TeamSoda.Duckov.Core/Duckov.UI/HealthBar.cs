using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Duckov.UI.Animations;
using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Duckov.UI;

public class HealthBar : MonoBehaviour, IPoolable
{
	private RectTransform rectTransform;

	[SerializeField]
	private GameObject background;

	[SerializeField]
	private Image fill;

	[SerializeField]
	private Image followFill;

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private GameObject deathIndicator;

	[SerializeField]
	private PunchReceiver deathIndicatorPunchReceiver;

	[SerializeField]
	private Image hurtBlink;

	[SerializeField]
	private HealthBar_DamageBar damageBarTemplate;

	[SerializeField]
	private Gradient colorOverAmount = new Gradient();

	[SerializeField]
	private float followFillDuration = 0.5f;

	[SerializeField]
	private float blinkDuration = 0.1f;

	[SerializeField]
	private Color blinkColor = Color.white;

	private Vector3 displayOffset = Vector3.zero;

	[SerializeField]
	private float releaseAfterOutOfFrame = 1f;

	[SerializeField]
	private float disappearDelay = 0.2f;

	[SerializeField]
	private Image levelIcon;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private UnityEvent onHurt;

	[SerializeField]
	private UnityEvent onDead;

	private Action releaseAction;

	private float lastTimeInFrame = float.MinValue;

	private float screenYOffset = 0.02f;

	private PrefabPool<HealthBar_DamageBar> _damageBarPool;

	private bool pooled;

	private Vector3[] cornersBuffer = new Vector3[4];

	public Health target { get; private set; }

	private PrefabPool<HealthBar_DamageBar> DamageBarPool
	{
		get
		{
			if (_damageBarPool == null)
			{
				_damageBarPool = new PrefabPool<HealthBar_DamageBar>(damageBarTemplate);
			}
			return _damageBarPool;
		}
	}

	public void NotifyPooled()
	{
		pooled = true;
	}

	public void NotifyReleased()
	{
		UnregisterEvents();
		target = null;
		pooled = false;
	}

	private void Awake()
	{
		rectTransform = base.transform as RectTransform;
	}

	private void OnDestroy()
	{
		UnregisterEvents();
		followFill?.DOKill();
		hurtBlink?.DOKill();
	}

	private void LateUpdate()
	{
		if (target == null || !target.isActiveAndEnabled || target.Hidden)
		{
			Release();
		}
		else
		{
			UpdatePosition();
		}
	}

	private bool CheckInFrame()
	{
		rectTransform.GetWorldCorners(cornersBuffer);
		Vector3[] array = cornersBuffer;
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector = array[i];
			if (vector.x > 0f && vector.x < (float)Screen.width && vector.y > 0f && vector.y < (float)Screen.height)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateFrame()
	{
		if (CheckInFrame())
		{
			lastTimeInFrame = Time.unscaledTime;
		}
		if (Time.unscaledTime - lastTimeInFrame > releaseAfterOutOfFrame)
		{
			Release();
		}
	}

	private void UpdatePosition()
	{
		Vector3 position = target.transform.position + displayOffset;
		Vector3 position2 = Camera.main.WorldToScreenPoint(position);
		position2.y += screenYOffset * (float)Screen.height;
		base.transform.position = position2;
	}

	public void Setup(Health target, DamageInfo? damage = null, Action releaseAction = null)
	{
		this.releaseAction = releaseAction;
		UnregisterEvents();
		if (target == null)
		{
			Release();
			return;
		}
		if (target.IsDead)
		{
			Release();
			return;
		}
		background.SetActive(value: true);
		deathIndicator.SetActive(value: false);
		fill.gameObject.SetActive(value: true);
		followFill.gameObject.SetActive(value: true);
		this.target = target;
		RefreshOffset();
		RegisterEvents();
		Refresh();
		lastTimeInFrame = Time.unscaledTime;
		damageBarTemplate.gameObject.SetActive(value: false);
		if (damage.HasValue)
		{
			OnTargetHurt(damage.Value);
		}
		UpdatePosition();
	}

	public void RefreshOffset()
	{
		if (!target)
		{
			return;
		}
		displayOffset = Vector3.up * 1.5f;
		CharacterMainControl characterMainControl = target.TryGetCharacter();
		if ((bool)characterMainControl && (bool)characterMainControl.characterModel)
		{
			Transform helmatSocket = characterMainControl.characterModel.HelmatSocket;
			if ((bool)helmatSocket)
			{
				displayOffset = Vector3.up * (Vector3.Distance(characterMainControl.transform.position, helmatSocket.position) + 0.5f);
			}
		}
	}

	private void RegisterEvents()
	{
		if (!(target == null))
		{
			RefreshCharacterIcon();
			target.OnMaxHealthChange.AddListener(OnTargetMaxHealthChange);
			target.OnHealthChange.AddListener(OnTargetHealthChange);
			target.OnHurtEvent.AddListener(OnTargetHurt);
			target.OnDeadEvent.AddListener(OnTargetDead);
		}
	}

	private void RefreshCharacterIcon()
	{
		if (!target)
		{
			levelIcon.gameObject.SetActive(value: false);
			nameText.gameObject.SetActive(value: false);
			return;
		}
		CharacterMainControl characterMainControl = target.TryGetCharacter();
		if (!characterMainControl)
		{
			levelIcon.gameObject.SetActive(value: false);
			nameText.gameObject.SetActive(value: false);
			return;
		}
		CharacterRandomPreset characterPreset = characterMainControl.characterPreset;
		if (!characterPreset)
		{
			levelIcon.gameObject.SetActive(value: false);
			nameText.gameObject.SetActive(value: false);
			return;
		}
		Sprite characterIcon = characterPreset.GetCharacterIcon();
		if (!characterIcon)
		{
			levelIcon.gameObject.SetActive(value: false);
		}
		else
		{
			levelIcon.sprite = characterIcon;
			levelIcon.gameObject.SetActive(value: true);
		}
		if (!characterPreset.showName)
		{
			nameText.gameObject.SetActive(value: false);
			return;
		}
		nameText.text = characterPreset.DisplayName;
		nameText.gameObject.SetActive(value: true);
	}

	private void UnregisterEvents()
	{
		if (!(target == null))
		{
			target.OnMaxHealthChange.RemoveListener(OnTargetMaxHealthChange);
			target.OnHealthChange.RemoveListener(OnTargetHealthChange);
			target.OnHurtEvent.RemoveListener(OnTargetHurt);
			target.OnDeadEvent.RemoveListener(OnTargetDead);
		}
	}

	private void OnTargetMaxHealthChange(Health obj)
	{
		Refresh();
	}

	private void OnTargetHealthChange(Health obj)
	{
		Refresh();
	}

	private void OnTargetHurt(DamageInfo damage)
	{
		Color blinkEndColor = blinkColor;
		blinkEndColor.a = 0f;
		if (hurtBlink != null)
		{
			hurtBlink.DOColor(blinkColor, blinkDuration).From().OnKill(delegate
			{
				if (hurtBlink != null)
				{
					hurtBlink.color = blinkEndColor;
				}
			});
		}
		onHurt?.Invoke();
		ShowDamageBar(damage.finalDamage);
	}

	private void OnTargetDead(DamageInfo damage)
	{
		UnregisterEvents();
		onDead?.Invoke();
		if ((bool)damage.toDamageReceiver && (bool)damage.toDamageReceiver.health)
		{
			DeathTask(damage.toDamageReceiver.health).Forget();
		}
	}

	internal void Release()
	{
		if (pooled && (!(target != null) || !target.IsMainCharacterHealth || target.IsDead || !target.gameObject.activeInHierarchy))
		{
			UnregisterEvents();
			_ = target != null;
			target = null;
			releaseAction?.Invoke();
		}
	}

	private void Refresh()
	{
		float currentHealth = target.CurrentHealth;
		float maxHealth = target.MaxHealth;
		float num = 0f;
		if (maxHealth > 0f)
		{
			num = currentHealth / maxHealth;
		}
		fill.fillAmount = num;
		fill.color = colorOverAmount.Evaluate(num);
		if (followFill != null)
		{
			followFill.DOKill();
			followFill.DOFillAmount(num, followFillDuration);
		}
	}

	private void ShowDamageBar(float damageAmount)
	{
		float num = Mathf.Clamp01(damageAmount / target.MaxHealth);
		float num2 = Mathf.Clamp01(target.CurrentHealth / target.MaxHealth);
		float width = fill.rectTransform.rect.width;
		float damageBarWidth = width * num;
		float damageBarPostion = width * num2;
		HealthBar_DamageBar damageBar = DamageBarPool.Get();
		damageBar.Animate(damageBarPostion, damageBarWidth, delegate
		{
			DamageBarPool.Release(damageBar);
		}).Forget();
	}

	private async UniTask DeathTask(Health health)
	{
		background?.SetActive(value: false);
		deathIndicator?.SetActive(value: true);
		fill?.gameObject.SetActive(value: false);
		followFill?.gameObject.SetActive(value: false);
		deathIndicatorPunchReceiver?.Punch();
		await UniTask.WaitForSeconds(disappearDelay, ignoreTimeScale: true);
		if (health == target)
		{
			Release();
		}
	}
}
