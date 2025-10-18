using System.Collections.Generic;
using System.Linq;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.Options.UI;

public class OptionsUIEntry_Dropdown : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler
{
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private OptionsProviderBase provider;

	[SerializeField]
	private TMP_Dropdown dropdown;

	private string optionKey
	{
		get
		{
			if (provider == null)
			{
				return "";
			}
			return provider.Key;
		}
	}

	[LocalizationKey("Options")]
	public string LabelKey
	{
		get
		{
			return "Options_" + optionKey;
		}
		set
		{
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		SetupDropdown();
	}

	private void SetupDropdown()
	{
		if ((bool)provider)
		{
			List<string> list = provider.GetOptions().ToList();
			string currentOption = provider.GetCurrentOption();
			int num = list.IndexOf(currentOption);
			if (num < 0)
			{
				list.Insert(0, currentOption);
				num = 0;
			}
			dropdown.ClearOptions();
			dropdown.AddOptions(list.ToList());
			dropdown.SetValueWithoutNotify(num);
		}
	}

	private void Awake()
	{
		LocalizationManager.OnSetLanguage += OnSetLanguage;
		dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
		label.text = LabelKey.ToPlainText();
	}

	private void Start()
	{
		SetupDropdown();
	}

	private void OnDestroy()
	{
		LocalizationManager.OnSetLanguage -= OnSetLanguage;
	}

	private void OnSetLanguage(SystemLanguage language)
	{
		SetupDropdown();
		label.text = LabelKey.ToPlainText();
	}

	private void OnDropdownValueChanged(int index)
	{
		if ((bool)provider)
		{
			int num = provider.GetOptions().ToList().IndexOf(dropdown.options[index].text);
			if (num >= 0)
			{
				provider.Set(num);
			}
			SetupDropdown();
		}
	}

	private void OnValidate()
	{
		if ((bool)label)
		{
			label.text = LabelKey.ToPlainText();
		}
	}
}
