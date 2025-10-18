using System;
using System.Reflection;
using Duckov.Rules;
using Duckov.UI;
using UnityEngine;

namespace Duckov.Options.UI;

public class RuleEntry_Int : MonoBehaviour
{
	[SerializeField]
	private SliderWithTextField slider;

	[SerializeField]
	private string fieldName = "damageFactor_ToPlayer";

	private FieldInfo field;

	private void Awake()
	{
		SliderWithTextField sliderWithTextField = slider;
		sliderWithTextField.onValueChanged = (Action<float>)Delegate.Combine(sliderWithTextField.onValueChanged, new Action<float>(OnValueChanged));
		GameRulesManager.OnRuleChanged += OnRuleChanged;
		Type typeFromHandle = typeof(Ruleset);
		field = typeFromHandle.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
		RefreshValue();
	}

	private void OnRuleChanged()
	{
		RefreshValue();
	}

	private void OnValueChanged(float value)
	{
		if (GameRulesManager.SelectedRuleIndex != RuleIndex.Custom)
		{
			RefreshValue();
			return;
		}
		Ruleset current = GameRulesManager.Current;
		SetValue(current, (int)value);
		GameRulesManager.NotifyRuleChanged();
	}

	public void RefreshValue()
	{
		float valueWithoutNotify = GetValue(GameRulesManager.Current);
		slider.SetValueWithoutNotify(valueWithoutNotify);
	}

	protected void SetValue(Ruleset ruleset, int value)
	{
		field.SetValue(ruleset, value);
	}

	protected int GetValue(Ruleset ruleset)
	{
		return (int)field.GetValue(ruleset);
	}
}
