namespace ItemStatsSystem;

[MenuPath("General/Update")]
public class UpdateTrigger : EffectTrigger
{
	public override string DisplayName => "Update";

	private void Update()
	{
		Trigger();
	}
}
