using Duckov.Buffs;
using ItemStatsSystem;

public class AddBuffAction : EffectAction
{
	public Buff buffPfb;

	private CharacterMainControl MainControl => base.Master?.Item?.GetCharacterMainControl();

	protected override void OnTriggered(bool positive)
	{
		if ((bool)MainControl)
		{
			MainControl.AddBuff(buffPfb, MainControl);
		}
	}
}
