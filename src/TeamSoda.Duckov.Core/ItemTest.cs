using ItemStatsSystem;
using UnityEngine;

public class ItemTest : MonoBehaviour
{
	public Item characterTemplate;

	public Item swordTemplate;

	public Item characterInstance;

	public Item swordInstance;

	public void DoInstantiate()
	{
		characterInstance = characterTemplate.CreateInstance();
		swordInstance = swordTemplate.CreateInstance();
	}

	public void EquipSword()
	{
		characterInstance.Slots["Weapon"].Plug(swordInstance, out var _);
	}

	public void UequipSword()
	{
		characterInstance.Slots["Weapon"].Unplug();
	}

	public void DestroyInstances()
	{
		if ((bool)characterInstance)
		{
			characterInstance.DestroyTreeImmediate();
		}
		if ((bool)swordInstance)
		{
			swordInstance.DestroyTreeImmediate();
		}
	}
}
