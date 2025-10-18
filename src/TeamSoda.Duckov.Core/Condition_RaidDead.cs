using Duckov.Quests;

public class Condition_RaidDead : Condition
{
	public override bool Evaluate()
	{
		return RaidUtilities.CurrentRaid.dead;
	}
}
