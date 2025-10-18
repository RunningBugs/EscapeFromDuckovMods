using Duckov.Rules;
using Saves;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.CustomerService;

public class QuestionairButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private string addressCN = "rsmTLx1";

	private string addressJP = "mHE3yAa";

	private string addressEN = "YdoJpod";

	private string format = "https://usersurvey.biligame.com/vm/{address}.aspx?sojumpparm={id}|{difficulty}|{time}|{level}";

	public string GenerateQuestionair()
	{
		string address = LocalizationManager.CurrentLanguage switch
		{
			SystemLanguage.ChineseSimplified => addressCN, 
			SystemLanguage.Japanese => addressJP, 
			_ => addressEN, 
		};
		int currentSlot = SavesSystem.CurrentSlot;
		string id = $"{PlatformInfo.Platform}_{PlatformInfo.GetID()}";
		string time = $"{GameClock.GetRealTimePlayedOfSaveSlot(currentSlot).TotalMinutes:0}";
		string level = $"{EXPManager.Level}";
		RuleIndex ruleIndexOfSaveSlot = GameRulesManager.GetRuleIndexOfSaveSlot(currentSlot);
		int num = 0;
		switch (ruleIndexOfSaveSlot)
		{
		case RuleIndex.Custom:
			num = 0;
			break;
		case RuleIndex.ExtraEasy:
			num = 1;
			break;
		case RuleIndex.Easy:
			num = 2;
			break;
		case RuleIndex.Standard:
			num = 3;
			break;
		case RuleIndex.Hard:
			num = 4;
			break;
		case RuleIndex.ExtraHard:
			num = 5;
			break;
		}
		string difficulty = $"{num}";
		return format.Format(new { address, id, time, level, difficulty });
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Application.OpenURL(GenerateQuestionair());
	}
}
