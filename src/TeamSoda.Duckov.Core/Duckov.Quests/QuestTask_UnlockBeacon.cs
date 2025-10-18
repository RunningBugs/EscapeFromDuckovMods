using Duckvo.Beacons;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.Quests;

public class QuestTask_UnlockBeacon : Task
{
	[SerializeField]
	private string beaconID;

	[SerializeField]
	private int beaconIndex;

	[LocalizationKey("Default")]
	private string DescriptionKey
	{
		get
		{
			return "Task_Beacon_" + beaconID;
		}
		set
		{
		}
	}

	public override string Description => DescriptionKey.ToPlainText();

	public override object GenerateSaveData()
	{
		return 0;
	}

	public override void SetupSaveData(object data)
	{
	}

	protected override bool CheckFinished()
	{
		return BeaconManager.GetBeaconUnlocked(beaconID, beaconIndex);
	}
}
