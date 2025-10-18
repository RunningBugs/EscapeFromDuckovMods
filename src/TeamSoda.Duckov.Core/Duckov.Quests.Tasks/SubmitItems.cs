using System;
using System.Collections.Generic;
using ItemStatsSystem;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.Quests.Tasks;

public class SubmitItems : Task
{
	[ItemTypeID]
	[SerializeField]
	private int itemTypeID;

	[Range(1f, 100f)]
	[SerializeField]
	private int requiredAmount = 1;

	[SerializeField]
	private int submittedAmount;

	private ItemMetaData? _cachedMeta;

	[SerializeField]
	private MapElementForTask mapElement;

	public int ItemTypeID => itemTypeID;

	private ItemMetaData CachedMeta
	{
		get
		{
			if (!_cachedMeta.HasValue || _cachedMeta.Value.id != itemTypeID)
			{
				_cachedMeta = ItemAssetsCollection.GetMetaData(itemTypeID);
			}
			return _cachedMeta.Value;
		}
	}

	private string descriptionFormatKey => "Task_SubmitItems";

	private string DescriptionFormat => descriptionFormatKey.ToPlainText();

	private string havingAmountFormatKey => "Task_SubmitItems_HavingAmount";

	private string HavingAmountFormat => havingAmountFormatKey.ToPlainText();

	public override string Description
	{
		get
		{
			string text = DescriptionFormat.Format(new
			{
				ItemDisplayName = CachedMeta.DisplayName,
				submittedAmount = submittedAmount,
				requiredAmount = requiredAmount
			});
			if (!IsFinished())
			{
				text = text + " " + HavingAmountFormat.Format(new
				{
					amount = ItemUtilities.GetItemCount(itemTypeID)
				});
			}
			return text;
		}
	}

	public override Sprite Icon => CachedMeta.icon;

	public override bool Interactable => true;

	public override bool PossibleValidInteraction => CheckItemEnough();

	public override string InteractText => "Task_SubmitItems_Interact".ToPlainText();

	public override bool NeedInspection
	{
		get
		{
			if (!IsFinished())
			{
				return CheckItemEnough();
			}
			return false;
		}
	}

	public static event Action<SubmitItems> onItemEnough;

	protected override void OnInit()
	{
		base.OnInit();
		PlayerStorage.OnPlayerStorageChange += OnPlayerStorageChanged;
		CharacterMainControl.OnMainCharacterInventoryChangedEvent = (Action<CharacterMainControl, Inventory, int>)Delegate.Combine(CharacterMainControl.OnMainCharacterInventoryChangedEvent, new Action<CharacterMainControl, Inventory, int>(OnMainCharacterInventoryChanged));
		CheckItemEnough();
	}

	private void OnDestroy()
	{
		PlayerStorage.OnPlayerStorageChange -= OnPlayerStorageChanged;
		CharacterMainControl.OnMainCharacterInventoryChangedEvent = (Action<CharacterMainControl, Inventory, int>)Delegate.Remove(CharacterMainControl.OnMainCharacterInventoryChangedEvent, new Action<CharacterMainControl, Inventory, int>(OnMainCharacterInventoryChanged));
	}

	private void OnPlayerStorageChanged(PlayerStorage storage, Inventory inventory, int index)
	{
		if (!base.Master.Complete)
		{
			Item itemAt = inventory.GetItemAt(index);
			if (!(itemAt == null) && itemAt.TypeID == itemTypeID)
			{
				CheckItemEnough();
			}
		}
	}

	private void OnMainCharacterInventoryChanged(CharacterMainControl control, Inventory inventory, int index)
	{
		if (!base.Master.Complete)
		{
			Item itemAt = inventory.GetItemAt(index);
			if (!(itemAt == null) && itemAt.TypeID == itemTypeID)
			{
				CheckItemEnough();
			}
		}
	}

	private bool CheckItemEnough()
	{
		if (ItemUtilities.GetItemCount(itemTypeID) >= requiredAmount)
		{
			SubmitItems.onItemEnough?.Invoke(this);
			SetMapElementVisable(visable: false);
			return true;
		}
		SetMapElementVisable(visable: true);
		return false;
	}

	private void SetMapElementVisable(bool visable)
	{
		if ((bool)mapElement)
		{
			if (visable)
			{
				mapElement.name = base.Master.DisplayName;
			}
			mapElement.SetVisibility(visable);
		}
	}

	public void Submit(Item item)
	{
		if (item.TypeID != itemTypeID)
		{
			Debug.LogError("提交的物品类型与需求不一致。");
			return;
		}
		int num = requiredAmount - submittedAmount;
		if (num <= 0)
		{
			Debug.LogError("目标已达成，不需要继续提交物品");
			return;
		}
		int num2 = submittedAmount;
		if (num < item.StackCount)
		{
			item.StackCount -= num;
			submittedAmount += num;
		}
		else
		{
			foreach (Item allChild in item.GetAllChildren(includingGrandChildren: false, excludeSelf: true))
			{
				allChild.Detach();
				if (!ItemUtilities.SendToPlayerCharacter(allChild))
				{
					allChild.Drop(CharacterMainControl.Main, createRigidbody: true);
				}
			}
			item.Detach();
			item.DestroyTree();
			submittedAmount += item.StackCount;
		}
		Debug.Log("submission done");
		if (num2 != submittedAmount)
		{
			base.Master.NotifyTaskFinished(this);
		}
		ReportStatusChanged();
	}

	protected override bool CheckFinished()
	{
		return submittedAmount >= requiredAmount;
	}

	public override object GenerateSaveData()
	{
		return submittedAmount;
	}

	public override void SetupSaveData(object data)
	{
		submittedAmount = (int)data;
	}

	public override void Interact()
	{
		if (base.Master == null)
		{
			return;
		}
		List<Item> list = ItemUtilities.FindAllBelongsToPlayer((Item e) => e != null && e.TypeID == itemTypeID);
		for (int num = 0; num < list.Count; num++)
		{
			Item item = list[num];
			Submit(item);
			if (IsFinished())
			{
				break;
			}
		}
	}
}
