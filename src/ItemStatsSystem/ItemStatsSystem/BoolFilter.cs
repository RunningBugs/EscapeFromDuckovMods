namespace ItemStatsSystem;

[MenuPath("Debug/Bool")]
public class BoolFilter : EffectFilter
{
	public bool value;

	public override string DisplayName => "根据 Bool 值";

	protected override bool OnEvaluate(EffectTriggerEventContext context)
	{
		return value;
	}
}
