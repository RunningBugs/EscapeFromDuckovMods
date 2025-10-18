using Duckov.Buffs;
using Duckov.Scenes;
using UnityEngine;

public class StormWeather : MonoBehaviour
{
	public Buff buff;

	public float addBuffTimeSpace = 1f;

	private float addBuffTimer;

	private CharacterMainControl target;

	private bool onlyOutDoor = true;

	public float stormProtectionThreshold = 0.9f;

	private void Update()
	{
		if (!LevelManager.LevelInited)
		{
			return;
		}
		SubSceneEntry subSceneInfo = MultiSceneCore.Instance.GetSubSceneInfo();
		if (onlyOutDoor && subSceneInfo.IsInDoor)
		{
			return;
		}
		if (!target)
		{
			target = CharacterMainControl.Main;
			if (!target)
			{
				return;
			}
		}
		addBuffTimer -= Time.deltaTime;
		if (addBuffTimer <= 0f)
		{
			addBuffTimer = addBuffTimeSpace;
			if (!(target.StormProtection > stormProtectionThreshold))
			{
				target.AddBuff(buff);
			}
		}
	}
}
