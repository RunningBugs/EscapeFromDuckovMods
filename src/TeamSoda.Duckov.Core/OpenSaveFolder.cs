using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class OpenSaveFolder : MonoBehaviour
{
	private string filePath => Application.persistentDataPath;

	private void Update()
	{
		if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.lKey.isPressed)
		{
			OpenFolder();
		}
	}

	public void OpenFolder()
	{
		Process.Start(new ProcessStartInfo
		{
			FileName = filePath,
			UseShellExecute = true
		});
	}
}
