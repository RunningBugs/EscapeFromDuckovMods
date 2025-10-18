using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class BlackScreen : MonoBehaviour
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private MaterialPropertyFade fadeElement;

	[SerializeField]
	private Image fadeImage;

	[SerializeField]
	private float defaultDuration = 0.5f;

	[SerializeField]
	private AnimationCurve defaultShowCurve;

	[SerializeField]
	private AnimationCurve defaultHideCurve;

	private int taskCounter;

	public static BlackScreen Instance => GameManager.BlackScreen;

	private void Awake()
	{
		if (Instance != this)
		{
			Debug.LogError("检测到应当删除的BlackScreen实例", base.gameObject);
		}
	}

	private void SetFadeCurve(AnimationCurve curve)
	{
		fadeElement.ShowCurve = curve;
		fadeElement.HideCurve = curve;
	}

	private void SetCircleFade(float circleFade)
	{
		fadeImage.material.SetFloat("_CircleFade", circleFade);
	}

	private UniTask LShowAndReturnTask(AnimationCurve animationCurve = null, float circleFade = 0f, float duration = -1f)
	{
		taskCounter++;
		if (taskCounter > 1)
		{
			return UniTask.CompletedTask;
		}
		fadeElement.Duration = ((duration > 0f) ? duration : defaultDuration);
		if (animationCurve == null)
		{
			SetFadeCurve(defaultShowCurve);
		}
		else
		{
			SetFadeCurve(animationCurve);
		}
		SetCircleFade(circleFade);
		return fadeGroup.ShowAndReturnTask();
	}

	private UniTask LHideAndReturnTask(AnimationCurve animationCurve = null, float circleFade = 0f, float duration = -1f)
	{
		if (--taskCounter > 0)
		{
			return UniTask.CompletedTask;
		}
		fadeElement.Duration = ((duration > 0f) ? duration : defaultDuration);
		if (animationCurve == null)
		{
			SetFadeCurve(defaultHideCurve);
		}
		else
		{
			SetFadeCurve(animationCurve);
		}
		SetCircleFade(circleFade);
		return fadeGroup.HideAndReturnTask();
	}

	public static UniTask ShowAndReturnTask(AnimationCurve animationCurve = null, float circleFade = 0f, float duration = 0.5f)
	{
		if (Instance == null)
		{
			return UniTask.CompletedTask;
		}
		return Instance.LShowAndReturnTask(animationCurve, circleFade, duration);
	}

	public static UniTask HideAndReturnTask(AnimationCurve animationCurve = null, float circleFade = 0f, float duration = 0.5f)
	{
		if (Instance == null)
		{
			return UniTask.CompletedTask;
		}
		return Instance.LHideAndReturnTask(animationCurve, circleFade, duration);
	}
}
