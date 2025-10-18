using Duckov.Scenes;
using Duckvo.Beacons;
using UnityEngine;

public class TeleportBeacon : MonoBehaviour
{
	[SceneID]
	public string beaconScene;

	public int beaconIndex;

	public GameObject activeByUnlocked;

	public InteractableBase interactable;

	private void Start()
	{
		bool beaconUnlocked = BeaconManager.GetBeaconUnlocked(beaconScene, beaconIndex);
		activeByUnlocked.SetActive(beaconUnlocked);
		interactable.gameObject.SetActive(!beaconUnlocked);
	}

	public void ActivateBeacon()
	{
		BeaconManager.UnlockBeacon(beaconScene, beaconIndex);
		activeByUnlocked.SetActive(value: true);
		interactable.gameObject.SetActive(value: false);
	}
}
