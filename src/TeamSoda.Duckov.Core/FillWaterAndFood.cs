using UnityEngine;

public class FillWaterAndFood : MonoBehaviour
{
	public float water;

	public float food;

	public void Fill()
	{
		CharacterMainControl main = CharacterMainControl.Main;
		if ((bool)main)
		{
			main.AddWater(water);
			main.AddEnergy(food);
		}
	}
}
