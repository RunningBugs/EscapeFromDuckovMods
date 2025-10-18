using Saves;

namespace Duckov.Quests.Conditions;

public class RequireHasFished : Condition
{
	public override bool Evaluate()
	{
		return GetHasFished();
	}

	public static void SetHasFished()
	{
		SavesSystem.Save("HasFished", value: true);
	}

	public static bool GetHasFished()
	{
		return SavesSystem.Load<bool>("HasFished");
	}
}
