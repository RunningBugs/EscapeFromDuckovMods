using UnityEngine;

namespace ItemStatsSystem;

public class ItemUsedTrigger : EffectTrigger
{
	public override string DisplayName => "当物品被使用";

	private void OnEnable()
	{
		if (base.Master != null && base.Master.Item != null)
		{
			base.Master.Item.onUse += OnItemUsed;
		}
		else
		{
			Debug.LogError("因为找不到对象，未能注册物品使用事件。");
		}
	}

	private new void OnDisable()
	{
		if (!(base.Master == null) && !(base.Master.Item == null))
		{
			base.Master.Item.onUse -= OnItemUsed;
		}
	}

	private void OnItemUsed(Item item, object user)
	{
		Trigger();
	}
}
