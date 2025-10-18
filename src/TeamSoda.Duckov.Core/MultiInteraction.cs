using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MultiInteraction : MonoBehaviour
{
	[SerializeField]
	private List<InteractableBase> interactables;

	public ReadOnlyCollection<InteractableBase> Interactables => interactables.AsReadOnly();

	private void OnTriggerEnter(Collider other)
	{
		if (CharacterMainControl.Main.gameObject == other.gameObject)
		{
			MultiInteractionMenu.Instance?.SetupAndShow(this).Forget();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (CharacterMainControl.Main.gameObject == other.gameObject)
		{
			MultiInteractionMenu.Instance?.Hide().Forget();
		}
	}
}
