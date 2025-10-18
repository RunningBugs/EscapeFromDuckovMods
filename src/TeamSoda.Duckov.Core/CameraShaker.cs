using Cinemachine;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
	public enum CameraShakeTypes
	{
		recoil,
		explosion,
		meleeAttackHit
	}

	private static CameraShaker _instance;

	public CinemachineImpulseSource recoilSource;

	public CinemachineImpulseSource meleeAttackSource;

	public CinemachineImpulseSource explosionSource;

	private void Awake()
	{
		_instance = this;
	}

	public static void Shake(Vector3 velocity, CameraShakeTypes shakeType)
	{
		if (!(_instance == null))
		{
			switch (shakeType)
			{
			case CameraShakeTypes.recoil:
				_instance.recoilSource.GenerateImpulseWithVelocity(velocity);
				break;
			case CameraShakeTypes.explosion:
				_instance.explosionSource.GenerateImpulseWithVelocity(velocity);
				break;
			case CameraShakeTypes.meleeAttackHit:
				_instance.meleeAttackSource.GenerateImpulseWithVelocity(velocity);
				break;
			}
		}
	}
}
