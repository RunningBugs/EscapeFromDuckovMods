using UnityEngine;

public class FishingPoint : MonoBehaviour
{
	public InteractableBase Interactable;

	public Action_Fishing action;

	public Transform playerPoint;

	private void Awake()
	{
		OnPlayerTakeFishingRod(null);
		Interactable.OnInteractFinishedEvent.AddListener(OnInteractFinished);
	}

	private void OnDestroy()
	{
		if ((bool)Interactable)
		{
			Interactable.OnInteractFinishedEvent.RemoveListener(OnInteractFinished);
		}
	}

	private void OnPlayerTakeFishingRod(FishingRod rod)
	{
	}

	private void OnInteractFinished(CharacterMainControl character, InteractableBase interact)
	{
		if ((bool)character)
		{
			character.SetPosition(playerPoint.position);
			character.SetAimPoint(playerPoint.position + playerPoint.forward * 10f);
			character.movementControl.SetAimDirection(playerPoint.forward);
			character.StartAction(action);
		}
	}
}
