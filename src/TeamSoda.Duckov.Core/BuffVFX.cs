using Duckov.Buffs;
using UnityEngine;

public class BuffVFX : MonoBehaviour
{
	public Buff buff;

	public GameObject shockFxPfb;

	private GameObject shockFxInstance;

	public Vector3 offsetFromCharacter;

	private void Awake()
	{
		if (!buff)
		{
			buff = GetComponent<Buff>();
		}
		buff.OnSetupEvent.AddListener(OnSetup);
	}

	private void OnSetup()
	{
		if (shockFxInstance != null)
		{
			Object.Destroy(shockFxInstance);
		}
		if ((bool)buff && (bool)buff.Character && (bool)shockFxPfb)
		{
			shockFxInstance = Object.Instantiate(shockFxPfb, buff.Character.transform);
			shockFxInstance.transform.localPosition = offsetFromCharacter;
			shockFxInstance.transform.localRotation = Quaternion.identity;
		}
	}

	private void OnDestroy()
	{
		if (shockFxInstance != null)
		{
			Object.Destroy(shockFxInstance);
		}
	}

	public void AutoSetup()
	{
		buff = GetComponent<Buff>();
	}
}
