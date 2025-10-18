using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Utilities;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

public class Projectile : MonoBehaviour
{
	public ProjectileContext context;

	public float radius;

	private float traveledDistance;

	private List<RaycastHit> hits;

	private LayerMask hitLayers;

	private Vector3 hitPoint;

	private Vector3 hitNormal;

	private bool dead;

	private bool overMaxDistance;

	[SerializeField]
	private GameObject hitFx;

	private Vector3 direction;

	private Vector3 velocity;

	private float gravity;

	[HideInInspector]
	public List<GameObject> damagedObjects;

	public static Action<Vector3> OnBulletFlyByCharacter;

	private bool flyThroughCharacterSoundPlayed;

	private bool firstFrame = true;

	private Vector3 startPoint;

	private ObjectPool<Projectile> pool;

	[SerializeField]
	private TrailRenderer trail;

	[FormerlySerializedAs("spin")]
	public Transform randomRotate;

	private bool inited;

	private DamageReceiver _dmgReceiverTemp;

	private float _distanceThisFrame;

	private int _hitCount;

	public void SetPool(ObjectPool<Projectile> _pool)
	{
		pool = _pool;
	}

	private void Release()
	{
		if (pool == null)
		{
			Debug.Log("Destroy");
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			pool.Release(this);
		}
	}

	private void Awake()
	{
		if (!inited)
		{
			inited = true;
			Init();
		}
	}

