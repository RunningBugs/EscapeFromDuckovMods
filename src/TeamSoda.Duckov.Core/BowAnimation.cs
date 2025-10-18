using UnityEngine;

public class BowAnimation : MonoBehaviour
{
	public ItemAgent_Gun gunAgent;

	public Animator animator;

	private int hash_Loaded = "Loaded".GetHashCode();

	private int hash_Aiming = "Aiming".GetHashCode();

	private int hash_Shoot = "Shoot".GetHashCode();

	private void Start()
	{
		if (gunAgent != null)
		{
			gunAgent.OnShootEvent += OnShoot;
			gunAgent.OnLoadedEvent += OnLoaded;
			if (gunAgent.BulletCount > 0)
			{
				OnLoaded();
			}
		}
	}

	private void OnDestroy()
	{
		if (gunAgent != null)
		{
			gunAgent.OnShootEvent -= OnShoot;
			gunAgent.OnLoadedEvent -= OnLoaded;
		}
	}

	private void OnShoot()
	{
		animator.SetTrigger("Shoot");
		if (gunAgent.BulletCount <= 0)
		{
			animator.SetBool("Loaded", value: false);
		}
	}

	private void OnLoaded()
	{
		animator.SetBool("Loaded", value: true);
	}
}
