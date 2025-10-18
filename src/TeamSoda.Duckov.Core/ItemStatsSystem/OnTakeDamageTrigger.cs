using UnityEngine;

namespace ItemStatsSystem;

[MenuPath("General/On Take Damage")]
public class OnTakeDamageTrigger : EffectTrigger
{
	[SerializeField]
	public int threshold;

	private Health target;

	private void OnEnable()
	{
		RegisterEvents();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		UnregisterEvents();
	}

	protected override void OnMasterSetTargetItem(Effect effect, Item item)
	{
		RegisterEvents();
	}

	private void RegisterEvents()
	{
		UnregisterEvents();
		if (base.Master == null)
		{
			return;
		}
		Item item = base.Master.Item;
		if (!(item == null))
		{
			CharacterMainControl characterMainControl = item.GetCharacterMainControl();
			if (!(characterMainControl == null))
			{
				target = characterMainControl.Health;
				target.OnHurtEvent.AddListener(OnTookDamage);
			}
		}
	}

	private void UnregisterEvents()
	{
		if (!(target == null))
		{
			target.OnHurtEvent.RemoveListener(OnTookDamage);
		}
	}

	private void OnTookDamage(DamageInfo info)
	{
		if (!(info.damageValue < (float)threshold))
		{
			Trigger();
		}
	}
}
