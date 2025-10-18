using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class ScrollViewBorderFrame : MonoBehaviour
{
	[SerializeField]
	private ScrollRect scrollRect;

	[Range(0f, 1f)]
	[SerializeField]
	private float maxAlpha = 1f;

	[SerializeField]
	private float extendThreshold = 10f;

	[SerializeField]
	private float extendOffset;

	[SerializeField]
	private Graphic upGraphic;

	[SerializeField]
	private Graphic downGraphic;

	[SerializeField]
	private Graphic leftGraphic;

	[SerializeField]
	private Graphic rightGraphic;

	private void OnEnable()
	{
		scrollRect.onValueChanged.AddListener(Refresh);
		UniTask.Void(async delegate
		{
			await UniTask.Yield();
			await UniTask.Yield();
			await UniTask.Yield();
			Refresh();
		});
	}

	private void OnDisable()
	{
		scrollRect.onValueChanged.RemoveListener(Refresh);
	}

	private void Start()
	{
		Refresh();
	}

	private void Refresh(Vector2 scrollPos)
	{
		RectTransform viewport = scrollRect.viewport;
		RectTransform content = scrollRect.content;
		Rect rect = viewport.rect;
		Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, content);
		float num = bounds.max.y - rect.max.y + extendOffset;
		float num2 = rect.min.y - bounds.min.y + extendOffset;
		float num3 = rect.min.x - bounds.min.x + extendOffset;
		float num4 = bounds.max.x - rect.max.x + extendOffset;
		float alpha = Mathf.Lerp(0f, maxAlpha, num / extendThreshold);
		float alpha2 = Mathf.Lerp(0f, maxAlpha, num2 / extendThreshold);
		float alpha3 = Mathf.Lerp(0f, maxAlpha, num3 / extendThreshold);
		float alpha4 = Mathf.Lerp(0f, maxAlpha, num4 / extendThreshold);
		SetAlpha(upGraphic, alpha);
		SetAlpha(downGraphic, alpha2);
		SetAlpha(leftGraphic, alpha3);
		SetAlpha(rightGraphic, alpha4);
		static void SetAlpha(Graphic graphic, float a)
		{
			if (!(graphic == null))
			{
				Color color = graphic.color;
				color.a = a;
				graphic.color = color;
			}
		}
	}

	private void Refresh()
	{
		if (!(scrollRect == null))
		{
			Refresh(scrollRect.normalizedPosition);
		}
	}
}
