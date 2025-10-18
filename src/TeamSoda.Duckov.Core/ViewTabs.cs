using Duckov.UI;
using Duckov.UI.Animations;
using UnityEngine;

public class ViewTabs : MonoBehaviour
{
	[SerializeField]
	private FadeGroup fadeGroup;

	public void Show()
	{
		fadeGroup.Show();
	}

	public void Hide()
	{
		fadeGroup.Hide();
	}

	private void Update()
	{
		if (fadeGroup.IsShown && View.ActiveView == null)
		{
			Hide();
		}
	}
}
