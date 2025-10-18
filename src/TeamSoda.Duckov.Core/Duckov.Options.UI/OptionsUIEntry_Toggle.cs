using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.Options.UI;

public class OptionsUIEntry_Toggle : MonoBehaviour
{
	[SerializeField]
	private string key;

	[SerializeField]
	private bool defaultValue;

	[Space]
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private Slider toggle;

	[LocalizationKey("Default")]
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

	public bool Value
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

	private int SliderValue
	{
		get
		{
			if (!Value)
			{
				return 0;
			}
			return 1;
		}
	}

	private void Awake()
	{
		toggle.wholeNumbers = true;
		toggle.minValue = 0f;
		toggle.maxValue = 1f;
		toggle.onValueChanged.AddListener(OnToggleValueChanged);
		label.text = labelKey.ToPlainText();
	}

	private void OnEnable()
	{
		toggle.SetValueWithoutNotify(SliderValue);
	}

	private void OnToggleValueChanged(float value)
	{
		Value = value > 0f;
	}
}
