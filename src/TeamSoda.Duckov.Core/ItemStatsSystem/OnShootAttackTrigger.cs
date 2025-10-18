using UnityEngine;

namespace ItemStatsSystem;

[MenuPath("General/On Shoot&Attack")]
public class OnShootAttackTrigger : EffectTrigger
{
	[SerializeField]
	private bool onShoot = true;

	[SerializeField]
	private bool onAttack = true;

	private CharacterMainControl target;

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
		if (item == null)
		{
			return;
		}
		target = item.GetCharacterMainControl();
		if (!(target == null))
		{
			if (onShoot)
			{
				target.OnShootEvent += OnShootAttack;
			}
			if (onAttack)
			{
				target.OnAttackEvent += OnShootAttack;
			}
		}
	}

	private void UnregisterEvents()
	{
		if (!(target == null))
		{
			if (onShoot)
			{
				target.OnShootEvent -= OnShootAttack;
			}
			if (onAttack)
			{
				target.OnAttackEvent -= OnShootAttack;
			}
		}
	}

	private void OnShootAttack(DuckovItemAgent agent)
	{
		Trigger();
	}
}
