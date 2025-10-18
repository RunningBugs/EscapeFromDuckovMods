using Cysharp.Threading.Tasks;
using UnityEngine;

public class InteractablePickup : InteractableBase
{
	[SerializeField]
	private DuckovItemAgent itemAgent;

	public SpriteRenderer sprite;

	private Rigidbody rb;

	private Vector3 throwStartPoint;

	private bool destroied;

	public DuckovItemAgent ItemAgent => itemAgent;

	protected override bool IsInteractable()
	{
		return true;
	}

	public void OnInit()
	{
		if ((bool)itemAgent && (bool)itemAgent.Item && (bool)sprite)
		{
			sprite.sprite = itemAgent.Item.Icon;
		}
		overrideInteractName = true;
		base.InteractName = itemAgent.Item.DisplayNameRaw;
	}

	protected override void OnInteractStart(CharacterMainControl character)
	{
		character.PickupItem(itemAgent.Item);
		StopInteract();
	}

	public void Throw(Vector3 direction, float randomAngle)
	{
		throwStartPoint = base.transform.position;
		if (!rb)
		{
			rb = base.gameObject.AddComponent<Rigidbody>();
		}
		rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		if (direction.magnitude < 0.1f)
		{
			direction = Vector3.zero;
		}
		else
		{
			direction.y = 0f;
			direction.Normalize();
			direction = Quaternion.Euler(0f, Random.Range(0f - randomAngle, randomAngle) * 0.5f, 0f) * direction;
			direction *= Random.Range(0.5f, 1f) * 3f;
			direction.y = 2.5f;
		}
		rb.velocity = direction;
		DestroyRigidbody().Forget();
	}

	protected override void OnDestroy()
	{
		destroied = true;
		base.OnDestroy();
	}

	private async UniTaskVoid DestroyRigidbody()
	{
		await UniTask.WaitForSeconds(3);
		if (destroied || !rb)
		{
			return;
		}
		if (rb.velocity.y < -0.2f)
		{
			rb.transform.position = throwStartPoint;
			rb.position = throwStartPoint;
			await UniTask.WaitForSeconds(3);
		}
		if (!destroied && (bool)rb)
		{
			if (rb.velocity.y < -0.2f)
			{
				rb.transform.position = throwStartPoint;
				rb.position = throwStartPoint;
			}
			if ((bool)rb)
			{
				Object.Destroy(rb);
			}
		}
	}
}
