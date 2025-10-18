using Duckov.Buildings;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.Quests.Tasks;

public class QuestTask_ConstructBuilding : Task
{
	[SerializeField]
	private string buildingID;

	[LocalizationKey("Default")]
	private string descriptionFormatKey => "Task_ConstructBuilding";

	private string DescriptionFormat => descriptionFormatKey.ToPlainText();

	public override string Description => DescriptionFormat.Format(new
	{
		BuildingName = Building.GetDisplayName(buildingID)
	});

	public override object GenerateSaveData()
	{
		return null;
	}

	protected override bool CheckFinished()
	{
		return BuildingManager.Any(buildingID);
	}

	public override void SetupSaveData(object data)
	{
	}
}
