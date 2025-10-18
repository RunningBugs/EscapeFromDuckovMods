using Duckov.Buffs;
using UnityEngine;
using UnityEngine.Events;

public class DamageReceiver : MonoBehaviour
{
	public bool useSimpleHealth;

	public Health health;

	public HealthSimpleBase simpleHealth;

	public bool isHalfObsticle;

	public UnityEvent<DamageInfo> OnHurtEvent;

	public UnityEvent<DamageInfo> OnDeadEvent;

	public Teams Team
	{
		get
		{
			if (!useSimpleHealth && (bool)health)
			{
				return health.team;
			}
			if (useSimpleHealth && (bool)simpleHealth)
			{
				return simpleHealth.team;
			}
			return Teams.all;
		}
	}

	public bool IsMainCharacter
	{
		get
		{
			if (!useSimpleHealth && (bool)health)
			{
				return health.IsMainCharacterHealth;
			}
			return false;
		}
	}

	public bool IsDead
	{
		get
		{
			if (!health)
			{
				return false;
			}
			return health.IsDead;
		}
	}

	private void Start()
	{
		base.gameObject.layer = LayerMask.NameToLayer("DamageReceiver");
		if ((bool)health)
		{
			health.OnDeadEvent.AddListener(OnDead);
		}
	}

	private void OnDestroy()
	{
		if ((bool)health)
		{
			health.OnDeadEvent.RemoveListener(OnDead);
		}
	}

	public bool Hurt(DamageInfo damageInfo)
	{
		damageInfo.toDamageReceiver = this;
		OnHurtEvent?.Invoke(damageInfo);
		if ((bool)health)
		{
			health.Hurt(damageInfo);
		}
		return true;
	}

	public bool AddBuff(Buff buffPfb, CharacterMainControl fromWho)
	{
		if (useSimpleHealth)
		{
			return false;
		}
		if (!health)
		{
			return false;
		}
		CharacterMainControl characterMainControl = health.TryGetCharacter();
		if (!characterMainControl)
		{
			return false;
		}
		characterMainControl.AddBuff(buffPfb, fromWho);
		return true;
	}

	public void OnDead(DamageInfo dmgInfo)
	{
		base.gameObject.SetActive(value: false);
		OnDeadEvent?.Invoke(dmgInfo);
	}
}
