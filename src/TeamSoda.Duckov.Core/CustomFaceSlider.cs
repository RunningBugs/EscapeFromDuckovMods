using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomFaceSlider : MonoBehaviour
{
	[SerializeField]
	private Slider slider;

	[SerializeField]
	private TMP_InputField valueField;

	[SerializeField]
	private string valueFormat = "0.##";

	[SerializeField]
	private TextLocalizor nameText;

	private CustomFaceUI master;

	public float Value => slider.value;

	private void Awake()
	{
		slider.onValueChanged.AddListener(OnSliderValueChanged);
		valueField.onEndEdit.AddListener(OnEndEditField);
	}

	private void Start()
	{
		RefreshFieldText();
	}

	private void OnEndEditField(string str)
	{
		if (float.TryParse(str, out var result))
		{
			result = Mathf.Clamp(result, slider.minValue, slider.maxValue);
			slider.SetValueWithoutNotify(result);
			master.SetDirty();
		}
		RefreshFieldText();
	}

	private void OnDestroy()
	{
		slider.onValueChanged.RemoveListener(OnSliderValueChanged);
	}

	public void Init(float minValue, float maxValue, CustomFaceUI _master, string nameKey)
	{
		master = _master;
		SetMinMaxValue(minValue, maxValue);
		SetNameKey(nameKey);
	}

	private void OnSliderValueChanged(float _value)
	{
		valueField.SetTextWithoutNotify(_value.ToString(valueFormat));
		master.SetDirty();
	}

	public void SetNameKey(string _nameKey)
	{
		nameText.Key = _nameKey;
	}

	public void SetMinMaxValue(float min, float max)
	{
		slider.minValue = min;
		slider.maxValue = max;
	}

	public void SetValue(float value)
	{
		slider.value = value;
	}

	private void RefreshFieldText()
	{
		valueField.SetTextWithoutNotify(slider.value.ToString(valueFormat));
	}
}
