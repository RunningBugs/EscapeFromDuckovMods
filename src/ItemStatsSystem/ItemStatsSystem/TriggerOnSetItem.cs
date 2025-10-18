namespace ItemStatsSystem;

public class TriggerOnSetItem : EffectTrigger
{
	protected override void OnMasterSetTargetItem(Effect effect, Item item)
	{
		Trigger();
	}
}
