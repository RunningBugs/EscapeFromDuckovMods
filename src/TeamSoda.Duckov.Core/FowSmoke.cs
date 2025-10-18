using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class FowSmoke : MonoBehaviour
{
	[SerializeField]
	private int res = 8;

	[SerializeField]
	private float radius;

	[SerializeField]
	private float height;

	[SerializeField]
	private float thickness;

	public Transform colParent;

	public ParticleSystem[] particles;

	public float startTime;

	public float lifeTime;

	public float particleFadeTime = 3f;

	public UnityEvent beforeFadeOutEvent;

	private void Start()
	{
		UpdateSmoke().Forget();
	}

	private async UniTaskVoid UpdateSmoke()
	{
		if (colParent == null)
		{
			return;
		}
		colParent.localScale = Vector3.one * 0.01f;
		float startTimer = 0f;
		while (startTimer < startTime)
		{
			await UniTask.WaitForEndOfFrame(this);
			if (colParent == null)
			{
				return;
			}
			startTimer += Time.deltaTime;
			colParent.localScale = Vector3.one * Mathf.Clamp01(startTimer / startTime);
		}
		await UniTask.WaitForSeconds(startTime);
		if (colParent != null)
		{
			colParent.gameObject.SetActive(value: true);
		}
		await UniTask.WaitForSeconds(lifeTime);
		beforeFadeOutEvent?.Invoke();
		for (int i = 0; i < particles.Length; i++)
		{
			if (!(particles[i] == null))
			{
				ParticleSystem.EmissionModule emission = particles[i].emission;
				emission.rateOverTime = 0f;
			}
		}
		float dieTimer = 0f;
		while (dieTimer < particleFadeTime)
		{
			await UniTask.WaitForEndOfFrame(this);
			dieTimer += Time.deltaTime;
			float num = Mathf.Clamp01(dieTimer / particleFadeTime);
			if (colParent == null)
			{
				return;
			}
			colParent.localScale = Vector3.one * (1f - num);
		}
		if (base.gameObject != null)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(base.transform.position, radius);
	}
}
