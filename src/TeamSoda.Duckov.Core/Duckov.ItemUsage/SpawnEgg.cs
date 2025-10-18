using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.ItemUsage;

public class SpawnEgg : UsageBehavior
{
	public Egg eggPrefab;

	public CharacterRandomPreset spawnCharacter;

	public float eggSpawnDelay = 2f;

	[LocalizationKey("Default")]
	public string descriptionKey = "Usage_SpawnEgg";

	public override DisplaySettingsData DisplaySettings => new DisplaySettingsData
	{
		display = true,
		description = (descriptionKey.ToPlainText() ?? "")
	};

	public override bool CanBeUsed(Item item, object user)
	{
		return true;
	}

	protected override void OnUse(Item item, object user)
	{
		CharacterMainControl characterMainControl = user as CharacterMainControl;
		if (!(characterMainControl == null))
		{
			Egg egg = Object.Instantiate(eggPrefab, characterMainControl.transform.position, Quaternion.identity);
			Collider component = egg.GetComponent<Collider>();
			Collider component2 = characterMainControl.GetComponent<Collider>();
			if ((bool)component && (bool)component2)
			{
				Debug.Log("关掉角色和蛋的碰撞");
				Physics.IgnoreCollision(component, component2, ignore: true);
			}
			egg.Init(characterMainControl.transform.position, characterMainControl.CurrentAimDirection * 1f, characterMainControl, spawnCharacter, eggSpawnDelay);
		}
	}
}
