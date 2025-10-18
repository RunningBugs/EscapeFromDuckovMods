using UnityEngine;

public class DestroyOvertime : MonoBehaviour
{
	public float life = 1f;

	private void Awake()
	{
		if (life <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Update()
	{
		life -= Time.deltaTime;
		if (life <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnValidate()
	{
		ProcessParticleSystem();
	}

	private void ProcessParticleSystem()
	{
		float num = 0f;
		ParticleSystem component = GetComponent<ParticleSystem>();
		if (!component)
		{
			return;
		}
		if (component != null)
		{
			ParticleSystem.MainModule main = component.main;
			main.stopAction = ParticleSystemStopAction.None;
			if (main.startLifetime.constant > num)
			{
				num = main.startLifetime.constant;
			}
		}
		ParticleSystem[] componentsInChildren = base.transform.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			ParticleSystem.MainModule main2 = componentsInChildren[i].main;
			main2.stopAction = ParticleSystemStopAction.None;
			if (main2.startLifetime.constant > num)
			{
				num = main2.startLifetime.constant;
			}
		}
		life = num + 0.2f;
	}
}
