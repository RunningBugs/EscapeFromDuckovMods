using DG.Tweening;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class BarDisplay : MonoBehaviour
{
	[SerializeField]
	private string labelText;

	[SerializeField]
	private Color color = Color.red;

	[SerializeField]
	private float animateDuration = 0.25f;

	[SerializeField]
	private TextMeshProUGUI text_Label;

	[SerializeField]
	private TextMeshProUGUI text_Current;

	[SerializeField]
	private TextMeshProUGUI text_Max;

	[SerializeField]
	private Image fill;

	private void Awake()
	{
		fill.fillAmount = 0f;
		ApplyLook();
	}

	public void Setup(string labelText, Color color, float current, float max, string format = "0.#", float min = 0f)
	{
		SetupLook(labelText, color);
		SetValue(current, max, format, min);
	}

	public void Setup(string labelText, Color color, int current, int max, int min = 0)
	{
		SetupLook(labelText, color);
		SetValue(current, max, min);
	}

	public void SetupLook(string labelText, Color color)
	{
		this.labelText = labelText;
		this.color = color;
		ApplyLook();
	}

	private void ApplyLook()
	{
		text_Label.text = labelText.ToPlainText();
		fill.color = color;
	}

	public void SetValue(float current, float max, string format = "0.#", float min = 0f)
	{
		text_Current.text = current.ToString(format);
		text_Max.text = max.ToString(format);
		float num = max - min;
		float endValue = 1f;
		if (num > 0f)
		{
			endValue = (current - min) / num;
		}
		fill.DOKill();
		fill.DOFillAmount(endValue, animateDuration).SetEase(Ease.OutCubic);
	}

	public void SetValue(int current, int max, int min = 0)
	{
		text_Current.text = current.ToString();
		text_Max.text = max.ToString();
		int num = max - min;
		float endValue = 1f;
		if (num > 0)
		{
			endValue = (float)(current - min) / (float)num;
		}
		fill.DOKill();
		fill.DOFillAmount(endValue, animateDuration).SetEase(Ease.OutCubic);
	}
}
