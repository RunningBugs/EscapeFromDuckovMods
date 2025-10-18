using ItemStatsSystem;

public class CostStaminaAction : EffectAction
{
	public float staminaCost;

	private CharacterMainControl MainControl => base.Master?.Item?.GetCharacterMainControl();

	protected override void OnTriggered(bool positive)
	{
		if ((bool)MainControl)
		{
			MainControl.UseStamina(staminaCost);
		}
	}
}
