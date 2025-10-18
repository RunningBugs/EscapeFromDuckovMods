using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar_DamageBar : MonoBehaviour
{
	[SerializeField]
	internal RectTransform rectTransform;

	[SerializeField]
	internal Image image;

	[SerializeField]
	private float duration;

	[SerializeField]
	private float targetSizeDelta = 4f;

	[SerializeField]
	private AnimationCurve curve;

	[SerializeField]
	private Gradient colorOverTime;

	private void Awake()
	{
		if (rectTransform == null)
		{
			rectTransform = base.transform as RectTransform;
		}
		if (image == null)
		{
			image = GetComponent<Image>();
		}
	}

	public async UniTask Animate(float damageBarPostion, float damageBarWidth, Action onComplete)
	{
		base.gameObject.SetActive(value: true);
		rectTransform.anchoredPosition = new Vector2(damageBarPostion, 0f);
		rectTransform.sizeDelta = new Vector2(damageBarWidth, 0f);
		float time = 0f;
		while (time < duration)
		{
			if (rectTransform == null)
			{
				return;
			}
			time += Time.deltaTime;
			float time2 = time / duration;
			float y = curve.Evaluate(time2) * targetSizeDelta;
			rectTransform.sizeDelta = new Vector2(damageBarWidth, y);
			Color color = colorOverTime.Evaluate(time2);
			image.color = color;
			await UniTask.NextFrame();
		}
		onComplete?.Invoke();
	}
}
