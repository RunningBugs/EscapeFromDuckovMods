using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Duckov.UI.Animations;

public class RectTransformFade : FadeElement
{
	[SerializeField]
	private bool debug;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private float duration = 0.4f;

	[SerializeField]
	private Vector2 offset = Vector2.left * 10f;

	[SerializeField]
	[Range(-1f, 1f)]
	private float uniformScale;

	[SerializeField]
	[Range(-180f, 180f)]
	private float rotateZ;

	[SerializeField]
	private AnimationCurve showingAnimationCurve;

	[SerializeField]
	private AnimationCurve hidingAnimationCurve;

	private Vector2 cachedAnchordPosition = Vector2.zero;

	private Vector3 cachedScale = Vector3.one;

	private Vector3 cachedRotation = Vector3.zero;

	private bool initialized;

	private Vector2 TargetAnchoredPosition => cachedAnchordPosition + offset;

	private Vector3 TargetScale => cachedScale + Vector3.one * uniformScale;

	private Vector3 TargetRotation => cachedRotation + Vector3.forward * rotateZ;

	private void Initialize()
	{
		if (initialized)
		{
			Debug.LogError("Object Initialized Twice, aborting");
			return;
		}
		CachePose();
		initialized = true;
	}

	private void CachePose()
	{
		if (!(rectTransform == null))
		{
			cachedAnchordPosition = rectTransform.anchoredPosition;
			cachedScale = rectTransform.localScale;
			cachedRotation = rectTransform.localRotation.eulerAngles;
		}
	}

	private void Awake()
	{
		if (rectTransform == null || rectTransform.gameObject != base.gameObject)
		{
			rectTransform = GetComponent<RectTransform>();
		}
		if (!initialized)
		{
			Initialize();
		}
	}

	private void OnValidate()
	{
		if (rectTransform == null || rectTransform.gameObject != base.gameObject)
		{
			rectTransform = GetComponent<RectTransform>();
		}
	}

	protected override async UniTask HideTask(int token)
	{
		if (!initialized)
		{
			Initialize();
		}
		UniTask uniTask = rectTransform.DOAnchorPos(TargetAnchoredPosition, duration).SetEase(hidingAnimationCurve).OnComplete(delegate
		{
			rectTransform.anchoredPosition = TargetAnchoredPosition;
		})
			.ToUniTask();
		UniTask uniTask2 = rectTransform.DOScale(TargetScale, duration).SetEase(showingAnimationCurve).OnComplete(delegate
		{
			rectTransform.localScale = TargetScale;
		})
			.ToUniTask();
		UniTask uniTask3 = rectTransform.DOLocalRotate(TargetRotation, duration).SetEase(showingAnimationCurve).OnComplete(delegate
		{
			rectTransform.localRotation = Quaternion.Euler(TargetRotation);
		})
			.ToUniTask();
		await UniTask.WhenAll(uniTask, uniTask2, uniTask3);
	}

	protected override async UniTask ShowTask(int token)
	{
		if (!initialized)
		{
			Initialize();
		}
		UniTask uniTask = rectTransform.DOAnchorPos(cachedAnchordPosition, duration).SetEase(showingAnimationCurve).OnComplete(delegate
		{
			rectTransform.anchoredPosition = cachedAnchordPosition;
			if (debug)
			{
				Debug.Log($"Move Complete {base.gameObject.activeInHierarchy}");
			}
		})
			.ToUniTask();
		UniTask uniTask2 = rectTransform.DOScale(cachedScale, duration).SetEase(showingAnimationCurve).OnComplete(delegate
		{
			rectTransform.localScale = cachedScale;
		})
			.ToUniTask();
		UniTask uniTask3 = rectTransform.DOLocalRotate(cachedRotation, duration).SetEase(showingAnimationCurve).OnComplete(delegate
		{
			rectTransform.localRotation = Quaternion.Euler(cachedRotation);
		})
			.ToUniTask();
		await UniTask.WhenAll(uniTask, uniTask2, uniTask3);
		if (debug)
		{
			Debug.Log("Ending Show Task");
		}
	}

	protected override void OnSkipHide()
	{
		if (debug)
		{
			Debug.Log("OnSkipHide");
		}
		if (!initialized)
		{
			Initialize();
		}
		rectTransform.anchoredPosition = TargetAnchoredPosition;
		rectTransform.localScale = TargetScale;
		rectTransform.localRotation = Quaternion.Euler(TargetRotation);
	}

	private void OnDestroy()
	{
		rectTransform?.DOKill();
	}

	protected override void OnSkipShow()
	{
		if (debug)
		{
			Debug.Log("OnSkipShow");
		}
		if (!initialized)
		{
			Initialize();
		}
		rectTransform.anchoredPosition = cachedAnchordPosition;
		rectTransform.localScale = cachedScale;
		rectTransform.localRotation = Quaternion.Euler(cachedRotation);
	}
}
