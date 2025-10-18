using Duckov.Buildings;
using Duckov.Quests;

public class Conditon_BuildingConstructed : Condition
{
	public string buildingID;

	public bool not;

	public override bool Evaluate()
	{
		bool flag = BuildingManager.Any(buildingID);
		if (not)
		{
			flag = !flag;
		}
		return flag;
	}
}
