using Duckov.Rules;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;

public class RuleIndexDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	private void Awake()
	{
		LocalizationManager.OnSetLanguage += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		LocalizationManager.OnSetLanguage -= OnLanguageChanged;
	}

	private void OnLanguageChanged(SystemLanguage language)
	{
		Refresh();
	}

	private void OnEnable()
	{
		Refresh();
	}

	private void Refresh()
	{
		text.text = $"Rule_{GameRulesManager.SelectedRuleIndex}".ToPlainText();
	}
}
