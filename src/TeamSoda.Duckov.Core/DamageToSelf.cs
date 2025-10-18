using UnityEngine;

public class DamageToSelf : MonoBehaviour
{
	public DamageInfo dmg;

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.K))
		{
			dmg.fromCharacter = CharacterMainControl.Main;
			CharacterMainControl.Main.Health.Hurt(dmg);
		}
		if (Input.GetKeyDown(KeyCode.L))
		{
			float value = CharacterMainControl.Main.CharacterItem.GetStat("InventoryCapacity").Value;
			Debug.Log($"InventorySize:{value}");
		}
	}
}
