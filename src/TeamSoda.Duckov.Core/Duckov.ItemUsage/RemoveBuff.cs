using ItemStatsSystem;
using UnityEngine.Serialization;

namespace Duckov.ItemUsage;

public class RemoveBuff : UsageBehavior
{
	public int buffID;

	[FormerlySerializedAs("removeOneLayer")]
	public bool litmitRemoveLayerCount;

	public int removeLayerCount = 2;

	public bool useDurability;

	public int durabilityUsage = 1;

	public override bool CanBeUsed(Item item, object user)
	{
		if (!item)
		{
			return false;
		}
		if (useDurability && item.Durability < (float)durabilityUsage)
		{
			return false;
		}
		CharacterMainControl characterMainControl = user as CharacterMainControl;
		if (characterMainControl == null)
		{
			return false;
		}
		return characterMainControl.HasBuff(buffID);
	}

	protected override void OnUse(Item item, object user)
	{
		CharacterMainControl characterMainControl = user as CharacterMainControl;
		if (!(characterMainControl == null))
		{
			if (!litmitRemoveLayerCount)
			{
				characterMainControl.RemoveBuff(buffID, removeOneLayer: false);
			}
			for (int i = 0; i < removeLayerCount; i++)
			{
				characterMainControl.RemoveBuff(buffID, litmitRemoveLayerCount);
			}
			if (useDurability && item.Durability > 0f)
			{
				item.Durability -= durabilityUsage;
			}
		}
	}
}
