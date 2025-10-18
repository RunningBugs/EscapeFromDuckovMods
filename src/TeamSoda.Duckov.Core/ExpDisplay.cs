using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI txtLevel;

	[SerializeField]
	private TextMeshProUGUI txtCurrentExp;

	[SerializeField]
	private TextMeshProUGUI txtMaxExp;

	[SerializeField]
	private Image expBarFill;

	[SerializeField]
	private bool snapToCurrentOnEnable;

	[SerializeField]
	private float animationDuration = 0.1f;

	[SerializeField]
	private AnimationCurve animationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private long displayExp;

	private int displayingLevel = -1;

	private Dictionary<int, (long from, long to)> cachedLevelExpRange = new Dictionary<int, (long, long)>();

	private int currentToken;

	private void Refresh()
	{
		EXPManager instance = EXPManager.Instance;
		if (!(instance == null))
		{
			int num = instance.LevelFromExp(displayExp);
			if (displayingLevel != num)
			{
				displayingLevel = num;
				OnDisplayingLevelChanged();
			}
			(long, long) levelExpRange = GetLevelExpRange(num);
			long num2 = levelExpRange.Item2 - levelExpRange.Item1;
			txtLevel.text = num.ToString();
			txtCurrentExp.text = displayExp.ToString();
			string text = ((levelExpRange.Item2 != long.MaxValue) ? levelExpRange.Item2.ToString() : "∞");
			txtMaxExp.text = text;
			float fillAmount = (float)((double)(displayExp - levelExpRange.Item1) / (double)num2);
			expBarFill.fillAmount = fillAmount;
		}
	}

	private void OnDisplayingLevelChanged()
	{
	}

	private (long from, long to) GetLevelExpRange(int level)
	{
		if (cachedLevelExpRange.TryGetValue(level, out (long, long) value))
		{
			return value;
		}
		EXPManager instance = EXPManager.Instance;
		if (instance == null)
		{
			return (from: 0L, to: 0L);
		}
		(long, long) levelExpRange = instance.GetLevelExpRange(level);
		cachedLevelExpRange[level] = levelExpRange;
		return levelExpRange;
	}

	private void SnapToCurrent()
	{
		displayExp = EXPManager.EXP;
		Refresh();
	}

	private async UniTask Animate(long targetExp, float duration, AnimationCurve curve)
	{
		int token = (currentToken = UnityEngine.Random.Range(int.MinValue, int.MaxValue));
		if (!(duration <= 0f))
		{
			float time = 0f;
			long from = displayExp;
			while (time < duration)
			{
				if (currentToken != token)
				{
					return;
				}
				float t = curve.Evaluate(time / duration);
				displayExp = LongLerp(from, targetExp, t);
				time += Time.deltaTime;
				Refresh();
				await UniTask.WaitForEndOfFrame(this);
			}
		}
		displayExp = targetExp;
		Refresh();
	}

	private long LongLerp(long a, long b, float t)
	{
		long num = b - a;
		return a + (long)(t * (float)num);
	}

	private void OnEnable()
	{
		if (snapToCurrentOnEnable)
		{
			SnapToCurrent();
		}
		RegisterEvents();
	}

	private void OnDisable()
	{
		UnregisterEvents();
	}

	private void RegisterEvents()
	{
		EXPManager.onExpChanged = (Action<long>)Delegate.Combine(EXPManager.onExpChanged, new Action<long>(OnExpChanged));
	}

	private void UnregisterEvents()
	{
		EXPManager.onExpChanged = (Action<long>)Delegate.Remove(EXPManager.onExpChanged, new Action<long>(OnExpChanged));
	}

	private void OnExpChanged(long exp)
	{
		Animate(exp, animationDuration, animationCurve).Forget();
	}
}
