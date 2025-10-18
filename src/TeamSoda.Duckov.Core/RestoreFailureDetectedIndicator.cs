using Duckov.UI.Animations;
using Saves;
using UnityEngine;

public class RestoreFailureDetectedIndicator : MonoBehaviour
{
	[SerializeField]
	private FadeGroup fadeGroup;

	private void OnEnable()
	{
		SavesSystem.OnRestoreFailureDetected += Refresh;
		SavesSystem.OnSetFile += Refresh;
		Refresh();
	}

	private void OnDisable()
	{
		SavesSystem.OnRestoreFailureDetected -= Refresh;
		SavesSystem.OnSetFile -= Refresh;
	}

	private void Refresh()
	{
		if (SavesSystem.RestoreFailureMarker)
		{
			fadeGroup.Show();
		}
		else
		{
			fadeGroup.Hide();
		}
	}
}
