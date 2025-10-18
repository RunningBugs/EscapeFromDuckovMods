using ItemStatsSystem;
using SodaCraft.Localizations;

namespace Duckov.ItemUsage;

[MenuPath("食物/食物")]
public class FoodDrink : UsageBehavior
{
	public float energyValue;

	public float waterValue;

	[LocalizationKey("Default")]
	public string energyKey = "Usage_Energy";

	[LocalizationKey("Default")]
	public string waterKey = "Usage_Water";

	public float UseDurability;

	public override DisplaySettingsData DisplaySettings
	{
		get
		{
			DisplaySettingsData result = new DisplaySettingsData
			{
				display = true
			};
			if (energyValue != 0f && waterValue != 0f)
			{
				result.description = energyKey.ToPlainText() + ": " + energyValue + "  " + waterKey.ToPlainText() + ": " + waterValue;
			}
			else if (energyValue != 0f)
			{
				result.description = energyKey.ToPlainText() + ": " + energyValue;
			}
			else
			{
				result.description = waterKey.ToPlainText() + ": " + waterValue;
			}
			return result;
		}
	}

	public override bool CanBeUsed(Item item, object user)
	{
		if (!(user as CharacterMainControl))
		{
			return false;
		}
		return true;
	}

	protected override void OnUse(Item item, object user)
	{
		CharacterMainControl characterMainControl = user as CharacterMainControl;
		if ((bool)characterMainControl)
		{
			Eat(characterMainControl);
			if (UseDurability > 0f && item.UseDurability)
			{
				item.Durability -= UseDurability;
			}
		}
	}

	private void Eat(CharacterMainControl character)
	{
		if (energyValue != 0f)
		{
			character.AddEnergy(energyValue);
		}
		if (waterValue != 0f)
		{
			character.AddWater(waterValue);
		}
	}
}
