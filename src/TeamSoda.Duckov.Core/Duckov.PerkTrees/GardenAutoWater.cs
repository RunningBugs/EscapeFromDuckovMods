using Duckov.Crops;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.PerkTrees;

public class GardenAutoWater : PerkBehaviour, IGardenAutoWaterProvider
{
	[SerializeField]
	[LocalizationKey("Default")]
	private string descriptionKey = "PerkBehaviour_GardenAutoWater";

	[SerializeField]
	private string gardenID = "Default";

	public override string Description => descriptionKey.ToPlainText();

	protected override void OnUnlocked()
	{
		Garden.Register(this);
	}

	protected override void OnOnDestroy()
	{
		Garden.Unregister(this);
	}

	public bool TakeEffect(string gardenID)
	{
		return gardenID == this.gardenID;
	}
}
