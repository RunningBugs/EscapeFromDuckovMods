using ItemStatsSystem;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.Quests.Tasks;

public class QuestTask_UseItem : Task
{
	[SerializeField]
	private int requireAmount = 1;

	[ItemTypeID]
	[SerializeField]
	private int itemTypeID;

	[SerializeField]
	private bool resetOnLevelInitialized;

	[SerializeField]
	private int amount;

	private ItemMetaData? _cachedMeta;

	private ItemMetaData CachedMeta
	{
		get
		{
			if (!_cachedMeta.HasValue)
			{
				_cachedMeta = ItemAssetsCollection.GetMetaData(itemTypeID);
			}
			return _cachedMeta.Value;
		}
	}

	private string descriptionFormatKey => "Task_UseItem";

	private string DescriptionFormat => descriptionFormatKey.ToPlainText();

	private string ItemDisplayName => CachedMeta.DisplayName;

	public override string Description => DescriptionFormat.Format(new { ItemDisplayName, amount, requireAmount });

	public override Sprite Icon => CachedMeta.icon;

	private void OnEnable()
	{
		Item.onUseStatic += OnItemUsed;
		LevelManager.OnLevelInitialized += OnLevelInitialized;
	}

	private void OnDisable()
	{
		Item.onUseStatic -= OnItemUsed;
		LevelManager.OnLevelInitialized -= OnLevelInitialized;
	}

	private void OnLevelInitialized()
	{
		if (resetOnLevelInitialized)
		{
			amount = 0;
		}
	}

	private void OnItemUsed(Item item, object user)
	{
		if ((bool)LevelManager.Instance && user as CharacterMainControl == LevelManager.Instance.MainCharacter && item.TypeID == itemTypeID)
		{
			AddCount();
		}
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
