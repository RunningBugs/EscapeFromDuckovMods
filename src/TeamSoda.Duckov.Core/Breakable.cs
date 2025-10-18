using Saves;
using UnityEngine;

public class Breakable : MonoBehaviour
{
	private enum BreakableStates
	{
		normal,
		danger,
		breaked
	}

	public bool save;

	public string saveKey;

	public HealthSimpleBase simpleHealth;

	public int dangerHealth = 50;

	public bool createExplosion;

	public float explosionRadius;

	public DamageInfo explosionDamageInfo;

	private BreakableStates breakableState;

	public GameObject normalVisual;

	public GameObject dangerVisual;

	public GameObject breakedVisual;

	public GameObject mainCollider;

	public GameObject dangerFx;

	public string SaveKey => "Breakable_" + saveKey;

	private void Awake()
	{
		normalVisual.SetActive(value: true);
		if ((bool)dangerVisual)
		{
			dangerVisual.SetActive(value: false);
		}
		if ((bool)breakedVisual)
		{
			breakedVisual.SetActive(value: false);
		}
		simpleHealth.OnHurtEvent += OnHurt;
		simpleHealth.OnDeadEvent += OnDead;
		bool flag = false;
		if (save)
		{
			flag = SavesSystem.Load<bool>(SaveKey);
		}
		if (flag)
		{
			breakableState = BreakableStates.danger;
			normalVisual.SetActive(value: false);
			if ((bool)dangerVisual)
			{
				dangerVisual.SetActive(value: false);
			}
			if ((bool)breakedVisual)
			{
				breakedVisual.SetActive(value: true);
			}
			if ((bool)simpleHealth && (bool)simpleHealth.dmgReceiver)
			{
				simpleHealth.dmgReceiver.gameObject.SetActive(value: false);
			}
		}
		else if ((bool)mainCollider)
		{
			mainCollider.SetActive(value: true);
		}
	}

	private void OnValidate()
	{
	}

	private void OnHurt(DamageInfo dmgInfo)
	{
		switch (breakableState)
		{
		case BreakableStates.normal:
			if (simpleHealth.HealthValue <= (float)dangerHealth)
			{
				breakableState = BreakableStates.danger;
				if ((bool)dangerVisual)
				{
					normalVisual.SetActive(value: false);
					dangerVisual.SetActive(value: true);
				}
				if ((bool)dangerFx)
				{
					Object.Instantiate(dangerFx, base.transform.position, base.transform.rotation);
				}
			}
			break;
		case BreakableStates.danger:
		case BreakableStates.breaked:
			break;
		}
	}

	private void OnDead(DamageInfo dmgInfo)
	{
		explosionDamageInfo.fromCharacter = dmgInfo.fromCharacter;
		normalVisual.SetActive(value: false);
		if ((bool)dangerVisual)
		{
			dangerVisual.SetActive(value: false);
		}
		if ((bool)breakedVisual)
		{
			breakedVisual.SetActive(value: true);
		}
		if ((bool)mainCollider)
		{
			mainCollider.SetActive(value: false);
		}
		breakableState = BreakableStates.breaked;
		if (createExplosion)
		{
			LevelManager.Instance.ExplosionManager.CreateExplosion(base.transform.position, explosionRadius, explosionDamageInfo);
		}
		if (save)
		{
			SavesSystem.Save("Breakable_", saveKey, value: true);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (createExplosion)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(base.transform.position, explosionRadius);
		}
	}
}
