using ItemStatsSystem;

public class RemoveBuffAction : EffectAction
{
	public int buffID;

	public bool removeOneLayer;

	private CharacterMainControl MainControl => base.Master?.Item?.GetCharacterMainControl();

	protected override void OnTriggered(bool positive)
	{
		if ((bool)MainControl)
		{
			MainControl.RemoveBuff(buffID, removeOneLayer);
		}
	}
}
