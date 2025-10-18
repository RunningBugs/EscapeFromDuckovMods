using Duckov;
using Duckov.Quests;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

public class Condition_CharacterLevel : Condition
{
	private enum Relation
	{
		LessThan = 1,
		Equals = 2,
		GreaterThan = 4
	}

	[SerializeField]
	private Relation relation;

	[SerializeField]
	private int level;

	[LocalizationKey("Default")]
	private string DisplayTextFormatKey => relation switch
	{
		Relation.LessThan => "Condition_CharacterLevel_LessThan", 
		Relation.Equals => "Condition_CharacterLevel_Equals", 
		Relation.GreaterThan => "Condition_CharacterLevel_GreaterThan", 
		_ => "", 
	};

	private string DisplayTextFormat => DisplayTextFormatKey.ToPlainText();

	public override string DisplayText => DisplayTextFormat.Format(new { level });

	public override bool Evaluate()
	{
		int num = EXPManager.Level;
		return relation switch
		{
			Relation.LessThan => num <= level, 
			Relation.Equals => num == level, 
			Relation.GreaterThan => num >= level, 
			_ => false, 
		};
	}
}