	public void Init()
	{
		inited = true;
		damagedObjects = new List<GameObject>();
		damagedObjects.Clear();
		traveledDistance = 0f;
		dead = false;
		overMaxDistance = false;
		flyThroughCharacterSoundPlayed = false;
		firstFrame = true;
		hitLayers = (int)GameplayDataSettings.Layers.damageReceiverLayerMask | (int)GameplayDataSettings.Layers.wallLayerMask | (int)GameplayDataSettings.Layers.groundLayerMask;
		if ((bool)trail)
		{
			trail.Clear();
		}
		if ((bool)randomRotate)
		{
			randomRotate.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));
		}
	}

	private void Update()
	{
		if (dead)
		{
			Release();
			return;
		}
		UpdateMoveAndCheck();
		if (dead)
		{
			if (firstFrame && (bool)trail)
			{
				trail.Clear();
			}
			if (context.explosionRange > 0f)
			{
				DamageInfo dmgInfo = new DamageInfo(context.fromCharacter);
				dmgInfo.damageValue = context.explosionDamage;
				dmgInfo.fromWeaponItemID = context.fromWeaponItemID;
				dmgInfo.armorPiercing = context.armorPiercing;
				LevelManager.Instance.ExplosionManager.CreateExplosion(base.transform.position, context.explosionRange, dmgInfo);
			}
			Release();
		}
		UpdateFlyThroughSound();
		firstFrame = false;
	}

	private void UpdateFlyThroughSound()
	{
		if (dead || context.team == Teams.player || flyThroughCharacterSoundPlayed || CharacterMainControl.Main == null || velocity.magnitude < 9f)
		{
			return;
		}
		Vector3 lhs = CharacterMainControl.Main.transform.position - base.transform.position;
		lhs.y = 0f;
		if (!(lhs.magnitude > 5f))
		{
			lhs.Normalize();
			if (!(Vector3.Dot(lhs, velocity) > 0f))
			{
				flyThroughCharacterSoundPlayed = true;
				OnBulletFlyByCharacter?.Invoke(base.transform.position);
			}
		}
	}

	public void Init(ProjectileContext _context)
	{
		Init();
		context = _context;
		direction = context.direction;
		velocity = context.speed * direction;
		gravity = Mathf.Abs(context.gravity);
		UpdateAimDirection();
	}

	private void UpdateMoveAndCheck()
	{
		if (firstFrame)
		{
			startPoint = base.transform.position;
		}
		float num = Time.deltaTime;
		if (num > 0.04f)
		{
			num = 0.04f;
		}
		velocity.y -= num * gravity;
		direction = velocity.normalized;
		UpdateAimDirection();
		_distanceThisFrame = velocity.magnitude * num;
		if (_distanceThisFrame + traveledDistance > context.distance)
		{
			_distanceThisFrame = context.distance - traveledDistance;
			overMaxDistance = true;
		}
		Vector3 origin = base.transform.position - base.transform.forward * 0.1f;
		if (firstFrame && context.firstFrameCheck)
		{
			origin = context.firstFrameCheckStartPoint;
		}
		hits = Physics.SphereCastAll(origin, radius, direction, _distanceThisFrame + 0.3f, hitLayers, QueryTriggerInteraction.Ignore).ToList();
		int count = hits.Count;
		if (count > 0)
		{
			hits.Sort((RaycastHit a, RaycastHit b) => (a.distance > b.distance) ? 1 : 0);
			for (int num2 = 0; num2 < count; num2++)
			{
				RaycastHit raycastHit = hits[num2];
				hitPoint = raycastHit.point;
				if (raycastHit.distance <= 0f)
				{
					hitPoint = raycastHit.collider.transform.position;
				}
				if (damagedObjects.Contains(hits[num2].collider.gameObject) || (context.ignoreHalfObsticle && GameplayDataSettings.LayersData.IsLayerInLayerMask(hits[num2].collider.gameObject.layer, GameplayDataSettings.Layers.halfObsticleLayer)))
				{
					continue;
				}
				damagedObjects.Add(hits[num2].collider.gameObject);
				if (((int)GameplayDataSettings.Layers.damageReceiverLayerMask & (1 << hits[num2].collider.gameObject.layer)) != 0)
				{
					_dmgReceiverTemp = hits[num2].collider.GetComponent<DamageReceiver>();
					if (_dmgReceiverTemp.Team == context.team || (_dmgReceiverTemp.isHalfObsticle && context.ignoreHalfObsticle))
					{
						continue;
					}
				}
				else
				{
					_dmgReceiverTemp = null;
				}
				if ((bool)_dmgReceiverTemp)
				{
					bool flag = true;
					if (_dmgReceiverTemp.Team == context.team)
					{
						flag = false;
					}
					else if ((bool)_dmgReceiverTemp.health)
					{
						CharacterMainControl characterMainControl = _dmgReceiverTemp.health.TryGetCharacter();
						if ((bool)characterMainControl && _dmgReceiverTemp.health.TryGetCharacter().Dashing)
						{
							flag = false;
						}
						else if ((bool)characterMainControl && characterMainControl == context.fromCharacter)
						{
							flag = false;
						}
					}
					if (flag)
					{
						DamageInfo damageInfo = new DamageInfo(context.fromCharacter);
						damageInfo.damageValue = context.damage;
						if (context.halfDamageDistance > 0f && Vector3.Distance(startPoint, hitPoint) > context.halfDamageDistance)
						{
							damageInfo.damageValue *= 0.5f;
						}
						damageInfo.critDamageFactor = context.critDamageFactor;
						damageInfo.critRate = context.critRate;
						damageInfo.armorPiercing = context.armorPiercing;
						damageInfo.armorBreak = context.armorBreak;
						damageInfo.elementFactors.Add(new ElementFactor(ElementTypes.physics, context.element_Physics));
						damageInfo.elementFactors.Add(new ElementFactor(ElementTypes.fire, context.element_Fire));
						damageInfo.elementFactors.Add(new ElementFactor(ElementTypes.poison, context.element_Poison));
						damageInfo.elementFactors.Add(new ElementFactor(ElementTypes.electricity, context.element_Electricity));
						damageInfo.elementFactors.Add(new ElementFactor(ElementTypes.space, context.element_Space));
						damageInfo.damagePoint = hitPoint;
						damageInfo.buffChance = context.buffChance;
						damageInfo.buff = context.buff;
						damageInfo.bleedChance = context.bleedChance;
						damageInfo.damageType = DamageTypes.normal;
						damageInfo.fromWeaponItemID = context.fromWeaponItemID;
						damageInfo.damageNormal = raycastHit.normal.normalized;
						_dmgReceiverTemp.Hurt(damageInfo);
						_dmgReceiverTemp.AddBuff(GameplayDataSettings.Buffs.Pain, context.fromCharacter);
						context.penetrate--;
						if (context.penetrate < 0)
						{
							base.transform.position = hitPoint;
							dead = true;
							break;
						}
					}
					continue;
				}
				dead = true;
				base.transform.position = hitPoint;
				Vector3 normal = raycastHit.normal;
				if ((bool)hitFx)
				{
					UnityEngine.Object.Instantiate(hitFx, hitPoint, Quaternion.LookRotation(normal, Vector3.up));
				}
				else
				{
					UnityEngine.Object.Instantiate(GameplayDataSettings.Prefabs.BulletHitObsticleFx, hitPoint, Quaternion.LookRotation(normal, Vector3.up));
				}
				break;
			}
		}
		if (overMaxDistance)
		{
			dead = true;
		}
		if (!dead)
		{
			base.transform.position += direction * _distanceThisFrame;
			traveledDistance += _distanceThisFrame;
		}
	}

	private void UpdateAimDirection()
	{
		base.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
	}
}
