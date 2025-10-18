using UnityEngine;

namespace Duckov.MiniGames.Debugging;

public class HideAndLockCursor : MonoBehaviour
{
	private void OnEnable()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void OnDisable()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			base.enabled = false;
		}
	}
}
