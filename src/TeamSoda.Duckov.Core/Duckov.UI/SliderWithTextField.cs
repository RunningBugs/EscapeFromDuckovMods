using System;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class SliderWithTextField : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private TMP_InputField valueField;

	[SerializeField]
	private string valueFormat = "0";

	[SerializeField]
	private bool isPercentage;

	[SerializeField]
	private string _labelKey = "?";

	[SerializeField]
	private float value;

	public Action<float> onValueChanged;

	[LocalizationKey("Default")]
	public string LabelKey
	{
		get
		{
			return _labelKey;
		}
		set
		{
		}
	}

	public float Value
	{
		get
		{
			return GetValue();
		}
		set
		{
			SetValue(value);
		}
	}

	public void SetValueWithoutNotify(float value)
	{
		this.value = value;
		RefreshValues();
	}

	public void SetValue(float value)
	{
		SetValueWithoutNotify(value);
		onValueChanged?.Invoke(value);
	}

	public float GetValue()
	{
		return value;
	}

	private void Awake()
	{
		slider.onValueChanged.AddListener(OnSliderValueChanged);
		valueField.onEndEdit.AddListener(OnFieldEndEdit);
		RefreshLable();
		LocalizationManager.OnSetLanguage += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		LocalizationManager.OnSetLanguage -= OnLanguageChanged;
	}

	private void OnLanguageChanged(SystemLanguage language)
	{
		RefreshLable();
	}

	private void RefreshLable()
	{
		if ((bool)label)
		{
			label.text = LabelKey.ToPlainText();
		}
	}

	private void OnFieldEndEdit(string arg0)
	{
		if (float.TryParse(arg0, out var result))
		{
			if (isPercentage)
			{
				result /= 100f;
			}
			result = Mathf.Clamp(result, slider.minValue, slider.maxValue);
			Value = result;
		}
		RefreshValues();
	}

	private void OnEnable()
	{
		RefreshValues();
	}

	private void OnSliderValueChanged(float value)
	{
		Value = value;
		RefreshValues();
	}

	private void RefreshValues()
	{
		valueField.SetTextWithoutNotify(Value.ToString(valueFormat));
		slider.SetValueWithoutNotify(Value);
	}

	private void OnValidate()
	{
		RefreshLable();
	}
}
