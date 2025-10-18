using UnityEngine;

namespace Duckov.MiniGames.Examples.FPS;

public class FPS_Enemy_HomingFly : MiniGameBehaviour
{
	[SerializeField]
	private Rigidbody rigidbody;

	[SerializeField]
	private FPSHealth health;

	private bool CanSeeTarget => false;

	private bool Dead => health.Dead;

	private void Awake()
	{
		if (rigidbody == null)
		{
			rigidbody = GetComponent<Rigidbody>();
		}
		health.onDead += OnDead;
	}

	private void OnDead(FPSHealth health)
	{
		rigidbody.useGravity = true;
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (Dead)
		{
			UpdateDead(deltaTime);
		}
		else if (CanSeeTarget)
		{
			UpdateHoming(deltaTime);
		}
		else
		{
			UpdateIdle(deltaTime);
		}
	}

	private void UpdateIdle(float deltaTime)
	{
	}

	private void UpdateDead(float deltaTime)
	{
	}

	private void UpdateHoming(float deltaTime)
	{
	}
}
