using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HurtVisual : MonoBehaviour
{
	public bool useSimpleHealth;

	public HealthSimpleBase simpleHealth;

	private Health health;

	[SerializeField]
	private GameObject hitFX;

	[SerializeField]
	private GameObject hitFX_NoBlood;

	[SerializeField]
	private GameObject deadFx;

	[SerializeField]
	private GameObject deadFx_NoBlood;

	public List<Renderer> renderers;

	public static readonly int hurtHash = Shader.PropertyToID("_HurtValue");

	private MaterialPropertyBlock materialPropertyBlock;

	public float hurtCoolSpeed = 8f;

	public float hurtValueMultiplier = 1f;

	private float hurtValue;

	public GameObject HitFx
	{
		get
		{
			if (!GameManager.BloodFxOn && hitFX_NoBlood != null)
			{
				return hitFX_NoBlood;
			}
			return hitFX;
		}
	}

	public GameObject DeadFx
	{
		get
		{
			if (!GameManager.BloodFxOn && deadFx_NoBlood != null)
			{
				return deadFx_NoBlood;
			}
			return deadFx;
		}
	}

	public void SetHealth(Health _health)
	{
		if (!useSimpleHealth)
		{
			if (health != null)
			{
				health.OnHurtEvent.RemoveListener(OnHurt);
				health.OnDeadEvent.RemoveListener(OnDead);
			}
			health = _health;
			_health.OnHurtEvent.AddListener(OnHurt);
			_health.OnDeadEvent.AddListener(OnDead);
			Init();
		}
	}

	private void Awake()
	{
		if (useSimpleHealth && simpleHealth != null)
		{
			simpleHealth.OnHurtEvent += OnHurt;
			simpleHealth.OnDeadEvent += OnDead;
		}
	}

	private void Init()
	{
	}

	private void Update()
	{
		if (hurtValue > 0f)
		{
			SetRendererValue(hurtValue);
			hurtValue -= Time.unscaledDeltaTime * hurtCoolSpeed;
			if (hurtValue <= 0f)
			{
				SetRendererValue(0f);
			}
		}
	}

	private void OnHurt(DamageInfo dmgInfo)
	{
		bool flag = (bool)health && health.Hidden;
		if ((bool)HitFx && !flag)
		{
			PlayHurtEventProxy component = Object.Instantiate(HitFx, dmgInfo.damagePoint, Quaternion.LookRotation(dmgInfo.damageNormal)).GetComponent<PlayHurtEventProxy>();
			if ((bool)component)
			{
				component.Play(dmgInfo.crit > 0);
			}
		}
		hurtValue = 1f;
		SetRendererValue(hurtValue);
	}

	private void SetRendererValue(float value)
	{
		int count = renderers.Count;
		for (int i = 0; i < count; i++)
		{
			if (!(renderers[i] == null))
			{
				if (materialPropertyBlock == null)
				{
					materialPropertyBlock = new MaterialPropertyBlock();
				}
				renderers[i].GetPropertyBlock(materialPropertyBlock);
				materialPropertyBlock.SetFloat(hurtHash, value * hurtValueMultiplier);
				renderers[i].SetPropertyBlock(materialPropertyBlock);
			}
		}
	}

	private void OnDead(DamageInfo dmgInfo)
	{
		if ((bool)DeadFx)
		{
			PlayHurtEventProxy component = Object.Instantiate(DeadFx, base.transform.position, base.transform.rotation).GetComponent<PlayHurtEventProxy>();
			if ((bool)component)
			{
				component.Play(dmgInfo.crit > 0);
			}
		}
	}

	private void OnDestroy()
	{
		if ((bool)health)
		{
			health.OnHurtEvent.RemoveListener(OnHurt);
			health.OnDeadEvent.RemoveListener(OnDead);
		}
	}

	private void AutoSet()
	{
		renderers = GetComponentsInChildren<Renderer>(includeInactive: true).ToList();
		renderers.RemoveAll((Renderer e) => e == null || e.GetComponent<ParticleSystem>() != null);
	}

	public void SetRenderers(List<Renderer> _renderers)
	{
		renderers = _renderers;
	}
}
