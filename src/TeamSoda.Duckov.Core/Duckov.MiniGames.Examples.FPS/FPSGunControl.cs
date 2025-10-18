using UnityEngine;

namespace Duckov.MiniGames.Examples.FPS;

public class FPSGunControl : MiniGameBehaviour
{
	[SerializeField]
	private Camera mainCamera;

	[SerializeField]
	private Transform gunParent;

	[SerializeField]
	private FPSGun gun;

	public FPSGun Gun => gun;

	public float ScatterAngle
	{
		get
		{
			if ((bool)Gun)
			{
				return Gun.ScatterAngle;
			}
			return 0f;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (gun != null)
		{
			SetGun(gun);
		}
	}

	protected override void OnUpdate(float deltaTime)
	{
		bool buttonDown = base.Game.GetButtonDown(MiniGame.Button.A);
		bool buttonUp = base.Game.GetButtonUp(MiniGame.Button.A);
		if (buttonDown)
		{
			gun.SetTrigger(value: true);
		}
		if (buttonUp)
		{
			gun.SetTrigger(value: false);
		}
		UpdateGunPhysicsStatus(deltaTime);
	}

	private void UpdateGunPhysicsStatus(float deltaTime)
	{
	}

	private void SetGun(FPSGun gunInstance)
	{
		if (gunInstance != gun)
		{
			Object.Destroy(gun);
		}
		gun = gunInstance;
		SetupGunData();
	}

	private void SetupGunData()
	{
		gun.Setup(mainCamera, gunParent);
	}
}
