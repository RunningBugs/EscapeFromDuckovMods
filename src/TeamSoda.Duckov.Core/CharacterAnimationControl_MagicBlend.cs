using UnityEngine;

public class CharacterAnimationControl_MagicBlend : MonoBehaviour
{
	public CharacterMainControl characterMainControl;

	public CharacterModel characterModel;

	public Animator animator;

	public float attackTime = 0.3f;

	private int attackLayer = -1;

	private bool attacking;

	private float attackTimer;

	private DuckovItemAgent holdAgent;

	private ItemAgent_Gun gunAgent;

	public AnimationCurve attackLayerWeightCurve;

	private int hash_MoveSpeed = Animator.StringToHash("MoveSpeed");

	private int hash_MoveDirX = Animator.StringToHash("MoveDirX");

	private int hash_MoveDirY = Animator.StringToHash("MoveDirY");

	private int hash_Dashing = Animator.StringToHash("Dashing");

	private int hash_Attack = Animator.StringToHash("Attack");

	private int hash_HandState = Animator.StringToHash("HandState");

	private int hash_GunReady = Animator.StringToHash("GunReady");

	private float weight;

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
		animator.SetLayerWeight(attackLayer, 0f);
	}

	private void Update()
	{
		if (!characterMainControl)
		{
			return;
		}
		animator.SetFloat(hash_MoveSpeed, characterMainControl.AnimationMoveSpeedValue);
		Vector2 animationLocalMoveDirectionValue = characterMainControl.AnimationLocalMoveDirectionValue;
		animator.SetFloat(hash_MoveDirX, animationLocalMoveDirectionValue.x);
		animator.SetFloat(hash_MoveDirY, animationLocalMoveDirectionValue.y);
		int value = 0;
		if (!holdAgent || !holdAgent.isActiveAndEnabled)
		{
			holdAgent = characterMainControl.CurrentHoldItemAgent;
		}
		else
		{
			value = (int)holdAgent.handAnimationType;
		}
		if (characterMainControl.carryAction.Running)
		{
			value = -1;
		}
		animator.SetInteger(hash_HandState, value);
		if (holdAgent != null && gunAgent == null)
		{
			gunAgent = holdAgent as ItemAgent_Gun;
		}
		bool value2 = false;
		if (gunAgent != null)
		{
			value2 = true;
			if (gunAgent.IsReloading() || gunAgent.BulletCount <= 0)
			{
				value2 = false;
			}
		}
		animator.SetBool(hash_GunReady, value2);
		bool dashing = characterMainControl.Dashing;
		animator.SetBool(hash_Dashing, dashing);
		UpdateAttackLayerWeight();
	}

	private void UpdateAttackLayerWeight()
	{
		if (!attacking)
		{
			if (weight > 0f)
			{
				weight = 0f;
				animator.SetLayerWeight(attackLayer, weight);
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
		animator.SetLayerWeight(attackLayer, weight);
	}

	public void OnAttack()
	{
		attacking = true;
		if (attackLayer < 0)
		{
			attackLayer = animator.GetLayerIndex("MeleeAttack");
		}
		animator.SetTrigger(hash_Attack);
		attackTimer = 0f;
	}
}
