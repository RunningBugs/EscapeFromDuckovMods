using System.Collections.Generic;
using Duckov.Scenes;
using Duckov.Utilities;
using Eflatun.SceneReference;
using ItemStatsSystem;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.Quests.Tasks;

public class QuestTask_KillCount : Task
{
	[SerializeField]
	private int requireAmount = 1;

	[SerializeField]
	private bool resetOnLevelInitialized;

	[SerializeField]
	private int amount;

	[SerializeField]
	private bool withWeapon;

	[SerializeField]
	[ItemTypeID]
	private int weaponTypeID;

	[SerializeField]
	private bool requireHeadShot;

	[SerializeField]
	private bool withoutHeadShot;

	[SerializeField]
	private bool requireBuff;

	[SerializeField]
	private int requireBuffID;

	[SerializeField]
	private CharacterRandomPreset requireEnemyType;

	[SceneID]
	[SerializeField]
	private string requireSceneID;

	[LocalizationKey("TasksAndRewards")]
	private string defaultEnemyNameKey
	{
		get
		{
			return "Task_Desc_AnyEnemy";
		}
		set
		{
		}
	}

	[LocalizationKey("TasksAndRewards")]
	private string defaultWeaponNameKey
	{
		get
		{
			return "Task_Desc_AnyWeapon";
		}
		set
		{
		}
	}

	private string weaponName
	{
		get
		{
			if (withWeapon)
			{
				return ItemAssetsCollection.GetMetaData(weaponTypeID).DisplayName;
			}
			return defaultWeaponNameKey.ToPlainText();
		}
	}

	private string enemyName
	{
		get
		{
			if (requireEnemyType == null)
			{
				return defaultEnemyNameKey.ToPlainText();
			}
			return requireEnemyType.DisplayName;
		}
	}

	[LocalizationKey("TasksAndRewards")]
	private string descriptionFormatKey
	{
		get
		{
			return "Task_KillCount";
		}
		set
		{
		}
	}

	[LocalizationKey("TasksAndRewards")]
	private string withWeaponDescriptionFormatKey
	{
		get
		{
			return "Task_Desc_WithWeapon";
		}
		set
		{
		}
	}

	[LocalizationKey("TasksAndRewards")]
	private string requireSceneDescriptionFormatKey
	{
		get
		{
			return "Task_Desc_RequireScene";
		}
		set
		{
		}
	}

	[LocalizationKey("TasksAndRewards")]
	private string RequireHeadShotDescriptionKey
	{
		get
		{
			return "Task_Desc_RequireHeadShot";
		}
		set
		{
		}
	}

	[LocalizationKey("TasksAndRewards")]
	private string WithoutHeadShotDescriptionKey
	{
		get
		{
			return "Task_Desc_WithoutHeadShot";
		}
		set
		{
		}
	}

	[LocalizationKey("TasksAndRewards")]
	private string RequireBuffDescriptionFormatKey
	{
		get
		{
			return "Task_Desc_WithBuff";
		}
		set
		{
		}
	}

	private string DescriptionFormat => descriptionFormatKey.ToPlainText();

	public override string[] ExtraDescriptsions
	{
		get
		{
			List<string> list = new List<string>();
			if (withWeapon)
			{
				list.Add(WithWeaponDescription);
			}
			if (!string.IsNullOrEmpty(requireSceneID))
			{
				list.Add(RequireSceneDescription);
			}
			if (requireHeadShot)
			{
				list.Add(RequireHeadShotDescription);
			}
			if (withoutHeadShot)
			{
				list.Add(WithoutHeadShotDescription);
			}
			if (requireBuff)
			{
				list.Add(RequireBuffDescription);
			}
			return list.ToArray();
		}
	}

	private string WithWeaponDescription => withWeaponDescriptionFormatKey.ToPlainText().Format(new { weaponName });

	private string RequireSceneDescription => requireSceneDescriptionFormatKey.ToPlainText().Format(new { requireSceneName });

	private string RequireHeadShotDescription => RequireHeadShotDescriptionKey.ToPlainText();

	private string WithoutHeadShotDescription => WithoutHeadShotDescriptionKey.ToPlainText();

	private string RequireBuffDescription
	{
		get
		{
			string buffDisplayName = GameplayDataSettings.Buffs.GetBuffDisplayName(requireBuffID);
			return RequireBuffDescriptionFormatKey.ToPlainText().Format(new
			{
				buffName = buffDisplayName
			});
		}
	}

	public override string Description => DescriptionFormat.Format(new { weaponName, enemyName, requireAmount, amount, requireSceneName });

	public SceneInfoEntry RequireSceneInfo => SceneInfoCollection.GetSceneInfo(requireSceneID);

	public SceneReference RequireScene => RequireSceneInfo?.SceneReference;

	public string requireSceneName
	{
		get
		{
			if (string.IsNullOrEmpty(requireSceneID))
			{
				return "Task_Desc_AnyScene".ToPlainText();
			}
			return RequireSceneInfo.DisplayName;
		}
	}

	public bool SceneRequirementSatisfied
	{
		get
		{
			if (string.IsNullOrEmpty(requireSceneID))
			{
				return true;
			}
			SceneReference requireScene = RequireScene;
			if (requireScene == null)
			{
				return true;
			}
			if (requireScene.UnsafeReason == SceneReferenceUnsafeReason.Empty)
			{
				return true;
			}
			if (requireScene.UnsafeReason == SceneReferenceUnsafeReason.None)
			{
				return requireScene.LoadedScene.isLoaded;
			}
			return true;
		}
	}

	private void OnEnable()
	{
		Health.OnDead += Health_OnDead;
		LevelManager.OnLevelInitialized += OnLevelInitialized;
	}

	private void OnDisable()
	{
		Health.OnDead -= Health_OnDead;
		LevelManager.OnLevelInitialized -= OnLevelInitialized;
	}

	private void OnLevelInitialized()
	{
		if (resetOnLevelInitialized)
		{
			amount = 0;
		}
	}

	private void Health_OnDead(Health health, DamageInfo info)
	{
		if (health.team == Teams.player)
		{
			return;
		}
		bool flag = false;
		CharacterMainControl fromCharacter = info.fromCharacter;
		if (fromCharacter != null && info.fromCharacter.IsMainCharacter())
		{
			flag = true;
		}
		if (!flag || (withWeapon && info.fromWeaponItemID != weaponTypeID) || !SceneRequirementSatisfied || (requireHeadShot && info.crit <= 0) || (withoutHeadShot && info.crit > 0) || (requireBuff && !fromCharacter.HasBuff(requireBuffID)))
		{
			return;
		}
		if (requireEnemyType != null)
		{
			CharacterMainControl characterMainControl = health.TryGetCharacter();
			if (characterMainControl == null)
			{
				return;
			}
			CharacterRandomPreset characterPreset = characterMainControl.characterPreset;
			if (characterPreset == null || characterPreset.nameKey != requireEnemyType.nameKey)
			{
				return;
			}
		}
		AddCount();
	}

	private void AddCount()
	{
		if (amount < requireAmount)
		{
			amount++;
			ReportStatusChanged();
		}
	}

	public override object GenerateSaveData()
	{
		return amount;
	}

	protected override bool CheckFinished()
	{
		return amount >= requireAmount;
	}

	public override void SetupSaveData(object data)
	{
		if (data is int num)
		{
			amount = num;
		}
	}
}
