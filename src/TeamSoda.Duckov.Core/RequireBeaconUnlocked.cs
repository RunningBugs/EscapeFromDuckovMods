using Duckov.Quests;
using Duckov.Scenes;
using Duckvo.Beacons;
using UnityEngine;

public class RequireBeaconUnlocked : Condition
{
	[SerializeField]
	[SceneID]
	private string beaconID;

	[SerializeField]
	private int beaconIndex;

	public override bool Evaluate()
	{
		return BeaconManager.GetBeaconUnlocked(beaconID, beaconIndex);
	}
}
