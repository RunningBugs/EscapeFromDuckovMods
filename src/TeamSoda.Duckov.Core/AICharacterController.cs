using System.Collections.Generic;
using Duckov;
using Duckov.ItemUsage;
using Duckov.Scenes;
using ItemStatsSystem;
using ItemStatsSystem.Stats;
using NodeCanvas.BehaviourTrees;
using UnityEngine;

public class AICharacterController : MonoBehaviour
{
	public DamageReceiver searchedEnemy;

	public InteractablePickup searchedPickup;

	private DamageReceiver cachedSearchedEnemy;

	private CharacterMainControl characterMainControl;

	[SerializeField]
	private AI_PathControl pathControl;

	public CharacterSpawnerGroup group;

	public CharacterMainControl leader;

	public AICharacterController leaderAI;

	public bool shootCanMove;

	public bool defaultWeaponOut;

	private float updateValueTimer = 1f;

	public float patrolTurnSpeed = 200f;

	public float combatTurnSpeed = 1000f;

	private Stat rotateSpeedStat;

	public bool hasSkill;

	public SkillBase skillPfb;

	public SkillBase skillInstance;

	public Vector2 skillCoolTimeRange;

	[Range(0.01f, 1f)]
	public float skillSuccessChance = 1f;

	public float nextReleaseSkillTimeMarker = -1f;

	private float noticeTimeMarker;

	public bool noticed;

	public float sightDistance = 20f;

	public float forceTracePlayerDistance;

	public float sightAngle = 100f;

	public float baseReactionTime = 0.2f;

	public float nightReactionTimeFactor = 1.5f;

	public float reactionTime = 0.2f;

	public float shootDelay = 0.2f;

	public float scatterMultiIfTargetRunning = 4f;

	public float scatterMultiIfOffScreen = 4f;

	public Vector2 shootTimeRange = Vector2.one;

	public Vector2 shootTimeSpaceRange = Vector2.one;

	public Vector2 combatMoveTimeRange = new Vector2(1f, 3f);

	public bool canDash;

	public Vector2 dashCoolTimeRange;

	private Vector3 noticeFromPos;

	[ItemTypeID]
	public int wantItem;

	private Vector3 noticeFromDirection;

	private float combatWithTargetTimer;

	private bool weaponOut;

	private CharacterMainControl noticeFromCharacter;

	public float hearingAbility = 1f;

	public float traceTargetChance = 1f;

	public Transform aimTarget;

	private bool aimingRuningMainCharacter;

	public float patrolRange;

	public Vector3 patrolPosition;

	public float combatMoveRange;

	public float forgetTime = 8f;

	public bool canTalk = true;

	public bool alert;

	public BehaviourTree patrolTree;

	public BehaviourTree alertTree;

	public BehaviourTree combatTree;

	public BehaviourTree combat_Attack_Tree;

	[HideInInspector]
	public float hurtTimeMarker;

	[HideInInspector]
	public DamageInfo lastDamageInfo;

	public bool hasObsticleToTarget;

	public GameObject hideIfFoundEnemy;

	private Modifier scatterMultiplierModifier;

	private Stat scatterMultiplierStat;

	private float scatterModifierMultiplier = 1f;

	public float itemSkillChance = 0.3f;

	public float itemSkillCoolTime = 6f;

	private GameCamera gameCamera;

	public GameObject foundDangerObject;

	private List<ItemSetting_Skill> skillsOnItem = new List<ItemSetting_Skill>();

	private List<Item> drugItems = new List<Item>();

	public CharacterMainControl CharacterMainControl => characterMainControl;

	public Vector3 NoticeFromPos => noticeFromPos;

	public Vector3 NoticeFromDirection => noticeFromDirection;

	public CharacterMainControl NoticeFromCharacter
	{
		get
		{
			if (!noticed)
			{
				return null;
			}
			return noticeFromCharacter;
		}
	}

	public void Init(CharacterMainControl _characterMainControl, Vector3 patrolCenter, AudioManager.VoiceType voiceType = AudioManager.VoiceType.Duck, AudioManager.FootStepMaterialType footStepMatType = AudioManager.FootStepMaterialType.organic)
	{
		patrolPosition = patrolCenter;
		characterMainControl = _characterMainControl;
		pathControl.controller = characterMainControl;
		base.transform.SetParent(characterMainControl.transform, worldPositionStays: false);
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		characterMainControl.Health.OnHurtEvent.AddListener(OnHurt);
		if ((bool)_characterMainControl)
		{
			_characterMainControl.AudioVoiceType = voiceType;
			_characterMainControl.FootStepMaterialType = footStepMatType;
		}
		scatterMultiplierStat = _characterMainControl.CharacterItem.GetStat("GunScatterMultiplier");
		rotateSpeedStat = characterMainControl.CharacterItem.GetStat("TurnSpeed");
		scatterMultiplierModifier = new Modifier(ModifierType.PercentageMultiply, scatterModifierMultiplier, this);
		scatterMultiplierStat.AddModifier(scatterMultiplierModifier);
		if (hasSkill && !skillInstance && (bool)skillPfb)
		{
			skillInstance = Object.Instantiate(skillPfb, base.transform);
			skillInstance.transform.localPosition = Vector3.zero;
			characterMainControl.SetSkill(SkillTypes.characterSkill, skillInstance, skillInstance.gameObject);
		}
	}

