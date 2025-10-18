using Duckov.Buildings;
using Duckov.Buildings.UI;
using UnityEngine;

public class BuilderViewInvoker : InteractableBase
{
	[SerializeField]
	private BuildingArea buildingArea;

	protected override void OnInteractFinished()
	{
		if (!(buildingArea == null))
		{
			BuilderView.Show(buildingArea);
		}
	}
}
