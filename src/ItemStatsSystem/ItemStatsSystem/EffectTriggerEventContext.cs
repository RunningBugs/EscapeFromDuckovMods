namespace ItemStatsSystem;

public struct EffectTriggerEventContext
{
	public EffectTrigger source;

	public bool positive;

	public EffectTriggerEventContext(EffectTrigger source, bool positive)
	{
		this.source = source;
		this.positive = positive;
	}
}
