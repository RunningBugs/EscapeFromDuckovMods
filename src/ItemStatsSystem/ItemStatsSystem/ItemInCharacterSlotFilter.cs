namespace ItemStatsSystem;

public class ItemInCharacterSlotFilter : EffectFilter
{
	protected override bool OnEvaluate(EffectTriggerEventContext context)
	{
		if (base.Master == null)
		{
			return false;
		}
		if (base.Master.Item == null)
		{
			return false;
		}
		return base.Master.Item.IsInCharacterSlot();
	}
}
