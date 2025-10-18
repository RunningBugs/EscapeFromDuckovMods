using System;
using Saves;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.Rules;

public class GameRulesManager : MonoBehaviour
{
	[Serializable]
	private struct RuleIndexFileEntry
	{
		public RuleIndex index;

		public RulesetFile file;
	}

	private const string SelectedRuleIndexSaveKey = "GameRulesManager_RuleIndex";

	private Ruleset customRuleSet;

	private const string CustomRuleSetKey = "Rule_Custom";

	[SerializeField]
	private RuleIndexFileEntry[] entries;

	public static GameRulesManager Instance => GameManager.DifficultyManager;

	public static Ruleset Current => Instance.mCurrent;

	private Ruleset mCurrent
	{
		get
		{
			if (SelectedRuleIndex == RuleIndex.Custom)
			{
				return CustomRuleSet;
			}
			RuleIndexFileEntry[] array = entries;
			for (int i = 0; i < array.Length; i++)
			{
				RuleIndexFileEntry ruleIndexFileEntry = array[i];
				if (ruleIndexFileEntry.index == SelectedRuleIndex)
				{
					return ruleIndexFileEntry.file.Data;
				}
			}
			return entries[0].file.Data;
		}
	}

	public static RuleIndex SelectedRuleIndex
	{
		get
		{
			if (SavesSystem.KeyExisits("GameRulesManager_RuleIndex"))
			{
				return SavesSystem.Load<RuleIndex>("GameRulesManager_RuleIndex");
			}
			return RuleIndex.Standard;
		}
		internal set
		{
			SavesSystem.Save("GameRulesManager_RuleIndex", value);
			NotifyRuleChanged();
		}
	}

	private Ruleset CustomRuleSet
	{
		get
		{
			if (customRuleSet == null)
			{
				ReloadCustomRuleSet();
			}
			return customRuleSet;
		}
	}

	public static event Action OnRuleChanged;

	public static void NotifyRuleChanged()
	{
		GameRulesManager.OnRuleChanged?.Invoke();
	}

	public static RuleIndex GetRuleIndexOfSaveSlot(int slot)
	{
		return SavesSystem.Load<RuleIndex>("GameRulesManager_RuleIndex", slot);
	}

	private void Awake()
	{
		SavesSystem.OnCollectSaveData += OnCollectSaveData;
		SavesSystem.OnSetFile += OnSetFile;
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= OnCollectSaveData;
		SavesSystem.OnSetFile -= OnSetFile;
	}

	private void OnSetFile()
	{
		ReloadCustomRuleSet();
	}

	private void ReloadCustomRuleSet()
	{
		if (SavesSystem.KeyExisits("Rule_Custom"))
		{
			customRuleSet = SavesSystem.Load<Ruleset>("Rule_Custom");
		}
		if (customRuleSet == null)
		{
			customRuleSet = new Ruleset();
			customRuleSet.displayNameKey = "Rule_Custom";
		}
	}

	private void OnCollectSaveData()
	{
		if (SelectedRuleIndex == RuleIndex.Custom && customRuleSet != null)
		{
			SavesSystem.Save("Rule_Custom", customRuleSet);
		}
	}

	internal static string GetRuleIndexDisplayNameOfSlot(int slotIndex)
	{
		RuleIndex ruleIndexOfSaveSlot = GetRuleIndexOfSaveSlot(slotIndex);
		return $"Rule_{ruleIndexOfSaveSlot}".ToPlainText();
	}
}
