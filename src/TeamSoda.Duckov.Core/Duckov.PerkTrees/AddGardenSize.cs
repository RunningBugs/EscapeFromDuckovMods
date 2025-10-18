using Duckov.Crops;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.PerkTrees;

public class AddGardenSize : PerkBehaviour, IGardenSizeAdder
{
	[LocalizationKey("Default")]
	[SerializeField]
	private string descriptionFormatKey = "PerkBehaviour_AddGardenSize";

	[SerializeField]
	private string gardenID = "Default";

	[SerializeField]
	private Vector2Int add;

	public override string Description => descriptionFormatKey.ToPlainText().Format(new
	{
		addX = add.x,
		addY = add.y
	});

	protected override void OnUnlocked()
	{
		Garden.Register(this);
	}

	protected override void OnOnDestroy()
	{
		Garden.Unregister(this);
	}

	public Vector2Int GetValue(string gardenID)
	{
		if (gardenID != this.gardenID)
		{
			return default(Vector2Int);
		}
		return add;
	}
}