	public float NightReactionTimeMultiplier()
	{
		return 1f;
	}

	public void AddItemSkill(ItemSetting_Skill skill)
	{
		skillsOnItem.Add(skill);
	}

	public ItemSetting_Skill GetItemSkill(bool random)
	{
		if (skillsOnItem.Count > 0 && random)
		{
			int index = Random.Range(0, skillsOnItem.Count);
			ItemSetting_Skill itemSetting_Skill = skillsOnItem[index];
			if ((bool)itemSetting_Skill)
			{
				return itemSetting_Skill;
			}
			skillsOnItem.RemoveAt(index);
		}
		if (skillsOnItem.Count > 0)
		{
			int num = 0;
			if (num < skillsOnItem.Count)
			{
				ItemSetting_Skill itemSetting_Skill2 = skillsOnItem[num];
				if (itemSetting_Skill2 == null || itemSetting_Skill2.Item == null)
				{
					skillsOnItem.RemoveAt(num);
					num--;
				}
				return itemSetting_Skill2;
			}
		}
		return null;
	}

	public void CheckAndAddDrugItem(Item targetItem)
	{
		if ((bool)targetItem.GetComponent<Drug>())
		{
			drugItems.Add(targetItem);
		}
	}

	public Item GetDrugItem()
	{
		if (drugItems.Count > 0)
		{
			int num = 0;
			if (num < drugItems.Count)
			{
				Item item = drugItems[num];
				if (item == null)
				{
					drugItems.RemoveAt(num);
					num--;
				}
				return item;
			}
		}
		return null;
	}

	private void Update()
	{
		if (!LevelManager.LevelInited)
		{
			return;
		}
		bool flag = searchedEnemy != null;
		if ((bool)aimTarget)
		{
			characterMainControl.SetAimPoint(aimTarget.transform.position + Vector3.up * 0.5f);
		}
		if (!gameCamera)
		{
			gameCamera = GameCamera.Instance;
		}
		updateValueTimer -= Time.deltaTime;
		if (updateValueTimer <= 0f)
		{
			updateValueTimer = 1f;
			bool isInDoor = MultiSceneCore.Instance.GetSubSceneInfo().IsInDoor;
			if (TimeOfDayController.Instance.AtNight && !isInDoor)
			{
				reactionTime = baseReactionTime * nightReactionTimeFactor;
			}
			else
			{
				reactionTime = baseReactionTime;
			}
			if (rotateSpeedStat != null)
			{
				if (flag)
				{
					rotateSpeedStat.BaseValue = combatTurnSpeed;
				}
				else
				{
					rotateSpeedStat.BaseValue = patrolTurnSpeed;
				}
			}
		}
		float num = 1f;
		if ((bool)aimTarget && (bool)CharacterMainControl.Main && aimTarget.gameObject == CharacterMainControl.Main.mainDamageReceiver.gameObject)
		{
			if (CharacterMainControl.Main.Running)
			{
				num = scatterMultiIfTargetRunning;
			}
			if ((bool)characterMainControl && (bool)gameCamera && gameCamera.IsOffScreen(characterMainControl.transform.position))
			{
				num = Mathf.Max(num, scatterMultiIfOffScreen);
			}
		}
		if (num != scatterModifierMultiplier)
		{
			scatterModifierMultiplier = num;
			scatterMultiplierModifier.Value = num;
		}
		if (group != null && group.hasLeader)
		{
			leaderAI = group.LeaderAI;
			if ((bool)leaderAI)
			{
				leader = leaderAI.characterMainControl;
			}
		}
		if (leader != null)
		{
			patrolPosition = leader.transform.position;
			Debug.DrawLine(base.transform.position, patrolPosition + Vector3.up * 2f, Color.magenta);
		}
		if (forceTracePlayerDistance > 0.5f && CharacterMainControl.Main != null && Vector3.Distance(base.transform.position, CharacterMainControl.Main.transform.position) < forceTracePlayerDistance)
		{
			searchedEnemy = CharacterMainControl.Main.mainDamageReceiver;
		}
		if ((bool)leaderAI)
		{
			if ((bool)leaderAI.searchedEnemy && !flag)
			{
				searchedEnemy = leaderAI.searchedEnemy;
				flag = true;
			}
			else if (!leaderAI.searchedEnemy && (bool)searchedEnemy)
			{
				leaderAI.searchedEnemy = searchedEnemy;
			}
		}
		if (flag && characterMainControl != null && searchedEnemy.Team == characterMainControl.Team)
		{
			searchedEnemy = null;
			flag = false;
		}
		if (searchedEnemy != cachedSearchedEnemy)
		{
			combatWithTargetTimer = 0f;
			cachedSearchedEnemy = searchedEnemy;
		}
		else
		{
			combatWithTargetTimer += Time.deltaTime;
		}
		if ((defaultWeaponOut || flag) && !weaponOut)
		{
			TakeOutWeapon();
		}
		if (hideIfFoundEnemy != null && !flag != hideIfFoundEnemy.activeSelf)
		{
			hideIfFoundEnemy.SetActive(!flag);
		}
	}

