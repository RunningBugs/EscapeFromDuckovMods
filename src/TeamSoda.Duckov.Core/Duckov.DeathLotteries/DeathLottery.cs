using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.Economy;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Data;
using ItemStatsSystem.Items;
using Saves;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.DeathLotteries;

public class DeathLottery : MonoBehaviour
{
	[Serializable]
	private struct dummyItemEntry
	{
		[ItemTypeID]
		public int typeID;
	}

	[Serializable]
	public struct OptionalCosts
	{
		[SerializeField]
		public Cost costA;

		[SerializeField]
		public bool useCostB;

		[SerializeField]
		public Cost costB;
	}

	[Serializable]
	public struct Status
	{
		public bool valid;

		public uint deadCharacterToken;

		public List<int> selectedItems;

		public List<ItemTreeData> candidates;

		public int SelectedCount => selectedItems.Count;
	}

	public const int MaxCandidateCount = 8;

	[SerializeField]
	[LocalizationKey("Default")]
	private string selectNotificationFormatKey = "DeathLottery_SelectNotification";

	[SerializeField]
	private Tag[] requireTags;

	[SerializeField]
	private Tag[] excludeTags;

	[SerializeField]
	private RandomContainer<dummyItemEntry> dummyItems;

	[SerializeField]
	private OptionalCosts[] costs;

	private Status status;

	private List<Item> itemInstances = new List<Item>();

	private bool loading;

	public int MaxChances => costs.Length;

	public static uint CurrentDeadCharacterToken => SavesSystem.Load<uint>("DeadCharacterToken");

	private string SelectNotificationFormat => selectNotificationFormatKey.ToPlainText();

	public OptionalCosts[] Costs => costs;

	public List<Item> ItemInstances => itemInstances;

	public Status CurrentStatus
	{
		get
		{
			if (loading)
			{
				return default(Status);
			}
			if (!status.valid)
			{
				return default(Status);
			}
			return status;
		}
	}

	public int RemainingChances => 0;

	public bool Loading => loading;

	public static event Action<DeathLottery> OnRequestUI;

	public void RequestUI()
	{
		DeathLottery.OnRequestUI?.Invoke(this);
	}

	private async UniTask Load()
	{
		loading = true;
		status = SavesSystem.Load<Status>("DeathLottery/status");
		if (!status.valid)
		{
			await CreateNewStatus();
		}
		else if (status.deadCharacterToken == CurrentDeadCharacterToken)
		{
			await LoadItemInstances();
		}
		else
		{
			await CreateNewStatus();
		}
		loading = false;
	}

	private async UniTask LoadItemInstances()
	{
		ClearItemInstances();
		foreach (ItemTreeData candidate in status.candidates)
		{
			if (candidate != null)
			{
				Item item = await ItemTreeData.InstantiateAsync(candidate);
				itemInstances.Add(item);
				item.transform.SetParent(base.transform);
			}
		}
	}

	private void ClearItemInstances()
	{
		for (int i = 0; i < itemInstances.Count; i++)
		{
			Item item = itemInstances[i];
			if (!(item.ParentItem != null))
			{
				item.DestroyTree();
			}
		}
		itemInstances.Clear();
	}

	[ContextMenu("ForceCreateNewStatus")]
	private void ForceCreateNewStatus()
	{
		if (!Loading)
		{
			ForceCreateNewStatusTask().Forget();
		}
	}

	private async UniTask ForceCreateNewStatusTask()
	{
		loading = true;
		await CreateNewStatus();
		loading = false;
	}

	private async UniTask CreateNewStatus()
	{
		List<ItemTreeData> candidates = new List<ItemTreeData>();
		Item deadCharacter = await ItemSavesUtilities.LoadLastDeadCharacterItem();
		if (deadCharacter == null)
		{
			return;
		}
		List<Item> obj = await SelectCandidates(deadCharacter);
		deadCharacter.DestroyTree();
		ClearItemInstances();
		foreach (Item item2 in obj)
		{
			item2.transform.SetParent(base.transform);
			itemInstances.Add(item2);
			ItemTreeData item = ItemTreeData.FromItem(item2);
			candidates.Add(item);
		}
		status = new Status
		{
			valid = true,
			deadCharacterToken = CurrentDeadCharacterToken,
			selectedItems = new List<int>(),
			candidates = candidates
		};
	}

