using Cysharp.Threading.Tasks;
using Duckov;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.Events;

public class Grenade : MonoBehaviour
{
	public bool hasCollideSound;

	public string collideSound;

	public int makeSoundCount = 3;

	private float makeSoundTimeMarker = -1f;

	public float damageRange;

	public bool isDangerForAi = true;

	public bool isLandmine;

	public float landmineTriggerRange;

	private bool landmineActived;

	private bool landmineTriggerd;

	public ExplosionFxTypes fxType;

	public GameObject fx;

	public Animator animator;

	[SerializeField]
	private Rigidbody rb;

	private int groundLayer;

	public bool delayFromCollide;

	public float delayTime = 1f;

	public bool createExplosion = true;

	public float explosionShakeStrength = 1f;

	public DamageInfo damageInfo;

	private bool bindAgent;

	private ItemAgent bindedAgent;

	private float lifeTimer;

	private float delayTimer;

	private Teams selfTeam;

	public GameObject createOnExlode;

	public float destroyDelay;

	public UnityEvent onExplodeEvent;

	private bool exploded;

	private bool canHurtSelf;

	private bool collide;

	private bool needCustomFx => fxType == ExplosionFxTypes.custom;

	private void OnCollisionEnter(Collision collision)
	{
		if (!collide)
		{
			collide = true;
		}
		Vector3 velocity = rb.velocity;
		velocity.x *= 0.5f;
		velocity.z *= 0.5f;
		rb.velocity = velocity;
		rb.angularVelocity *= 0.3f;
		if (makeSoundCount > 0 && Time.time - makeSoundTimeMarker > 0.3f)
		{
			makeSoundCount--;
			makeSoundTimeMarker = Time.time;
			AISound sound = new AISound
			{
				fromObject = base.gameObject,
				pos = base.transform.position
			};
			if ((bool)damageInfo.fromCharacter)
			{
				sound.fromTeam = damageInfo.fromCharacter.Team;
			}
			else
			{
				sound.fromTeam = Teams.all;
			}
			sound.soundType = SoundTypes.unknowNoise;
			if (isDangerForAi)
			{
				sound.soundType = SoundTypes.grenadeDropSound;
			}
			sound.radius = 20f;
			AIMainBrain.MakeSound(sound);
			if (hasCollideSound && collideSound != "")
			{
				AudioManager.Post(collideSound, base.gameObject);
			}
		}
	}

	public void BindAgent(ItemAgent _agent)
	{
		bindAgent = true;
		bindedAgent = _agent;
		bindedAgent.transform.SetParent(base.transform, worldPositionStays: false);
		bindedAgent.transform.localPosition = Vector3.zero;
		bindedAgent.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		lifeTimer += Time.deltaTime;
		if (!delayFromCollide || collide)
		{
			delayTimer += Time.deltaTime;
		}
		if (bindAgent)
		{
			if (bindedAgent == null)
			{
				Debug.Log("bind  null destroied");
				Object.Destroy(base.gameObject);
			}
			else if (lifeTimer > 0.5f && !bindedAgent.gameObject.activeInHierarchy)
			{
				bindedAgent.gameObject.SetActive(value: true);
			}
		}
		else if (!exploded && delayTimer > delayTime)
		{
			exploded = true;
			if (!isLandmine)
			{
				Explode();
			}
			else
			{
				ActiveLandmine().Forget();
			}
		}
	}

	private void Explode()
	{
		if (createExplosion)
		{
			damageInfo.isExplosion = true;
			LevelManager.Instance.ExplosionManager.CreateExplosion(base.transform.position, damageRange, damageInfo, fxType, explosionShakeStrength, canHurtSelf);
		}
		if (createExplosion && needCustomFx && fx != null)
		{
			Object.Instantiate(fx, base.transform.position, Quaternion.identity);
		}
		if ((bool)createOnExlode)
		{
			Object.Instantiate(createOnExlode, base.transform.position, Quaternion.identity);
		}
		onExplodeEvent?.Invoke();
		if (rb != null)
		{
			rb.constraints = (RigidbodyConstraints)10;
		}
		if (destroyDelay <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
		else if (destroyDelay < 999f)
		{
			DestroyOverTime().Forget();
		}
	}

	private async UniTask DestroyOverTime()
	{
		await UniTask.WaitForSeconds(destroyDelay);
		if (!(base.gameObject == null))
		{
			Object.Destroy(base.gameObject);
		}
	}

	private async UniTask ActiveLandmine()
	{
		if (!landmineActived)
		{
			landmineActived = true;
			if ((bool)animator)
			{
				animator.SetBool("Actived", value: true);
			}
			OnTriggerEnterEvent trigger = new GameObject().AddComponent<OnTriggerEnterEvent>();
			SphereCollider sphereCollider = trigger.gameObject.AddComponent<SphereCollider>();
			sphereCollider.transform.SetParent(base.transform, worldPositionStays: false);
			sphereCollider.transform.localPosition = Vector3.zero;
			sphereCollider.isTrigger = true;
			sphereCollider.radius = landmineTriggerRange;
			trigger.filterByTeam = true;
			trigger.selfTeam = selfTeam;
			trigger.Init();
			await UniTask.WaitForEndOfFrame(this);
			trigger.DoOnTriggerEnter.AddListener(OnLinemineTriggerd);
		}
	}

	private void OnLinemineTriggerd()
	{
		if (!landmineTriggerd)
		{
			landmineTriggerd = true;
			Explode();
		}
	}

	public void SetWeaponIdInfo(int typeId)
	{
		damageInfo.fromWeaponItemID = typeId;
	}

	public void Launch(Vector3 startPoint, Vector3 velocity, CharacterMainControl fromCharacter, bool canHurtSelf)
	{
		this.canHurtSelf = canHurtSelf;
		groundLayer = LayerMask.NameToLayer("Ground");
		rb.position = startPoint;
		base.transform.position = startPoint;
		rb.velocity = velocity;
		Vector3 angularVelocity = (Random.insideUnitSphere + Vector3.one) * 7f;
		angularVelocity.y = 0f;
		rb.angularVelocity = angularVelocity;
		if (fromCharacter != null)
		{
			Collider component = fromCharacter.GetComponent<Collider>();
			Collider component2 = GetComponent<Collider>();
			selfTeam = fromCharacter.Team;
			IgnoreCollisionForSeconds(component, component2, 0.5f).Forget();
		}
	}

	private async UniTask IgnoreCollisionForSeconds(Collider col1, Collider col2, float ignoreTime)
	{
		if (col1 != null && col2 != null)
		{
			Physics.IgnoreCollision(col1, col2, ignore: true);
		}
		await UniTask.WaitForSeconds(ignoreTime);
		if (col1 != null && col2 != null)
		{
			Physics.IgnoreCollision(col1, col2, ignore: false);
		}
	}
}
