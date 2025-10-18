using System;
using Duckov;
using UnityEngine;

public class CA_Attack : CharacterActionBase, IProgress
{
	private float attackActionTime = 0.25f;

	private ItemAgent_MeleeWeapon meleeWeapon;

	private float dealDamageTime = 0.1f;

	private bool damageDealed;

	private float lastAttackTime = -999f;

	private float cd = -1f;

	private bool slashFxSpawned;

	private float slashFxDelayTime;

	public bool DamageDealed => damageDealed;

	public event Action OnAttack;

	public override ActionPriorities ActionPriority()
	{
		return ActionPriorities.Attack;
	}

	public override bool CanMove()
	{
		return true;
	}

	public override bool CanRun()
	{
		return false;
	}

	public override bool CanUseHand()
	{
		return false;
	}

	public override bool CanControlAim()
	{
		return true;
	}

	public Progress GetProgress()
	{
		Progress result = default(Progress);
		if (base.Running)
		{
			result.inProgress = true;
			result.total = attackActionTime;
			result.current = actionTimer;
		}
		else
		{
			result.inProgress = false;
		}
		return result;
	}

	public override bool IsReady()
	{
		if (Time.time - lastAttackTime < cd)
		{
			return false;
		}
		meleeWeapon = characterController.GetMeleeWeapon();
		if (meleeWeapon == null)
		{
			return false;
		}
		if (meleeWeapon.StaminaCost > characterController.CurrentStamina)
		{
			return false;
		}
		return !base.Running;
	}

	protected override bool OnStart()
	{
		if (!characterController.CurrentHoldItemAgent)
		{
			return false;
		}
		meleeWeapon = characterController.GetMeleeWeapon();
		if (!meleeWeapon)
		{
			return false;
		}
		characterController.UseStamina(meleeWeapon.StaminaCost);
		dealDamageTime = meleeWeapon.DealDamageTime;
		damageDealed = false;
		this.OnAttack?.Invoke();
		CreateAttackSound();
		lastAttackTime = Time.time;
		cd = 1f / meleeWeapon.AttackSpeed;
		slashFxDelayTime = meleeWeapon.slashFxDelayTime;
		slashFxSpawned = false;
		return true;
	}

	private void CreateAttackSound()
	{
		AudioManager.Post("SFX/Combat/Melee/attack_" + meleeWeapon.SoundKey.ToLower(), base.gameObject);
	}

	protected override void OnStop()
	{
	}

	protected override void OnUpdateAction(float deltaTime)
	{
		if ((!(actionTimer > attackActionTime) && base.Running && !(meleeWeapon == null)) || !StopAction())
		{
			if (!slashFxSpawned && base.ActionTimer > slashFxDelayTime && (bool)meleeWeapon && (bool)meleeWeapon.slashFx)
			{
				slashFxSpawned = true;
				Vector3 position = characterController.transform.position;
				position.y = meleeWeapon.transform.position.y;
				UnityEngine.Object.Instantiate(meleeWeapon.slashFx, position, Quaternion.LookRotation(characterController.modelRoot.forward, Vector3.up)).transform.SetParent(base.transform);
			}
			if (!damageDealed && base.ActionTimer > dealDamageTime)
			{
				damageDealed = true;
				meleeWeapon.CheckAndDealDamage();
			}
		}
	}
}