	private void Awake()
	{
		SavesSystem.OnCollectSaveData += Save;
	}

	private void Start()
	{
		Load().Forget();
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= Save;
	}

	private void Save()
	{
		if (!loading)
		{
			SavesSystem.Save("DeathLottery/status", status);
		}
	}

	private async UniTask<List<Item>> SelectCandidates(Item deadCharacter)
	{
		List<Item> candidates = new List<Item>();
		if (deadCharacter.Slots != null)
		{
			foreach (Slot slot in deadCharacter.Slots)
			{
				if (slot == null)
				{
					continue;
				}
				Item content = slot.Content;
				if (content == null || !CanBeACandidate(content))
				{
					continue;
				}
				content.Detach();
				candidates.Add(content);
				if (candidates.Count < 8)
				{
					continue;
				}
				goto IL_0112;
			}
		}
		List<Item> list = new List<Item>();
		foreach (Item item2 in list)
		{
			item2.Detach();
		}
		int amount = 8 - candidates.Count;
		Item[] randomSubSet = list.GetRandomSubSet(amount);
		if (randomSubSet != null)
		{
			candidates.AddRange(randomSubSet);
		}
		goto IL_0112;
		IL_0112:
		int maxAttempts = 100;
		int attempts = 0;
		while (candidates.Count < 8)
		{
			attempts++;
			if (attempts > maxAttempts)
			{
				Debug.LogError("无法生成candidate");
				break;
			}
			Item item = await ItemAssetsCollection.InstantiateAsync(dummyItems.GetRandom().typeID);
			if (!(item == null))
			{
				candidates.Add(item);
			}
		}
		candidates.RandomizeOrder();
		return candidates;
	}

	private bool CanBeACandidate(Item item)
	{
		if (item == null)
		{
			return false;
		}
		Tag[] array = excludeTags;
		foreach (Tag item2 in array)
		{
			if (item.Tags.Contains(item2))
			{
				return false;
			}
		}
		array = requireTags;
		foreach (Tag item3 in array)
		{
			if (item.Tags.Contains(item3))
			{
				return true;
			}
		}
		return false;
	}

	public async UniTask<bool> Select(int index, Cost payWhenSucceed)
	{
		if (loading)
		{
			return false;
		}
		if (!status.valid)
		{
			return false;
		}
		if (status.SelectedCount >= MaxChances)
		{
			return false;
		}
		if (status.selectedItems.Contains(index))
		{
			return false;
		}
		Item instance = await ItemTreeData.InstantiateAsync(status.candidates[index]);
		if (instance == null)
		{
			return false;
		}
		if (!payWhenSucceed.Enough)
		{
			return false;
		}
		if ((bool)instance.GetComponent<ItemSetting_Gun>())
		{
			Item item = await ItemUtilities.GenerateBullet(instance);
			if (item != null)
			{
				SendToPlayer(item);
			}
		}
		SendToPlayer(instance);
		status.selectedItems.Add(index);
		payWhenSucceed.Pay();
		NotificationText.Push(SelectNotificationFormat.Format(new
		{
			itemName = instance.DisplayName
		}));
		return true;
		static void SendToPlayer(Item item2)
		{
			if (!(item2 == null) && !ItemUtilities.SendToPlayerCharacter(item2))
			{
				ItemUtilities.SendToPlayerStorage(item2);
			}
		}
	}

	internal OptionalCosts GetCost()
	{
		if (!status.valid)
		{
			return default(OptionalCosts);
		}
		if (status.SelectedCount >= Costs.Length)
		{
			return default(OptionalCosts);
		}
		return Costs[status.SelectedCount];
	}
}
