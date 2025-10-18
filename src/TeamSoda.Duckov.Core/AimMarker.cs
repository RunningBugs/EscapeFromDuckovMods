using System.Collections.Generic;
using LeTai.TrueShadow;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AimMarker : MonoBehaviour
{
	public RectTransform aimMarkerUI;

	public List<Image> aimMarkerImages;

	public RectTransform left;

	public RectTransform right;

	public RectTransform up;

	public RectTransform down;

	private float scatter;

	private float minScatter;

	public CanvasGroup rootCanvasGroup;

	public CanvasGroup normalAimCanvasGroup;

	public Animator aimMarkerAnimator;

	public ActionProgressHUD reloadProgressBar;

	public UnityEvent onShoot;

	private ADSAimMarker currentAdsAimMarker;

	[SerializeField]
	private ADSAimMarker defaultAdsAimMarker;

	private readonly int inProgressHash = Animator.StringToHash("InProgress");

	private readonly int killMarkerHash = Animator.StringToHash("KillMarkerShow");

	[SerializeField]
	private TextMeshProUGUI distanceText;

	[SerializeField]
	private TrueShadow distanceGlow;

	[SerializeField]
	private Color distanceTextColorFull;

	[SerializeField]
	private Color distanceTextColorHalf;

	[SerializeField]
	private Color distanceTextColorOver;

	private float adsValue;

	private float killMarkerTime = 0.6f;

	private float killMarkerTimer;

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

	private void Awake()
	{
		if (!currentAdsAimMarker)
		{
			SwitchAdsAimMarker(defaultAdsAimMarker);
		}
	}

	private void Start()
	{
		rootCanvasGroup.alpha = 1f;
		ItemAgent_Gun.OnMainCharacterShootEvent += OnMainCharacterShoot;
		Health.OnDead += OnKill;
	}

	private void OnDestroy()
	{
		ItemAgent_Gun.OnMainCharacterShootEvent -= OnMainCharacterShoot;
		Health.OnDead -= OnKill;
	}

	private void Update()
	{
		aimMarkerAnimator.SetBool(inProgressHash, reloadProgressBar.InProgress);
		if (killMarkerTimer > 0f)
		{
			killMarkerTimer -= Time.deltaTime;
			aimMarkerAnimator.SetBool(killMarkerHash, killMarkerTimer > 0f);
		}
		CharacterMainControl main = CharacterMainControl.Main;
		if (main == null)
		{
			return;
		}
		if (main.Health.IsDead)
		{
			rootCanvasGroup.alpha = 0f;
			return;
		}
		InputManager inputManager = LevelManager.Instance.InputManager;
		if (!(inputManager == null))
		{
			Vector3 inputAimPoint = inputManager.InputAimPoint;
			Vector3 vector = MainCam.WorldToScreenPoint(inputAimPoint);
			vector = inputManager.AimScreenPoint;
			SetAimMarkerPosScreenSpace(vector);
		}
	}

	private void LateUpdate()
	{
		CharacterMainControl main = CharacterMainControl.Main;
		if (main == null)
		{
			return;
		}
		InputManager inputManager = LevelManager.Instance.InputManager;
		if (inputManager == null)
		{
			return;
		}
		float num = 0f;
		Vector3 inputAimPoint = inputManager.InputAimPoint;
		ItemAgent_Gun gun = main.GetGun();
		Color color = distanceTextColorFull;
		if (gun != null)
		{
			if (adsValue == 0f && gun.AdsValue > 0f)
			{
				OnStartAdsWithGun(gun);
			}
			adsValue = gun.AdsValue;
			scatter = Mathf.MoveTowards(scatter, gun.CurrentScatter, 500f * Time.deltaTime);
			minScatter = Mathf.MoveTowards(minScatter, gun.MinScatter, 500f * Time.deltaTime);
			left.anchoredPosition = Vector3.left * (20f + scatter * 5f);
			right.anchoredPosition = Vector3.right * (20f + scatter * 5f);
			up.anchoredPosition = Vector3.up * (20f + scatter * 5f);
			down.anchoredPosition = Vector3.down * (20f + scatter * 5f);
			num = Vector3.Distance(inputAimPoint, gun.muzzle.position);
			float bulletDistance = gun.BulletDistance;
			color = ((num < bulletDistance * 0.495f) ? distanceTextColorFull : ((!(num < bulletDistance)) ? distanceTextColorOver : distanceTextColorHalf));
		}
		else
		{
			adsValue = 0f;
			scatter = 0f;
			minScatter = 0f;
			num = Vector3.Distance(inputAimPoint, main.transform.position + Vector3.up * 0.5f);
			color = distanceTextColorFull;
		}
		float alpha = Mathf.Clamp01((0.5f - adsValue) * 2f);
		if ((bool)currentAdsAimMarker)
		{
			currentAdsAimMarker.SetScatter(scatter, minScatter);
			currentAdsAimMarker.SetAdsValue(adsValue);
			if (!currentAdsAimMarker.hideNormalCrosshair)
			{
				alpha = 1f;
			}
		}
		else
		{
			alpha = 1f;
		}
		normalAimCanvasGroup.alpha = alpha;
		if ((bool)distanceText)
		{
			distanceText.text = num.ToString("00") + " M";
			distanceText.color = color;
			distanceGlow.Color = color;
		}
	}

	public void SetAimMarkerPosScreenSpace(Vector3 pos)
	{
		aimMarkerUI.position = pos;
		if ((bool)currentAdsAimMarker)
		{
			currentAdsAimMarker.SetAimMarkerPos(pos);
		}
	}

	private void OnStartAdsWithGun(ItemAgent_Gun gun)
	{
		ADSAimMarker aimMarkerPfb = gun.GetAimMarkerPfb();
		if ((bool)aimMarkerPfb)
		{
			SwitchAdsAimMarker(aimMarkerPfb);
		}
	}

	private void SwitchAdsAimMarker(ADSAimMarker newAimMarkerPfb)
	{
		if (newAimMarkerPfb == null)
		{
			Object.Destroy(currentAdsAimMarker.gameObject);
			currentAdsAimMarker = null;
		}
		else if (!currentAdsAimMarker || !(newAimMarkerPfb == currentAdsAimMarker.selfPrefab))
		{
			if ((bool)currentAdsAimMarker)
			{
				Object.Destroy(currentAdsAimMarker.gameObject);
			}
			currentAdsAimMarker = Object.Instantiate(newAimMarkerPfb);
			currentAdsAimMarker.selfPrefab = newAimMarkerPfb;
			currentAdsAimMarker.transform.SetParent(base.transform);
			currentAdsAimMarker.parentAimMarker = this;
			RectTransform obj = currentAdsAimMarker.transform as RectTransform;
			obj.anchorMin = Vector2.zero;
			obj.anchorMax = Vector2.one;
			obj.sizeDelta = Vector2.zero;
			obj.offsetMax = Vector2.zero;
			obj.offsetMin = Vector2.zero;
		}
	}

	private void SetAimMarkerColor(Color col)
	{
		int count = aimMarkerImages.Count;
		for (int i = 0; i < count; i++)
		{
			aimMarkerImages[i].color = col;
		}
	}

	private void OnKill(Health _health, DamageInfo dmgInfo)
	{
		if (!(_health == null) && _health.team != Teams.player)
		{
			killMarkerTimer = killMarkerTime;
		}
	}

	private void OnMainCharacterShoot(ItemAgent_Gun gunAgnet)
	{
		onShoot?.Invoke();
		if ((bool)currentAdsAimMarker)
		{
			currentAdsAimMarker.OnShoot();
		}
	}
}