	public void SetNoticedToTarget(DamageReceiver target)
	{
		if ((bool)target)
		{
			noticeFromCharacter = target.health.TryGetCharacter();
			noticeTimeMarker = Time.time;
			noticeFromDirection = (target.transform.position - base.transform.position).normalized;
			noticeFromPos = target.transform.position;
		}
	}

	private void OnEnable()
	{
		AIMainBrain.OnSoundSpawned += OnSound;
	}

	private void OnDisable()
	{
		AIMainBrain.OnSoundSpawned -= OnSound;
	}

	private void OnDestroy()
	{
		AIMainBrain.OnSoundSpawned -= OnSound;
		if ((bool)characterMainControl)
		{
			characterMainControl.Health.OnHurtEvent.RemoveListener(OnHurt);
		}
		if (scatterMultiplierStat != null)
		{
			scatterMultiplierStat.RemoveAllModifiersFromSource(this);
		}
	}

	private void OnSound(AISound sound)
	{
		switch (sound.soundType)
		{
		case SoundTypes.unknowNoise:
			if (sound.fromTeam == characterMainControl.Team)
			{
				return;
			}
			break;
		case SoundTypes.combatSound:
			if (sound.fromTeam == characterMainControl.Team)
			{
				return;
			}
			break;
		case SoundTypes.grenadeDropSound:
			if ((bool)sound.fromObject)
			{
				foundDangerObject = sound.fromObject;
			}
			break;
		}
		Vector3 pos = sound.pos;
		pos.y = 0f;
		Vector3 position = base.transform.position;
		position.y = 0f;
		if (Vector3.Distance(position, pos) < sound.radius * hearingAbility)
		{
			noticed = true;
			noticeFromCharacter = sound.fromCharacter;
			noticeTimeMarker = Time.time;
			noticeFromDirection = (pos - position).normalized;
			noticeFromPos = sound.pos;
		}
	}

	private void OnHurt(DamageInfo dmgInfo)
	{
		if (!dmgInfo.isFromBuffOrEffect)
		{
			noticed = true;
			lastDamageInfo = dmgInfo;
			noticeFromCharacter = dmgInfo.fromCharacter;
			noticeTimeMarker = Time.time;
			hurtTimeMarker = Time.time;
			noticeFromDirection = dmgInfo.damageNormal;
			if ((bool)noticeFromCharacter)
			{
				noticeFromPos = noticeFromCharacter.transform.position;
			}
			else
			{
				noticeFromPos = base.transform.position + noticeFromDirection * 3f;
			}
		}
	}

	public bool IsHurt(float timeThreshold, int damageThreshold, ref DamageInfo dmgInfo)
	{
		dmgInfo = lastDamageInfo;
		if (Time.time - hurtTimeMarker < timeThreshold)
		{
			return lastDamageInfo.finalDamage >= (float)damageThreshold;
		}
		return false;
	}

	public bool isNoticing(float timeThreshold)
	{
		if (!noticed)
		{
			return false;
		}
		return Time.time - noticeTimeMarker < timeThreshold;
	}

	public void MoveToPos(Vector3 pos)
	{
		if ((bool)pathControl && (bool)pathControl.controller)
		{
			pathControl.MoveToPos(pos);
		}
	}

	public bool HasPath()
	{
		return pathControl.path != null;
	}

	public bool WaitingForPathResult()
	{
		return pathControl.WaitingForPathResult;
	}

	public void StopMove()
	{
		pathControl.StopMove();
	}

	public bool IsMoving()
	{
		return pathControl.Moving;
	}

	public bool ReachedEndOfPath()
	{
		return pathControl.ReachedEndOfPath;
	}

	public void SetTarget(Transform _aimTarget)
	{
		aimTarget = _aimTarget;
	}

	public void SetAimInput(Vector3 aimInput, AimTypes aimType)
	{
		characterMainControl.SetAimPoint(characterMainControl.transform.position + aimInput * 1000f);
		characterMainControl.SetAimType(aimType);
	}

	public void PutBackWeapon()
	{
		if (!(characterMainControl.CurrentHoldItemAgent == null))
		{
			characterMainControl.agentHolder.ChangeHoldItem(null);
			weaponOut = false;
		}
	}

	public void TakeOutWeapon()
	{
		bool flag = characterMainControl.SwitchToFirstAvailableWeapon();
		weaponOut = flag;
	}
}
