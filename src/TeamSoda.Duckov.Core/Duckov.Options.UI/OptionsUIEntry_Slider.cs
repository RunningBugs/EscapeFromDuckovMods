using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.Options.UI;

public class OptionsUIEntry_Slider : MonoBehaviour
{
	[SerializeField]
	private string key;

	[Space]
	[SerializeField]
	private float defaultValue;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private TMP_InputField valueField;

	[SerializeField]
	private string valueFormat = "0";

	[LocalizationKey("Options")]
	private string labelKey
	{
		get
		{
			return "Options_" + key;
		}
		set
		{
		}
	}

	public float Value
	{
		get
		{
			return OptionsManager.Load(key, defaultValue);
		}
		set
		{
			OptionsManager.Save(key, value);
		}
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
			label.text = labelKey.ToPlainText();
		}
	}

	private void OnFieldEndEdit(string arg0)
	{
		if (float.TryParse(arg0, out var result))
		{
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
