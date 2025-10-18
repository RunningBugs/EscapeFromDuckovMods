using System;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;

public class UseToCreateItem : UsageBehavior
{
	[Serializable]
	private struct Entry
	{
		[ItemTypeID]
		[SerializeField]
		public int itemTypeID;
	}

	[SerializeField]
	private RandomContainer<Entry> entries;

	[LocalizationKey("Items")]
	public string descKey;

	[LocalizationKey("Default")]
	public string notificationKey;

	private bool running;

	public override DisplaySettingsData DisplaySettings => new DisplaySettingsData
	{
		display = true,
		description = descKey.ToPlainText()
	};

	public override bool CanBeUsed(Item item, object user)
	{
		if (!(user as CharacterMainControl))
		{
			return false;
		}
		return true;
	}

	protected override void OnUse(Item item, object user)
	{
		CharacterMainControl characterMainControl = user as CharacterMainControl;
		if ((bool)characterMainControl && entries.entries.Count != 0)
		{
			Generate(entries.GetRandom().itemTypeID, characterMainControl).Forget();
		}
	}

	private async UniTask Generate(int typeID, CharacterMainControl character)
	{
		if (running)
		{
			return;
		}
		running = true;
		Item item = await ItemAssetsCollection.InstantiateAsync(typeID);
		string displayName = item.DisplayName;
		bool num = character.PickupItem(item);
		NotificationText.Push(notificationKey.ToPlainText() + " " + displayName);
		if (!num && item != null)
		{
			if (item.ActiveAgent != null)
			{
				item.AgentUtilities.ReleaseActiveAgent();
			}
			PlayerStorage.Push(item);
		}
		running = false;
	}

	private void OnValidate()
	{
		entries.RefreshPercent();
	}
}
