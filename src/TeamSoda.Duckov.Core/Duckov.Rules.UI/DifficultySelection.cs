using System;
using Cysharp.Threading.Tasks;
using Duckov.Scenes;
using Duckov.UI.Animations;
using Duckov.Utilities;
using Saves;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.Rules.UI;

public class DifficultySelection : MonoBehaviour
{
	[Serializable]
	public struct SettingEntry
	{
		public RuleIndex ruleIndex;

		public Sprite icon;

		public bool recommended;

		[LocalizationKey("Default")]
		private string TitleKey
		{
			get
			{
				return $"Rule_{ruleIndex}";
			}
			set
			{
			}
		}

		public string Title => TitleKey.ToPlainText();

		[LocalizationKey("Default")]
		private string DescriptionKey
		{
			get
			{
				return $"Rule_{ruleIndex}_Desc";
			}
			set
			{
			}
		}

		public string Description => DescriptionKey.ToPlainText();
	}

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private TextMeshProUGUI textDescription;

	[SerializeField]
	[LocalizationKey("Default")]
	private string description_PlaceHolderKey = "DifficultySelection_Desc_PlaceHolder";

	[SerializeField]
	private Button confirmButton;

	[SerializeField]
	private FadeGroup customPanel;

	[SerializeField]
	private DifficultySelection_Entry entryTemplate;

	[SerializeField]
	private GameObject achievementDisabledIndicator;

	[SerializeField]
	private GameObject selectedCustomDifficultyBefore;

	private PrefabPool<DifficultySelection_Entry> _entryPool;

	[SerializeField]
	private SettingEntry[] displaySettings;

	private bool confirmed;

	private PrefabPool<DifficultySelection_Entry> EntryPool
	{
		get
		{
			if (_entryPool == null)
			{
				_entryPool = new PrefabPool<DifficultySelection_Entry>(entryTemplate);
			}
			return _entryPool;
		}
	}

	public static bool CustomDifficultyMarker
	{
		get
		{
			return SavesSystem.Load<bool>("CustomDifficultyMarker");
		}
		set
		{
			SavesSystem.Save("CustomDifficultyMarker", value);
		}
	}

	public RuleIndex SelectedRuleIndex
	{
		get
		{
			if (SelectedEntry == null)
			{
				return RuleIndex.Standard;
			}
			return SelectedEntry.Setting.ruleIndex;
		}
	}

	public DifficultySelection_Entry SelectedEntry { get; private set; }

	public DifficultySelection_Entry HoveringEntry { get; private set; }

	private void Awake()
	{
		confirmButton.onClick.AddListener(OnConfirmButtonClicked);
	}

	private void OnConfirmButtonClicked()
	{
		confirmed = true;
	}

	public async UniTask Execute()
	{
		EntryPool.ReleaseAll();
		fadeGroup.Show();
		SettingEntry[] array = displaySettings;
		foreach (SettingEntry setting in array)
		{
			if (CheckShouldDisplay(setting))
			{
				DifficultySelection_Entry difficultySelection_Entry = EntryPool.Get();
				bool locked = !CheckUnlocked(setting);
				difficultySelection_Entry.Setup(this, setting, locked);
			}
		}
		foreach (DifficultySelection_Entry activeEntry in EntryPool.ActiveEntries)
		{
			if (activeEntry.Setting.ruleIndex == GameRulesManager.SelectedRuleIndex)
			{
				NotifySelected(activeEntry);
				break;
			}
		}
		if ((GameRulesManager.SelectedRuleIndex = await WaitForConfirmation()) == RuleIndex.Custom)
		{
			CustomDifficultyMarker = true;
		}
		await fadeGroup.HideAndReturnTask();
	}

	private bool CheckUnlocked(SettingEntry setting)
	{
		bool flag = !MultiSceneCore.GetVisited("Base");
		switch (setting.ruleIndex)
		{
		case RuleIndex.Standard:
		case RuleIndex.Easy:
		case RuleIndex.ExtraEasy:
		case RuleIndex.StandardChallenge:
		case RuleIndex.Hard:
		case RuleIndex.ExtraHard:
			if (flag)
			{
				return true;
			}
			if (GameRulesManager.SelectedRuleIndex == RuleIndex.Custom)
			{
				return false;
			}
			if (GameRulesManager.SelectedRuleIndex == RuleIndex.Rage)
			{
				return false;
			}
			return true;
		case RuleIndex.Rage:
			return GetRageUnlocked(flag);
		case RuleIndex.Custom:
			if (flag)
			{
				return true;
			}
			return GameRulesManager.SelectedRuleIndex == RuleIndex.Custom;
		default:
			return false;
		}
	}

	public static void UnlockRage()
	{
		SavesSystem.SaveGlobal("Difficulty/RageUnlocked", value: true);
	}

	public bool GetRageUnlocked(bool isFirstSelect)
	{
		if (!SavesSystem.LoadGlobal("Difficulty/RageUnlocked", defaultValue: false))
		{
			return false;
		}
		if (isFirstSelect)
		{
			return true;
		}
		if (GameRulesManager.SelectedRuleIndex == RuleIndex.Custom)
		{
			return false;
		}
		if (GameRulesManager.SelectedRuleIndex != RuleIndex.Rage)
		{
			return false;
		}
		return true;
	}

	private bool CheckShouldDisplay(SettingEntry setting)
	{
		return true;
	}

	private async UniTask<RuleIndex> WaitForConfirmation()
	{
		confirmed = false;
		while (!confirmed)
		{
			await UniTask.Yield();
		}
		return SelectedRuleIndex;
	}

	internal void NotifySelected(DifficultySelection_Entry entry)
	{
		SelectedEntry = entry;
		GameRulesManager.SelectedRuleIndex = SelectedRuleIndex;
		foreach (DifficultySelection_Entry activeEntry in EntryPool.ActiveEntries)
		{
			if (!(activeEntry == null))
			{
				activeEntry.Refresh();
			}
		}
		RefreshDescription();
		if (SelectedRuleIndex == RuleIndex.Custom)
		{
			ShowCustomRuleSetupPanel();
		}
		bool flag = SelectedRuleIndex == RuleIndex.Custom;
		achievementDisabledIndicator.SetActive(flag || CustomDifficultyMarker);
		selectedCustomDifficultyBefore.SetActive(CustomDifficultyMarker);
	}

	private void ShowCustomRuleSetupPanel()
	{
		customPanel?.Show();
	}

	internal void NotifyEntryPointerEnter(DifficultySelection_Entry entry)
	{
		HoveringEntry = entry;
		RefreshDescription();
	}

	internal void NotifyEntryPointerExit(DifficultySelection_Entry entry)
	{
		if (HoveringEntry == entry)
		{
			HoveringEntry = null;
			RefreshDescription();
		}
	}

	private void RefreshDescription()
	{
		string text = ((!(SelectedEntry != null)) ? description_PlaceHolderKey.ToPlainText() : SelectedEntry.Setting.Description);
		textDescription.text = text;
	}

	internal void SkipHide()
	{
		if (fadeGroup != null)
		{
			fadeGroup.SkipHide();
		}
	}
}
