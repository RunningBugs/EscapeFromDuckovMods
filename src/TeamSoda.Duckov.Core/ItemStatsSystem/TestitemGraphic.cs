using UnityEngine;
using UnityEngine.InputSystem;

namespace ItemStatsSystem;

public class TestitemGraphic : MonoBehaviour
{
	private ItemGraphicInfo instance;

	private void Start()
	{
	}

	private void Update()
	{
		if (Keyboard.current.gKey.wasPressedThisFrame)
		{
			if ((bool)instance)
			{
				Object.Destroy(instance.gameObject);
			}
			DuckovItemAgent currentHoldItemAgent = CharacterMainControl.Main.CurrentHoldItemAgent;
			if ((bool)currentHoldItemAgent)
			{
				instance = ItemGraphicInfo.CreateAGraphic(currentHoldItemAgent.Item, base.transform);
			}
		}
	}
}
