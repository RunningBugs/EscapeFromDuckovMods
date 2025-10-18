using DG.Tweening;
using UnityEngine;

namespace Duckov.UI;

public class PunchReceiver : MonoBehaviour
{
	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private ParticleSystem particle;

	[Min(0.0001f)]
	[SerializeField]
	private float duration = 0.01f;

	public int vibrato = 10;

	public float elasticity = 1f;

	[SerializeField]
	private Vector2 punchAnchorPosition;

	[SerializeField]
	[Range(-1f, 1f)]
	private float punchScaleUniform;

	[SerializeField]
	[Range(-180f, 180f)]
	private float punchRotationZ;

	[SerializeField]
	private Vector2 randomAnchorPosition;

	[SerializeField]
	[Range(0f, 180f)]
	private float randomRotationZ;

	[SerializeField]
	private AnimationCurve animationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private bool cacheWhenPunched;

	[SerializeField]
	private string sfx;

	private Vector2 cachedAnchorPosition;

	private Vector2 cachedScale;

	private Vector2 cachedRotation;

	private float PunchAnchorPositionDuration => duration;

	private float PunchScaleDuration => duration;

	private float PunchRotationDuration => duration;

	private bool ShouldPunchPosition
	{
		get
		{
			if (randomAnchorPosition.magnitude > 0.001f)
			{
				return punchAnchorPosition.magnitude > 0.001f;
			}
			return false;
		}
	}

	private void Awake()
	{
		if (rectTransform == null)
		{
			rectTransform = GetComponent<RectTransform>();
		}
		CachePose();
	}

	private void Start()
	{
	}

	[ContextMenu("Punch")]
	public void Punch()
	{
		if (base.enabled && !(rectTransform == null))
		{
			if (particle != null)
			{
				particle.Play();
			}
			rectTransform.DOKill();
			if (cacheWhenPunched)
			{
				CachePose();
			}
			Vector2 punch = punchAnchorPosition + new Vector2(Random.Range(0f - randomAnchorPosition.x, randomAnchorPosition.x), Random.Range(0f - randomAnchorPosition.y, randomAnchorPosition.y));
			float num = punchScaleUniform;
			float num2 = punchRotationZ + Random.Range(0f - randomRotationZ, randomRotationZ);
			if (ShouldPunchPosition)
			{
				rectTransform.DOPunchAnchorPos(punch, PunchAnchorPositionDuration, vibrato, elasticity).SetEase(animationCurve).OnKill(RestorePose);
			}
			rectTransform.DOPunchScale(Vector3.one * num, PunchScaleDuration, vibrato, elasticity).SetEase(animationCurve).OnKill(RestorePose);
			rectTransform.DOPunchRotation(Vector3.forward * num2, PunchRotationDuration, vibrato, elasticity).SetEase(animationCurve).OnKill(RestorePose);
			if (!string.IsNullOrWhiteSpace(sfx))
			{
				AudioManager.Post(sfx);
			}
		}
	}

	private void CachePose()
	{
		if (!(rectTransform == null))
		{
			cachedAnchorPosition = rectTransform.anchoredPosition;
			cachedScale = rectTransform.localScale;
			cachedRotation = rectTransform.localRotation.eulerAngles;
		}
	}

	private void RestorePose()
	{
		if (!(rectTransform == null))
		{
			if (ShouldPunchPosition)
			{
				rectTransform.anchoredPosition = cachedAnchorPosition;
			}
			rectTransform.localScale = cachedScale;
			rectTransform.localRotation = Quaternion.Euler(cachedRotation);
		}
	}

	private void OnDestroy()
	{
		rectTransform?.DOKill();
	}
}
