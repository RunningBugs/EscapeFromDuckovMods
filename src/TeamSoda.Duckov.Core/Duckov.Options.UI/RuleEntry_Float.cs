using System;
using System.Reflection;
using Duckov.Rules;
using Duckov.UI;
using UnityEngine;

namespace Duckov.Options.UI;

public class RuleEntry_Float : MonoBehaviour
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
		SetValue(current, value);
		GameRulesManager.NotifyRuleChanged();
	}

	public void RefreshValue()
	{
		float value = GetValue(GameRulesManager.Current);
		slider.SetValueWithoutNotify(value);
	}

	protected void SetValue(Ruleset ruleset, float value)
	{
		field.SetValue(ruleset, value);
	}

	protected float GetValue(Ruleset ruleset)
	{
		return (float)field.GetValue(ruleset);
	}
}
