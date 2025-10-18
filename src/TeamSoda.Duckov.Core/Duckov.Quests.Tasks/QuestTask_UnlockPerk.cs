using System;
using Duckov.PerkTrees;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.Quests.Tasks;

public class QuestTask_UnlockPerk : Task
{
	[SerializeField]
	private string perkTreeID;

	[SerializeField]
	private string perkObjectName;

	private Perk perk;

	[NonSerialized]
	private bool unlocked;

	private string descriptionFormatKey = "Task_UnlockPerk";

	private string DescriptionFormat => descriptionFormatKey.ToPlainText();

	private string PerkDisplayName
	{
		get
		{
			if (perk == null)
			{
				BindPerk();
			}
			if (perk == null)
			{
				return perkObjectName.ToPlainText();
			}
			return perk.DisplayName;
		}
	}

	public override string Description => DescriptionFormat.Format(new { PerkDisplayName });

	public override Sprite Icon
	{
		get
		{
			if (perk != null)
			{
				return perk.Icon;
			}
			return null;
		}
	}

	protected override void OnInit()
	{
		if (LevelManager.LevelInited)
		{
			BindPerk();
		}
		else
		{
			LevelManager.OnLevelInitialized += OnLevelInitialized;
		}
	}

	private bool BindPerk()
	{
		if ((bool)perk)
		{
			if (!unlocked && perk.Unlocked)
			{
				OnPerkUnlockStateChanged(perk, _unlocked: true);
			}
			return false;
		}
		PerkTree perkTree = PerkTreeManager.GetPerkTree(perkTreeID);
		if ((bool)perkTree)
		{
			foreach (Perk perk in perkTree.perks)
			{
				if (perk.gameObject.name == perkObjectName)
				{
					this.perk = perk;
					if (this.perk.Unlocked)
					{
						OnPerkUnlockStateChanged(this.perk, _unlocked: true);
					}
					this.perk.onUnlockStateChanged += OnPerkUnlockStateChanged;
					return true;
				}
			}
		}
		else
		{
			Debug.LogError("PerkTree Not Found " + perkTreeID, base.gameObject);
		}
		Debug.LogError("Perk Not Found: " + perkTreeID + "/" + perkObjectName, base.gameObject);
		return false;
	}

	private void OnPerkUnlockStateChanged(Perk _perk, bool _unlocked)
	{
		if (!base.Master.Complete && _unlocked)
		{
			unlocked = true;
			ReportStatusChanged();
		}
	}

	private void OnDestroy()
	{
		if ((bool)perk)
		{
			perk.onUnlockStateChanged -= OnPerkUnlockStateChanged;
		}
		LevelManager.OnLevelInitialized -= OnLevelInitialized;
	}

	private void OnLevelInitialized()
	{
		BindPerk();
	}

	public override object GenerateSaveData()
	{
		return unlocked;
	}

	protected override bool CheckFinished()
	{
		return unlocked;
	}

	public override void SetupSaveData(object data)
	{
		if (data is bool flag)
		{
			unlocked = flag;
		}
	}
}
