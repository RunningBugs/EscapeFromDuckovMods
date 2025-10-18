using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecomposeSlider : MonoBehaviour
{
	[SerializeField]
	private Slider slider;

	public TextMeshProUGUI minText;

	public TextMeshProUGUI maxText;

	public TextMeshProUGUI valueText;

	public int Value
	{
		get
		{
			return Mathf.RoundToInt(slider.value);
		}
		set
		{
			slider.value = value;
			valueText.text = value.ToString();
		}
	}

	public event Action<float> OnValueChangedEvent;

	private void Awake()
	{
		slider.onValueChanged.AddListener(OnValueChanged);
	}

	private void OnDestroy()
	{
		slider.onValueChanged.RemoveListener(OnValueChanged);
	}

	private void OnValueChanged(float value)
	{
		this.OnValueChangedEvent(value);
		valueText.text = value.ToString();
	}

	public void SetMinMax(int min, int max)
	{
		slider.minValue = min;
		slider.maxValue = max;
		minText.text = min.ToString();
		maxText.text = max.ToString();
	}
}
