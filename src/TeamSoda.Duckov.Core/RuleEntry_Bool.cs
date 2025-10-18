using System.Reflection;
using Duckov.Rules;
using SodaCraft.Localizations;
using UnityEngine;

public class RuleEntry_Bool : OptionsProviderBase
{
	[SerializeField]
	private string fieldName;

	private FieldInfo field;

	public override string Key => fieldName;

	private void Awake()
	{
		field = typeof(Ruleset).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public override string GetCurrentOption()
	{
		Ruleset current = GameRulesManager.Current;
		if ((bool)field.GetValue(current))
		{
			return "Options_On".ToPlainText();
		}
		return "Options_Off".ToPlainText();
	}

	public override string[] GetOptions()
	{
		return new string[2]
		{
			"Options_Off".ToPlainText(),
			"Options_On".ToPlainText()
		};
	}

	public override void Set(int index)
	{
		bool flag = index > 0;
		Ruleset current = GameRulesManager.Current;
		field.SetValue(current, flag);
	}
}
