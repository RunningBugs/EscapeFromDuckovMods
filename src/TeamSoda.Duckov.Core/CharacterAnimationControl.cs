using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationControl : MonoBehaviour
{
	public CharacterMainControl characterMainControl;

	public CharacterModel characterModel;

	public Animator animator;

	public float attackTime = 0.3f;

	private int attackLayer = -1;

	private bool attacking;

	private float attackTimer;

	private bool hasAnimationIfDashCanControl;

	public AnimationCurve attackLayerWeightCurve;

	private int hash_MoveSpeed = Animator.StringToHash("MoveSpeed");

	private int hash_MoveDirX = Animator.StringToHash("MoveDirX");

	private int hash_MoveDirY = Animator.StringToHash("MoveDirY");

	private int hash_RightHandOut = Animator.StringToHash("RightHandOut");

	private int hash_HandState = Animator.StringToHash("HandState");

	private int hash_Dashing = Animator.StringToHash("Dashing");

	private int hash_Attack = Animator.StringToHash("Attack");

	private HashSet<int> animatorHashes = new HashSet<int>();

	private float weight;

	private DuckovItemAgent holdAgent;

	private void InitHash()
	{
		AnimatorControllerParameter[] parameters = animator.parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			animatorHashes.Add(animatorControllerParameter.nameHash);
		}
	}

	private void SetAnimatorBool(int hash, bool value)
	{
		if (animatorHashes.Contains(hash))
		{
			animator.SetBool(hash, value);
		}
	}

	private void SetAnimatorFloat(int hash, float value)
	{
		if (animatorHashes.Contains(hash))
		{
			animator.SetFloat(hash, value);
		}
	}

	private void SetAnimatorInteger(int hash, int value)
	{
		if (animatorHashes.Contains(hash))
		{
			animator.SetInteger(hash, value);
		}
	}

	private void SetAnimatorTrigger(int hash)
	{
		if (animatorHashes.Contains(hash))
		{
			animator.SetTrigger(hash);
		}
	}

	private void Awake()
	{
		if (!characterModel)
		{
			characterModel = GetComponent<CharacterModel>();
		}
		characterModel.OnCharacterSetEvent += OnCharacterSet;
		if ((bool)characterModel.characterMainControl)
		{
			characterMainControl = characterModel.characterMainControl;
		}
		characterModel.OnAttackOrShootEvent += OnAttack;
		InitHash();
	}

	private void OnDestroy()
	{
		if ((bool)characterModel)
		{
			characterModel.OnCharacterSetEvent -= OnCharacterSet;
			characterModel.OnAttackOrShootEvent -= OnAttack;
		}
	}

	private void OnCharacterSet()
	{
		characterMainControl = characterModel.characterMainControl;
	}

	private void Start()
	{
		if (attackLayer < 0)
		{
			attackLayer = animator.GetLayerIndex("MeleeAttack");
		}
	}

	private void SetAttackLayerWeight(float weight)
	{
		if (attackLayer >= 0)
		{
			animator.SetLayerWeight(attackLayer, weight);
		}
	}

	private void Update()
	{
		if ((bool)characterMainControl)
		{
			SetAnimatorFloat(hash_MoveSpeed, characterMainControl.AnimationMoveSpeedValue);
			Vector2 animationLocalMoveDirectionValue = characterMainControl.AnimationLocalMoveDirectionValue;
			SetAnimatorFloat(hash_MoveDirX, animationLocalMoveDirectionValue.x);
			SetAnimatorFloat(hash_MoveDirY, animationLocalMoveDirectionValue.y);
			bool value = true;
			if (characterMainControl.CurrentHoldItemAgent == null)
			{
				value = false;
			}
			else if (!characterMainControl.CurrentHoldItemAgent.gameObject.activeSelf)
			{
				value = false;
			}
			else if (characterMainControl.reloadAction.Running)
			{
				value = false;
			}
			SetAnimatorBool(hash_RightHandOut, value);
			bool flag = characterMainControl.Dashing;
			if (flag && !hasAnimationIfDashCanControl && characterMainControl.DashCanControl)
			{
				flag = false;
			}
			SetAnimatorBool(hash_Dashing, flag);
			int value2 = 0;
			if (!holdAgent)
			{
				holdAgent = characterMainControl.CurrentHoldItemAgent;
			}
			if (holdAgent != null)
			{
				value2 = (int)holdAgent.handAnimationType;
			}
			SetAnimatorInteger(hash_HandState, value2);
			UpdateAttackLayerWeight();
		}
	}

	private void UpdateAttackLayerWeight()
	{
		if (!attacking)
		{
			if (weight > 0f)
			{
				weight = 0f;
				SetAttackLayerWeight(weight);
			}
			return;
		}
		attackTimer += Time.deltaTime;
		weight = attackLayerWeightCurve.Evaluate(attackTimer / attackTime);
		if (attackTimer >= attackTime)
		{
			attacking = false;
			weight = 0f;
		}
		SetAttackLayerWeight(weight);
	}

	public void OnAttack()
	{
		if (!characterMainControl || !holdAgent || holdAgent.handAnimationType != HandheldAnimationType.meleeWeapon)
		{
			attacking = false;
			return;
		}
		attacking = true;
		if (attackLayer < 0)
		{
			attackLayer = animator.GetLayerIndex("MeleeAttack");
		}
		SetAnimatorTrigger(hash_Attack);
		attackTimer = 0f;
	}
}
