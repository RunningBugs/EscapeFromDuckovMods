using ItemStatsSystem;
using UnityEngine;

namespace Duckov.Effects;

public class OnItemTreeChangedTrigger : EffectTrigger
{
	[SerializeField]
	private bool triggerFalseWheneverChanged = true;

	[SerializeField]
	private bool triggerInInventory;

	protected override void Awake()
	{
		base.Awake();
		base.Master.onItemTreeChanged += OnItemTreeChanged;
	}

	private void OnDestroy()
	{
		if (!(base.Master == null))
		{
			base.Master.onItemTreeChanged -= OnItemTreeChanged;
		}
	}

	private void OnItemTreeChanged(Effect effect, Item item)
	{
		Item characterItem = item.GetCharacterItem();
		if (triggerFalseWheneverChanged)
		{
			Trigger(positive: false);
		}
		if (characterItem == null)
		{
			if (!triggerFalseWheneverChanged)
			{
				Trigger(positive: false);
			}
		}
		else if (triggerInInventory)
		{
			Trigger();
		}
		else if (item.IsInCharacterSlot())
		{
			Trigger();
		}
		else if (!triggerFalseWheneverChanged)
		{
			Trigger(positive: false);
		}
	}
}
